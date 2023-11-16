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
    private bool initialized;
    private JobHandle jobHandle1;
    private JobHandle jobHandle2;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
        Random = new Random((uint)state.WorldUnmanaged.Time.ElapsedTime + 1234);
    }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var soldierQuery = SystemAPI.QueryBuilder().WithAll<Soldier>().Build();
        if (soldierQuery.IsEmpty)
        {
            foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
            {
                var entities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(spawner.ValueRO.Count, ref state.WorldUnmanaged.UpdateAllocator);
                // Create a lot of entities ! 
                state.EntityManager.Instantiate(spawner.ValueRO.Prefab, entities);
            }
        }
        else
        {
            if(!initialized)
            {
                foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
                {
                    var setEntityPosition = new SetEntityPositionJob
                    {
                        random = Random, 
                        circle = spawner.ValueRO.initialRadius
                    };

                    // Create a query that includes both the Solider and the LocalTransform components
                    var newSoldierQuery = new EntityQuery();
                    if(spawner.ValueRO.team == 0){
                        newSoldierQuery = SystemAPI.QueryBuilder().WithAll<Soldier, LocalTransform, SoldierTeamA>().Build();
                        setEntityPosition.ScheduleParallel (newSoldierQuery);
                    }
                    else {
                        newSoldierQuery = SystemAPI.QueryBuilder().WithAll<Soldier, LocalTransform, SoldierTeamB>().Build();
                        setEntityPosition.ScheduleParallel (newSoldierQuery);
                    }
                }
                initialized = true;
            }
            if(jobHandle1.IsCompleted/* && jobHandle2.IsCompleted*/)
                state.Enabled = false;
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
[WithAll(typeof(LocalTransform))]
partial struct SetEntityPositionJob: IJobEntity
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