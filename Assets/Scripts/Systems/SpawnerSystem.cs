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
    public Random Random;
    public EntityQuery query;

    public void OnCreate(ref SystemState state)
    {
        Random = new Random((uint)state.WorldUnmanaged.Time.ElapsedTime + 1234);
        query = state.GetEntityQuery(ComponentType.ReadOnly<Soldier>());
    }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    { 
        var count = query.CalculateEntityCount();
        foreach (var (spawner, spawnerLocalToWorld, entity) in
                 SystemAPI.Query<RefRO<Spawner>, RefRO<LocalToWorld>>().WithEntityAccess())

        {
            if(count/2 >= spawner.ValueRO.Count) continue;
            var entities =
                CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(spawner.ValueRO.Count,
                    ref state.WorldUnmanaged.UpdateAllocator);
            state.EntityManager.Instantiate(spawner.ValueRO.Prefab, entities);
            var setEntityPosition = new SetEntityPosition
            {
                random = Random, circle = spawner.ValueRO.initialRadius
            };
            setEntityPosition.ScheduleParallel();
        }
    }
    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}

[BurstCompile]
[WithAll(typeof(Soldier))]
partial struct SetEntityPosition: IJobEntity
{
    public Random random;
    public int circle;
    
    [BurstCompile]
    public void Execute( ref LocalTransform transform, in Soldier soldier )
    {
        // Generate a random angle in radians
        float randomAngle = random.NextFloat(0, 2 * math.PI);
        float randomRadius = random.NextFloat(0, circle);
        float x = soldier.initialPos.x + randomRadius * math.cos(randomAngle);
        float z = soldier.initialPos.z + randomRadius * math.sin(randomAngle);
        transform.Position = new float3(x,0,z);
    }
}