using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    // Action for disabling or enabling AIController components and moving it to a hidden location
    [CreateAssetMenu(menuName = "AI/Actions/Hide and Disable")]
    public class SetHiddenAndDisabledAction : AIAction
    {
        public bool hidden;
        public Vector3 moveOffset;

        public override void Act(AIController controller)
        {
            // Disables or enables NavMeshAgent
            controller.Agent.enabled = !hidden;

            // Sets isKinematic to true if hidden
            if (hidden)
                controller.Rigidbody.isKinematic = true;

            // Moves controller's position based on an offset, such as under the ground
            if (hidden)
                controller.transform.position += moveOffset;
            else
                controller.transform.position -= moveOffset;

            // Prevents the AI from taking damage while hidden
            controller.Character.CanTakeDamage = !hidden;
        }
    }
}