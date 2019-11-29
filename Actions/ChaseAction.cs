using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Action_Chase", menuName = "AI/Actions/Chase")]
    public class ChaseAction : AIAction
    {
        public override void Act(AIController controller)
        {
            NavMeshPath path = new NavMeshPath();
            // If controller has a target
            if (controller.target.Value != null)
            {
                // Calculate and set a path toward them
                if (controller.Agent.isOnNavMesh && !controller.Agent.isOnOffMeshLink && controller.Agent.CalculatePath(controller.target.Value.position, path))
                {
                    controller.Agent.SetPath(path);
                }
            }
        }
    }
}
