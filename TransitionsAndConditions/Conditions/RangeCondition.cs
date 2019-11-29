using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName ="AI/Conditions/Range")]
    public class RangeCondition : AICondition
    {
        public float range;
        public float range2;
        public enum RangeComparator
        {
            LessThan,
            GreaterThan,
            Between
        }
        public RangeComparator comparator;

        public override bool Decide(AIController controller)
        {
            if (!controller.target.Value)
                return false;

            // Calculates distance between controller and target
            float distance = Vector3.Distance(controller.transform.position, controller.target.Value.position);

            switch (comparator)
            {
                // Returns if greater than
                case RangeComparator.GreaterThan:
                    return distance >= range;
                // Returns if less than
                case RangeComparator.LessThan:
                    return distance < range;
                // Returns if between range and range2
                default:
                    return (distance >= range && distance < range2) || (distance >= range2 && distance < range);
            }
        }
    }
}