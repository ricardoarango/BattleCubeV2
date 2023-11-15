using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public partial struct SoldierKillSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //var ecbSystem = World.GetOrCreateSystemManaged<NonSingletonECBSystem>();
        //var ecbSystem = state.
        
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        //var ecb = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW
        //    .CreateCommandBuffer(state.WorldUnmanaged);
            
        
        var allTranslation =  SystemAPI.GetComponentLookup<LocalTransform>();
        var soldierContactJob = new SoldierContactJob
        {
            AllTranslation  =  allTranslation,
            aliveFromEntity =  SystemAPI.GetComponentLookup<SoldierAlive>()
        };
        
        // Job that remove the target  
        var removeInvalidTargetsJob = new RemoveInvalidTargets
        {
            CommandBuffer = ecb.AsParallelWriter(),
            PositionFromEntity = allTranslation
        };
        // Job that Destroy entities
        var removeDeadSoldiersJob = new RemoveDeadSoldiers
        {
            CommandBuffer = ecb.AsParallelWriter()
        };

        state.Dependency = soldierContactJob.Schedule(state.Dependency);
        state.Dependency = removeInvalidTargetsJob.Schedule(state.Dependency);
        state.Dependency = removeDeadSoldiersJob.Schedule(state.Dependency);
        
        
        


    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

[BurstCompile]
[WithAll(typeof(Target))]
public partial struct SoldierContactJob : IJobEntity
{
    [ReadOnly]
    //[NativeDisableParallelForRestriction]
    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<LocalTransform> AllTranslation;
    
    // if other thread mark this variable as 0 is ok  
    [NativeDisableParallelForRestriction]
    public ComponentLookup<SoldierAlive> aliveFromEntity;
    
    public void Execute( [EntityIndexInChunk]int index, Entity entity, 
        [ReadOnly]ref LocalTransform translation, 
        [ReadOnly]ref Target target)
    {
        if (AllTranslation.HasComponent(target.Value))
        {
            var targetPos = AllTranslation[target.Value].Position;
            if (math.distance(translation.Position, targetPos ) < 1 )
            {
                aliveFromEntity[target.Value] = new SoldierAlive { Value = 0 };
            }
        }
    }
}

// Remove targets.
// Retrieve all entities with target and check if that entity still exist 
[BurstCompile]
[WithAll(typeof(Target))]
public partial struct RemoveInvalidTargets : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter CommandBuffer;
    [NativeDisableContainerSafetyRestriction]
    [ReadOnly] public ComponentLookup<LocalTransform> PositionFromEntity;
    public void Execute([EntityIndexInChunk]int index, Entity entity,  ref Target target)
    {
        if(!PositionFromEntity.HasComponent(target.Value))
            CommandBuffer.RemoveComponent<Target>(index, entity);
    }
}

// use the SoldierAlive value to Destroy the entities
[BurstCompile]
[WithAll(typeof(SoldierAlive))]
public partial struct RemoveDeadSoldiers : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter CommandBuffer;
    public void Execute([EntityIndexInChunk]int index, Entity entity, ref SoldierAlive alive)
    {
        if (alive.Value == 0)
        {
            CommandBuffer.DestroyEntity(index, entity);
        }
    }
}
