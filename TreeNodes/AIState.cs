using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    // An AITreeNode with actions, behaviours and its own transitions
    [CreateAssetMenu(menuName ="AI/State", order = -1)]
    public class AIState : AITreeNode
    {
        // Actions called on update, and when entering and exiting the state
        public AIAction[] updateActions;
        public AIAction[] enterActions;
        public AIAction[] exitActions;
        // Behaviours to be updated
        public AIBehaviour[] behaviours;

        // Transitions to other states
        public AITransition[] transitions;

        // Whether receiving a token can interupt this state
        public bool interuptable = true;

        [Tooltip("If true enter and exit will be called everytime the state restarts, even if not swapping states")]
        public bool repeatable = false;

        // How long controllers will remain in this state before attempting to find a new state
        // Can still be interupted
        public float stateDuration = 0;

        // Strings for setting animator triggers or entering states
        public string animatorTrigger;
        public string animatorStateName;

        public void EnterState(AIController controller)
        {
            // Call Enter Actions
            PerformActions(controller, enterActions);

            // Call Enter methods on behaviours
            foreach (AIBehaviour b in behaviours)
                b.Enter(controller);

            // Set animators
            if (animatorTrigger != "")
                controller.Animator.SetTrigger(animatorTrigger);
            else if (animatorStateName != "")
                controller.Animator.Play(animatorStateName);
            
        }

        public void UpdateState(AIController controller)
        {
            // Call Update Actions
            PerformActions(controller, updateActions);

            // Call Update methods on behaviours
            foreach (AIBehaviour b in behaviours)
                b.OnUpdate(controller);
        }

        public void ExitState(AIController controller)
        {
            // Call Exit Actions
            PerformActions(controller, exitActions);

            // Call Exit methods on behaviours
            foreach (AIBehaviour b in behaviours)
                b.Exit(controller);
        }

        // Helper function for calling Act on an array of actions
        private void PerformActions(AIController controller, AIAction[] actions)
        {
            foreach (AIAction act in actions)
            {
                act.Act(controller);
            }
        }

        public override AIState GetState(AIController controller)
        {
            return this;
        }

        // Attempts to get next state from list of transitions
        public AIState GetNextState(AIController controller, bool finished)
        {
            foreach (AITransition transition in transitions)
            {
                // If transition can interupt state, or state has finished
                if ((transition.canInterupt || finished) &&
                    transition.Decide(controller))
                    if (transition.node != null)
                        // Returns state from transition
                        return transition.node.GetState(controller);
            }

            return null;
        }

        // Copies over data
        public override AITreeNode Copy()
        {
            AIState copy = CreateInstance<AIState>();
            copy.name = name + " (Copy)";

            copy.interuptable = interuptable;
            copy.repeatable = repeatable;
            copy.stateDuration = stateDuration;
            copy.animatorTrigger = animatorTrigger;
            copy.animatorStateName = animatorStateName;

            copy.updateActions = updateActions;
            copy.enterActions = enterActions;
            copy.exitActions = exitActions;
            copy.behaviours = behaviours;
            copy.transitions = AITransition.CopyArray(transitions);

            return copy;
        }

        public override bool CanGetState(AIController controller)
        {
            return true;
        }
    }
}
