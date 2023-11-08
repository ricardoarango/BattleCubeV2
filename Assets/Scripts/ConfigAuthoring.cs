using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class ConfigAuthoringAuthoring : MonoBehaviour
{   
    public GameObject prefab;
    public int count;
    public int initialRadius;
    public int team;
    // Create an entity that has the prefabs 
    private class ConfigAuthoringBaker : Baker<ConfigAuthoringAuthoring>
    {
        public override void Bake(ConfigAuthoringAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new Spawner
            {
                Prefab  = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                Count = authoring.count,
                initialRadius = authoring.initialRadius
            });
        }
    }
}

public struct Soldier: IComponentData
{
    public float3 initialPos;
}

// used for now for filtering 
public struct SoldierTeamA: IComponentData
{
}
public struct SoldierTeamB: IComponentData
{
}
public struct Target : IComponentData
{
    public Entity Value;
}
public struct SoldierOrientation : IComponentData
{
    public float3 Value;
}

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public int Count;
    public int initialRadius;
}
