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
                InitialRadius = authoring.initialRadius,
                Team = authoring.team
            });
        }
    }
}

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public int Count;
    public int InitialRadius;
    public int Team;
    //public Entity PrefabBlueTeam;
    //public float3 SpawnPosition;
}
