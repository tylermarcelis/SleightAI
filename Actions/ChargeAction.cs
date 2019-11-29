using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName = "AI/Actions/Charge")]
    public class ChargeAction : AIAction
    {
        public float chargeOvershoot = 3;
        public override void Act(AIController controller)
        {
            NavMeshPath path = new NavMeshPath();
            // If controller has a target
            if (controller.target.Value != null)
            {
                // Get the x,z direction from the AIController to their target
                Vector3 targetPosition = controller.transform.position;
                Vector3 direction = controller.target.Value.position - targetPosition;
                direction.y = 0;
                direction.Normalize();

                // Find a position a set distance behind the target to charge to
                Vector3 endPos = targetPosition + direction * chargeOvershoot;
                NavMeshHit hit = new NavMeshHit();
                // NavMeshAgent raycast to find any objects hit on this path
                if (controller.Agent.Raycast(endPos, out hit))
                    endPos = hit.position;

                // Calculate and set path
                if (controller.Agent.isOnNavMesh && !controller.Agent.isOnOffMeshLink && controller.Agent.CalculatePath(endPos, path))
                {
                    controller.Agent.SetPath(path);
                }
            }
        }
    }
}