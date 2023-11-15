using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct SoldierMovSystem : ISystem
{
    private ComponentLookup<LocalTransform> m_AllSoldiers;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_AllSoldiers = SystemAPI.GetComponentLookup<LocalTransform>(true);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //use cache value
        m_AllSoldiers.Update(ref state);
        
        // here another method to ge teh query using query builder 
        //var spinningCubesQuery = SystemAPI.QueryBuilder().WithAll<RotationSpeed>().Build();
        
        new SoldierOrientationJob
        {
            AllPositions = m_AllSoldiers 
            
        }.Schedule();
    
        new SoldierMovSystemJob
        {
            DeltaTime = Time.deltaTime
        }.Schedule(  );
    
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

[WithAll(typeof(LocalTransform))]
[WithAll(typeof(SoldierOrientation))]
[WithAll(typeof(Target))]
[BurstCompile]
public partial struct SoldierOrientationJob : IJobEntity
{
    [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
    [ReadOnly] public ComponentLookup<LocalTransform> AllPositions;

    public void Execute([ReadOnly]ref LocalTransform translation, [ReadOnly]ref Target target, ref SoldierOrientation soldierOrientation)
    {
        if (AllPositions.HasComponent(target.Value))
        {
            var src = translation.Position;
            var dst = AllPositions[target.Value].Position;
            // just store the orientation
            soldierOrientation.Value = math.normalizesafe(dst - src);
        }
        else
        {
            // prevent to keep moving if the target is destroyed
            soldierOrientation.Value = float3.zero;
        }
    }
}

[BurstCompile]
[WithAll(typeof(Soldier))]
public partial struct SoldierMovSystemJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(ref LocalTransform translation, [ReadOnly]ref SoldierOrientation soldierOrientation)
    {
        translation.Position += soldierOrientation.Value * DeltaTime * 10;
    }
}



