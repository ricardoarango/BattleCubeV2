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
                
                AddComponent(entity, new SoldierOrientation());
                // TODO remove Soldier component 
                // add others Component 
                if (authoring.team == 0)
                {
                    AddComponent(entity, new SoldierTeamA()
                    {
                    });
                }
                if (authoring.team == 1)
                {
                    AddComponent(entity, new SoldierTeamB()
                    {
                    });
                }
            }
        }
    }
}