using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class ConfigAuthoringAuthoring : MonoBehaviour
{   
    public GameObject redTeam;
    public GameObject blueTeam;
    
    // Create an entity that has the prefabs 
    private class ConfigAuthoringBaker : Baker<ConfigAuthoringAuthoring>
    {
        public override void Bake(ConfigAuthoringAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Spawner
            {
                // By default, each authoring GameObject turns into an Entity.
                // Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
                PrefabRedTeam  = GetEntity(authoring.redTeam, TransformUsageFlags.Dynamic),
                PrefabBlueTeam = GetEntity(authoring.blueTeam, TransformUsageFlags.Dynamic),
                
                //https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/ecs-workflow-create-entities.html
                SpawnPosition = authoring.transform.position,
                NextSpawnTime = 0.0f,
                SpawnRate = 1
            });
        }
    }
}

public struct Spawner : IComponentData
{
    public Entity PrefabRedTeam;
    public Entity PrefabBlueTeam;
    public float3 SpawnPosition;
    public float NextSpawnTime;
    public float SpawnRate;
    
}
