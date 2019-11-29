using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Action_Flee", menuName = "AI/Actions/Flee")]
    public class FleeAction : AIAction
    {
        public float fleeDistance;
        public int testCount = 10;
        public override void Act(AIController controller)
        {
            // If controller has a target
            if (controller.target.Value != null)
            {
                if (controller.Agent.isOnNavMesh)
                {
                    // Calculate a 'flee path'
                    NavMeshPath fleePath = FindFleePath(controller);
                    if (fleePath.status != NavMeshPathStatus.PathInvalid)
                    {
                        // And set path on NavMeshAgent
                        controller.Agent.SetPath(fleePath);
                    }
                }
            }
        }

        // Starting from behind the controller, attempts to find a location to flee to by calculating paths to points around the target
        NavMeshPath FindFleePath(AIController controller)
        {
            // Get the targets position, ignoring y
            Vector3 targetPos = controller.target.Value.position;
            targetPos.y = controller.transform.position.y;

            NavMeshPath path = new NavMeshPath();
            // Get direction from target to AIController
            Vector3 dir = controller.transform.position - targetPos;
            float distanceCheck = fleeDistance + dir.magnitude;
            dir.Normalize();
            // Calculate position behind controller from target
            Vector3 newPos = (targetPos + dir * distanceCheck);
            NavMeshHit hit;

            // If valid location
            if (NavMesh.SamplePosition(newPos, out hit, 1, controller.Agent.areaMask))
            {
                // Calculates and returns path
                controller.Agent.CalculatePath(hit.position, path);
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    return path;
                }
            }

            // Otherwise for a set number of tests
            for (int i = 1; i < testCount; i++)
            {
                float rotation = ((float)i / testCount) * Mathf.PI;

                // Checks both positive and negative rotations
                for (int j = -1; j <= 1; j += 2)
                {
                    // Rotates direction from target to controller by a rotation
                    float rot = rotation * j;
                    float cos = Mathf.Cos(rot);
                    float sin = Mathf.Sin(rot);
                    Vector3 newdir = new Vector3(dir.x * cos - dir.z * sin, 0, dir.x * sin + dir.z * cos);
                    newdir.Normalize();

                    // And finds position from that direction from the target
                    newPos = (targetPos + (newdir * distanceCheck));
                    // If valid location
                    if (NavMesh.SamplePosition(newPos, out hit, 1, controller.Agent.areaMask))
                    {
                        // Calculates and returns path
                        controller.Agent.CalculatePath(hit.position, path);
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            return path;
                        }
                    }
                }
            }

            // Otherwise tries directly opposite the controller from the target
            newPos = (targetPos + -dir * distanceCheck);
            // If valid locations
            if (NavMesh.SamplePosition(newPos, out hit, 1, controller.Agent.areaMask))
            {
                // Calculates and returns path
                controller.Agent.CalculatePath(hit.position, path);
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    return path;
                }
            }
            return path;
        }
    }
}
