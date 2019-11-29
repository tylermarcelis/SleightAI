using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Behaviour_Leap", menuName = "AI/Behaviours/Leap")]
    public class LeapBehaviour : AIBehaviour
    {
        [Range(0,90)]
        public float steepnessAngle = 45;
        //public float damageAmount;

        public override void Enter(AIController controller)
        {
            if (controller.target.Value != null)
            {
                // Disables NavMeshAgent
                controller.Agent.enabled = false;
                if (controller.Rigidbody)
                {
                    // Freezes rotation and unsets the rigidbody's isKinematic
                    controller.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                    controller.Rigidbody.isKinematic = false;

                    // Sets velocity
                    controller.Rigidbody.velocity = GetLeapVelocity(controller.transform.position, controller.target.Value.position);
                }
            }
        }

        public override void Exit(AIController controller)
        {
            if (controller.Rigidbody)
            {
                // Resets rigidbody
                controller.Rigidbody.isKinematic = true;
                controller.Rigidbody.constraints = RigidbodyConstraints.None;
                controller.Rigidbody.velocity = Vector3.zero;
            }
            // Enables NavMeshAgent
            controller.Agent.enabled = true;
        }

        protected Vector3 GetLeapVelocity(Vector3 initialPosition, Vector3 targetPosition)
        {
            // Gets angle in radians
            float angle = steepnessAngle * Mathf.Deg2Rad;
            float gravity = Mathf.Abs(Physics.gravity.y);

            // Get controller and target positions, ignoring y
            Vector3 planarTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
            Vector3 planarPosition = new Vector3(initialPosition.x, 0, initialPosition.z);

            // Get distance between planar positions
            float distance = Vector3.Distance(planarTarget, planarPosition);
            float yOffset = initialPosition.y - targetPosition.y;

            // If controller won't be able to reach location, based on angle
            // Limits it to prevent errors
            if (-yOffset > distance * Mathf.Tan(angle))
            {
                yOffset = distance * Mathf.Tan(angle);
            }

            // Calculates the velocity needed to reach the target, in the y direction using some funky maths
            float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
            Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

            // Rotates the calculated velocity to face the target
            float angleBetweenObjects = Vector3.SignedAngle(Vector3.forward, planarTarget - planarPosition, Vector3.up);
            Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
            return finalVelocity;

        }
    }
}