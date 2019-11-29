using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    // An AITreeNode that contains a number of transitions
    [CreateAssetMenu(menuName = "AI/Nodes/ConditionalNode", order = 1)]
    public class AIConditionalNode : AITreeNode
    {
        public AITransition[] transitions;


        public override AIState GetState(AIController controller)
        {
            AIState returnState = null;
            // Attempts to get a return state from each transition
            foreach (AITransition transition in transitions)
            {
                if (transition.Decide(controller))
                    returnState = transition.node.GetState(controller);

                if (returnState)
                    break;
            }

            return returnState;
        }

        // Copies transitions
        public override AITreeNode Copy()
        {
            AIConditionalNode copy = CreateInstance<AIConditionalNode>();
            copy.name = name + " (Copy)";

            copy.transitions = AITransition.CopyArray(transitions);

            return copy;
        }

        // Returns true if any of the states can be accessed
        public override bool CanGetState(AIController controller)
        {
            bool returnState = false;
            foreach (AITransition transition in transitions)
            {
                if (transition.Decide(controller))
                    returnState = transition.node.CanGetState(controller);

                if (returnState)
                    break;
            }

            return returnState;
        }
    }
}
