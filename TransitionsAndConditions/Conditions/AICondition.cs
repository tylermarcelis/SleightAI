using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    // Abstract class for conditions
    public abstract class AICondition : ScriptableObject
    {
        public abstract bool Decide(AIController controller);
    }
}
