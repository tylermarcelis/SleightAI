using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    // Abstract class for Actions
    public abstract class AIAction : ScriptableObject
    {
        public abstract void Act(AIController controller);
    }
}
