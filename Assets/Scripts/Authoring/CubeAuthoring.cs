using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class CubeAuthoringAuthoring : MonoBehaviour
    {
        private class CubeAuthoringBaker : Baker<CubeAuthoringAuthoring>
        {
            public override void Bake(CubeAuthoringAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TestCube
                {
                });
            }
        }
    }
}
public struct TestCube : IComponentData
{
}