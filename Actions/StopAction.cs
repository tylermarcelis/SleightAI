using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Action_Stop", menuName = "AI/Actions/Stop")]
    public class StopAction : AIAction
    {
        public override void Act(AIController controller)
        {
            // Resets the controller's NavMeshAgents path
            if (controller.Agent.hasPath)
                controller.Agent.ResetPath();
        }
    }
}
