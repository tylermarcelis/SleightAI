using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName = "AI/Conditions/Grounded")]
    public class GroundedCondition : AICondition
    {
        // Returns whether the controller is grounded
        public override bool Decide(AIController controller)
        {
            return controller.IsGrounded;
        }
    }
}
