using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class SoldierAuthoringAuthoring : MonoBehaviour
    {
        public Vector3 initialPos;
        public int team;
        private class SoldierAuthoringBaker : Baker<SoldierAuthoringAuthoring>
        {
            public override void Bake(SoldierAuthoringAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new Soldier
                {
                    initialPos = authoring.initialPos
                });
            }
        }
    }
}