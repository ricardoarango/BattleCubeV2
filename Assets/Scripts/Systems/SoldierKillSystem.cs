using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DefaultNamespace.Systems
{
    public partial struct SoldierKillSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            new JobCheckContact
            {
                AllTranslation = SystemAPI.GetComponentLookup<LocalTransform>(),
                EntityCommandBuffer = ecb.AsParallelWriter()
                //job.aliveFromEntity = GetComponentDataFromEntity<SoldierAlive>();
            }.ScheduleParallel();

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
    
    [BurstCompile]
    [WithAll(typeof(Target))]
    [WithAll(typeof(Soldier))]
    public partial struct JobCheckContact : IJobEntity
    {
        [ReadOnly]
        //[NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<LocalTransform> AllTranslation;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        
        // disable native restriction ?
        //[NativeDisableParallelForRestriction]
        //public ComponentDataFromEntity<SoldierAlive> aliveFromEntity;
        
        public void Execute( [EntityIndexInChunk]int index, Entity entity, 
            [ReadOnly]ref LocalTransform translation, 
            [ReadOnly]ref Target target)
        {
            if (AllTranslation.HasComponent(target.Value))
            {
                var targetPos = AllTranslation[target.Value].Position;
                if (math.distance(translation.Position, targetPos ) < 0.5f)
                {
                    EntityCommandBuffer.DestroyEntity(index, target.Value);
                    EntityCommandBuffer.RemoveComponent<Target>( index, entity );
                    //aliveFromEntity[target.Value] = new SoldierAlive{Value = 0};
                }
            }
        }
    }
}