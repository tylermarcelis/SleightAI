using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName ="AI/Actions/Stat Change")]
    public class ChangeStatAction : AIAction
    {
        public CharacterStats.Stat stat;
        public enum StatOperator
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }
        public StatOperator statOperator;

        public float modifier;

        public override void Act(AIController controller)
        {
            // Modifys the controller's stat by an inputted amount
            switch (statOperator)
            {
                case StatOperator.Add:
                    controller.Character.stats.AddStat(stat, modifier);
                    break;
                case StatOperator.Subtract:
                    controller.Character.stats.AddStat(stat, -modifier);
                    break;
                case StatOperator.Multiply:
                    controller.Character.stats.MultiplyStat(stat, modifier);
                    break;
                case StatOperator.Divide:
                    controller.Character.stats.MultiplyStat(stat, 1/modifier);
                    break;
            }
        }
    }
}