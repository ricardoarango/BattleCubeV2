using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace.Systems
{
    public partial class TargetDebugSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            var allEnitities = GetComponentLookup<LocalTransform>(true);
            Entities.ForEach((Entity entity, ref Target target) =>
            {
                // if the entity was killed
               // if (allEnitities.Exists(target.Value))
                {
                    var entityTranslation = allEnitities[entity];
                    var targetTranslation = allEnitities[target.Value];
                    Debug.DrawLine(entityTranslation.Position, targetTranslation.Position);
                }
            }).Run();
        }
    }
}



