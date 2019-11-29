using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Action_LookAt", menuName = "AI/Actions/Look At")]
    public class LookAtAction : AIAction
    {
        public float degreesPerSecond = 90;
        public override void Act(AIController controller)
        {
            if (controller.target != null && controller.target.Value != null)
            {
                // Rotates the controller's transform to face target, over time
                Vector3 lookPos = controller.target.Value.position - controller.transform.position;
                lookPos.y = 0;
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, rotation, Time.deltaTime * degreesPerSecond);
            }
        }
    }
}