using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    // Behaviours are like actions, but with all enter, update and exit functions
    public abstract class AIBehaviour : ScriptableObject
    {

        public virtual void Enter(AIController controller)
        {
        }

        public virtual void OnUpdate(AIController controller)
        {

        }

        public virtual void Exit(AIController controller)
        {

        }
    }
}
