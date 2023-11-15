using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace.Systems
{
    public partial class TargetDebugSystem : SystemBase
    {
        private bool _enabledSystem;
        private ComponentLookup<LocalTransform> allTransform;
         protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.D))
            {
                _enabledSystem = !_enabledSystem;
            }
            if (_enabledSystem)
            {
                allTransform = GetComponentLookup<LocalTransform>(true);
                foreach (var (target, entity) in SystemAPI.Query<RefRO<Target>>().WithEntityAccess())
                {
                    if (allTransform.HasComponent(target.ValueRO.Value))
                    {
                        var entityTranslation = allTransform[entity];
                        var targetTranslation = allTransform[target.ValueRO.Value];
                        Debug.DrawLine(entityTranslation.Position, targetTranslation.Position);
                    }
                }
            }
        }
    }
}



