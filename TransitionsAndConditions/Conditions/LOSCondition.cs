using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName = "AI/Conditions/Line Of Sight")]
    public class LOSCondition : AICondition
    {
        public LayerMask blockLayers;
        public override bool Decide(AIController controller)
        {
            if (controller.target.Value == null)
                return false;

            // Calculate direction from controller to target
            Vector3 position = controller.transform.position;
            Vector3 targPosition = controller.target.Value.position;
            Vector3 dir = (targPosition - position).normalized;
            
            // Raycasts between controller and target and returns if there are no objects in between
            return !Physics.Raycast(position, dir, Vector3.Distance(position, targPosition), blockLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
