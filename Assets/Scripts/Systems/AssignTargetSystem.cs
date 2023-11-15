using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct AssignTargetSystem : ISystem
{
    public EntityQuery m_TeamAQuery;
    public EntityQuery m_TeamBQuery;
    public EntityQuery teamANoTarget;
    public EntityQuery teamBNoTarget;
    public Random m_Random;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_TeamAQuery = state.GetEntityQuery(ComponentType.ReadOnly<SoldierTeamA>());
        m_TeamBQuery = state.GetEntityQuery(ComponentType.ReadOnly<SoldierTeamB>());
        
        var typesQueryA = new NativeArray<ComponentType>(2, Allocator.Temp)
        {
            [0] = ComponentType.Exclude<Target>(),
            [1] = ComponentType.ReadOnly<SoldierTeamA>()
        };
        var typesQueryB = new NativeArray<ComponentType>(2, Allocator.Temp)
        {
            [0] = ComponentType.Exclude<Target>(),
            [1] = ComponentType.ReadOnly<SoldierTeamB>()
        };

        teamANoTarget = state.GetEntityQuery( typesQueryA );
        teamBNoTarget = state.GetEntityQuery( typesQueryB );
        m_Random = new Random((uint)state.WorldUnmanaged.Time.ElapsedTime + 1234);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        //todo revisar si esto es necesario ponerlo aca o en el onCreate
        /*var typesQueryA = new NativeArray<ComponentType>(2, Allocator.Temp)
        {
            [0] = ComponentType.Exclude<Target>(),
            [1] = ComponentType.ReadOnly<SoldierTeamA>()
        };
        var typesQueryB = new NativeArray<ComponentType>(2, Allocator.Temp)
        {
            [0] = ComponentType.Exclude<Target>(),
            [1] = ComponentType.ReadOnly<SoldierTeamB>()
        };
        teamANoTarget = state.GetEntityQuery( typesQueryA );
        teamBNoTarget = state.GetEntityQuery( typesQueryB );
        
        m_TeamAQuery = state.GetEntityQuery(ComponentType.ReadOnly<SoldierTeamA>());
        m_TeamBQuery = state.GetEntityQuery(ComponentType.ReadOnly<SoldierTeamB>());
        */
        
        
        var ecb = GetEntityCommandBuffer(ref state);
        if (teamANoTarget.CalculateEntityCount() > 0)
        {
            new AssignTargetJob
            {
                PotentialTargets = m_TeamBQuery.ToEntityArray(Allocator.TempJob),
                Random = m_Random,
                CommandBuffer = ecb 
            }.ScheduleParallel(teamANoTarget);
        }
        if (teamBNoTarget.CalculateEntityCount() > 0)
        {
            new AssignTargetJob
            {
                PotentialTargets = m_TeamAQuery.ToEntityArray(Allocator.TempJob),
                Random = m_Random,
                CommandBuffer = ecb
            }.ScheduleParallel(teamBNoTarget);
        }

    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
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
public partial struct AssignTargetJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter CommandBuffer;
    [ReadOnly, DeallocateOnJobCompletion] 
    public NativeArray<Entity> PotentialTargets;
    public Random Random;
    
    public void Execute([EntityIndexInQuery] int entityIndexInQuery, Entity entity)
    {
        Target target = new Target();
        int n = Random.NextInt(0, PotentialTargets.Length - 1);
        target.Value = PotentialTargets[n];
        CommandBuffer.AddComponent(entityIndexInQuery, entity , target );
    }

} 
