using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    public abstract class AITreeNode : ScriptableObject
    {
        public abstract AIState GetState(AIController controller);

        public abstract AITreeNode Copy();

        public abstract bool CanGetState(AIController controller);
    }
}
