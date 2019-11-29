using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CardSlinger.AI
{
    // Attempts to move a set distance, while remaining the same distance from the target
    [CreateAssetMenu(menuName = "AI/Actions/Strafe")]
    public class StrafeAction : AIAction
    {
        public float moveDistance = 3;

        public override void Act(AIController controller)
        {
            if (controller.target.Value) {

                Vector3 targetPosition = controller.target.Value.position;
                // Get target's position, ignoring y
                Vector2 targetPlanePosition = new Vector2(targetPosition.x, targetPosition.z);
                // Get controller's position, ignoring y
                Vector2 planePosition = new Vector2(controller.transform.position.x, controller.transform.position.z);
                // Calculate the vector from controller to target, ignoring y
                Vector2 relativePosition = (targetPlanePosition - planePosition);
                float distanceSqr = relativePosition.sqrMagnitude;

                // Calculates angle from target to move
                float angle = Mathf.Acos((2 * distanceSqr - moveDistance * moveDistance) / (2 * distanceSqr));

                // Randomise direction
                angle *= (Random.Range(0, 2) == 0) ? 1 : -1;

                // A vector storing the target's position, but with the controller's height
                Vector3 relTargetPos = new Vector3(targetPlanePosition.x, controller.transform.position.y - controller.Agent.baseOffset, targetPlanePosition.y);

                // Rotates the angle relative position by both position and negative of calculated angle
                Vector2[] rotatedPosition = { RotateByAngle(relativePosition, angle), RotateByAngle(relativePosition, -angle) };
                // And use these rotated directions to calculate new positions
                Vector3[] finalPosition = { relTargetPos - new Vector3(rotatedPosition[0].x, 0, rotatedPosition[0].y),
                    relTargetPos - new Vector3(rotatedPosition[1].x, 0, rotatedPosition[1].y) };

                // Try to calculate a path for both directions
                NavMeshPath path = new NavMeshPath();
                for (int i = 0; i < 2; i++)
                {
                    // Calculates and sets path
                    if (controller.Agent.CalculatePath(finalPosition[i], path) && path.status != NavMeshPathStatus.PathInvalid)
                    {
                        controller.Agent.SetPath(path);
                        return;
                    }
                }

            }
        }

        // Helper function to rotate a vector2 by an angle
        protected Vector2 RotateByAngle(Vector2 vector, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector2 rotatedPosition = new Vector2(
                cos * vector.x - sin * vector.y,
                sin * vector.x + cos * vector.y);

            return rotatedPosition;
        }
    }
}