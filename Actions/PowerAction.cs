using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardSlinger.Cards.Effects;

namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Action_Power", menuName = "AI/Actions/Use Power")]
    public class PowerAction : AIAction
    {
        public Power power;
        public bool castTowardTarget = true;
        public Vector3 targetOffset;

        public override void Act(AIController controller)
        {
            // Attempts to get the controllers powerManager
            PowerManager pm = controller.PowerManager;
            // If none found, creates one
            if (!pm)
                pm = PowerManager.CreateInstance(controller.Character);

            // If castTowardTarget is true
            if (castTowardTarget && controller.target.Value)
                // Casts power with a target
                pm.AddPower(power, controller.target, targetOffset);
            else
                // Otherwise casts power, using the power manager's forward direction
                pm.AddPower(power);
        }
    }
}
