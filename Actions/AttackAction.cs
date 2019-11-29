using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(fileName = "Action_Attack", menuName = "AI/Actions/Attack")]
    public class AttackAction : AIAction
    {
        public float damage = 1;

        public override void Act(AIController controller)
        {
            // If controller has a target
            if (controller.target.Value)
            {
                // And that target has a character component
                Character targ = controller.target.Value.GetComponentInParent<Character>();
                if (targ)
                {
                    targ.Damage(controller.Character, damage * controller.Character.stats.GetStat(CharacterStats.Stat.Damage));
                }
            }
        }
    }
}
