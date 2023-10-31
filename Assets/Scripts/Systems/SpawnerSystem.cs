using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    { 
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var world = state.World.Unmanaged;
        
        foreach (var (spawner, spawnerLocalToWorld,entity) in
                                                   SystemAPI.Query<RefRO<Spawner>, RefRO<LocalToWorld>>().WithEntityAccess())
            
        {
            var entities =
                CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(spawner.ValueRO.Count, ref world.UpdateAllocator);
            
            state.EntityManager.Instantiate(spawner.ValueRO.Prefab, entities);
            var team = spawner.ValueRO.Team;
            var setLocalToWorldJob = new SetEntityLocalToWorld
            {
                LocalToWorldFromEntity = localToWorldLookup,
                Entities = entities,
                Center = spawnerLocalToWorld.ValueRO.Position,
                Radius = spawner.ValueRO.InitialRadius
            };

            state.Dependency = setLocalToWorldJob.Schedule(spawner.ValueRO.Count, 64, state.Dependency);
            state.Dependency.Complete();
            ecb.DestroyEntity(entity);
            
            /*EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
           new SoliderSpawnJob
           {
               team = team,
               count = 40,
               Ecb = ecb
           }.ScheduleParallel();*/
        }

        ecb.Playback(state.EntityManager);
        
        
    }
  
    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}

[BurstCompile]
struct SetEntityLocalToWorld : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
    public ComponentLookup<LocalToWorld> LocalToWorldFromEntity;
    public NativeArray<Entity> Entities;
    public float3 Center;
    public float Radius;

    public void Execute(int i)
    {
        
        var entity = Entities[i];
        var random = new Random(((uint)(entity.Index + i + 1) * 0x9F6ABC1));
        float2 randomPos = random.NextFloat2Direction();
        Debug.Log(randomPos);
        var pos = new float3(Center.x , 0, Center.z );
        var localToWorld = new LocalToWorld
        {
            Value = float4x4.TRS(pos, quaternion.identity, new float3(1.0f, 1.0f, 1.0f))
        };
        LocalToWorldFromEntity[entity] = localToWorld;

    }
}
    
public partial struct SoliderSpawnJob :  IJobEntity
{
    public int team;
    public EntityCommandBuffer.ParallelWriter Ecb;
    public int count;
    public void Execute([EntityIndexInQuery] int index, in Spawner spawner)
    {
        var teamLocation = team == 0? 1 : -1;
        float offset = 2f;
        for (int x = 0; x < count; x++)
        {
            for (int z = 0; z < count; z++)
            {
                var pos = new float3(x * offset , 0, z * teamLocation *offset );
                var newEntity = Ecb.Instantiate(index, spawner.Prefab);
                Ecb.SetComponent(index, newEntity, LocalTransform.FromPosition(pos));
            }
        }
    }
}
    