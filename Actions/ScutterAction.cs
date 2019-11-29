using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Action_Scutter", menuName = "AI/Actions/Scutter")]
    public class ScutterAction : AIAction
    {
        public float radius;
        public override void Act(AIController controller)
        {
            // Returns if controller is not on a navMesh
            if (!controller.Agent.isOnNavMesh)
                return;

            // Gets a random position on a circle of set radius
            Vector2 movementOffset = Random.insideUnitCircle.normalized * radius;
            Vector3 finalPos;
            NavMeshHit[] hits = new NavMeshHit[4];
            // Gets four NavMeshHits from controller's position plus movementOffset, rotated by i * 90 degrees
            // This helps keep AI from getting stuck in corners or against walls
            for (int i = 0; i < 4; i++)
            {
                finalPos = controller.transform.position + new Vector3(movementOffset.x, 0, movementOffset.y);
                // NavMeshAgent Raycast to find the distance before colliding with edges of navmesh
                controller.Agent.Raycast(finalPos, out hits[i]);

                // Rotates movementOffset by 90 degrees
                float x = movementOffset.x;
                movementOffset.x = -movementOffset.y;
                movementOffset.y = x;
            }

            // Uses the distances before the NavMeshRaycast to get a weighted random path to a location
            NavMeshPath path = GetWeightedPath(hits, controller);
            // If path found, sets agent path
            if (path != null && path.status != NavMeshPathStatus.PathInvalid)
            {
                controller.Agent.SetPath(path);
            }
        }

        NavMeshPath GetWeightedPath(NavMeshHit[] hits, AIController controller)
        {
            float weightTotal = 0;
            NavMeshPath path = new NavMeshPath();

            // Calculate total weight from NavMeshHit distances
            foreach (NavMeshHit hit in hits)
            {
                weightTotal += hit.distance;
            }

            // Copies list
            List<NavMeshHit> values = new List<NavMeshHit>(hits);
            int selectedValue = -1;

            while (values.Count > 0 && path.status == NavMeshPathStatus.PathInvalid)
            {
                // Calculate random weight
                float randomWeight = Random.Range(0, weightTotal);

                // Find value corresponding to that weight
                for (int i = 0; i < values.Count; i++)
                {
                    NavMeshHit hit = hits[i];
                    if (randomWeight <= hit.distance)
                    {
                        // If found a path sets selectedValue and breaks out of for loop
                        if (controller.Agent.CalculatePath(hit.position, path))
                        {
                            selectedValue = i;
                        }
                        break;
                    }
                    // Otherwise, reduces randomWeight by the hits distance
                    else randomWeight -= hit.distance;
                }

                // If a path was invalid
                if (path.status == NavMeshPathStatus.PathInvalid)
                {
                    // Reduces total weight, resets path and removes point from list of values
                    weightTotal -= values[selectedValue].distance;
                    path = null;
                    values.RemoveAt(selectedValue);
                    selectedValue = -1;
                }

            }

            return path;
        }
    }
}