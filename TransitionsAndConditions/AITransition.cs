using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [System.Serializable]
    public class AITransition
    {
        // Transitions store AITreeNodes, conditions and whether they can interupt the previous state
        public AICondition[] conditions;
        public AITreeNode node;
        public bool canInterupt;

        public bool Decide(AIController controller)
        {
            // Checks each condition
            foreach (AICondition cond in conditions)
            {
                // If any return false, returns false
                if (!cond.Decide(controller))
                    return false;
            }
            return true;
        }

        // Copys the transition
        public AITransition Copy()
        {
            AITransition copy = new AITransition()
            {
                node = (node != null) ? node.Copy() : null,
                canInterupt = canInterupt,
                conditions = conditions
            };

            return copy;
        }

        // Copys an array of transitions
        public static AITransition[] CopyArray(AITransition[] arr)
        {
            AITransition[] copy = new AITransition[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                copy[i] = arr[i].Copy();
            }

            return copy;
        }
    }
}
