using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName ="AI/Nodes/Weighted Conditional")]
    public class AIWeightedConditionalNode : AITreeNode
    {
        // A weighted node is a serialized class storing modifyable weights
        // for a weighted random system
        [System.Serializable]
        public class WeightedNode
        {
            public AITreeNode node;
            // When node is not picked, adds weightModifier to currentWeight
            public float weightModifier = 1;
            // When node is picked, resets current weight to returnWeight
            public float returnWeight = 0;
            [HideInInspector]
            public float currentWeight;

            public WeightedNode() { }
            public WeightedNode(AITreeNode node, float weight, float returnWeight)
            {
                this.node = node;
                weightModifier = weight;
                currentWeight = returnWeight;
                this.returnWeight = returnWeight;
            }
        }

        public WeightedNode[] weightedValues;

        // Copies scriptableObject so that each AIController has different currentWeights
        public override AITreeNode Copy()
        {
            AIWeightedConditionalNode copy = CreateInstance<AIWeightedConditionalNode>();
            copy.name = name + " (Copy)";
            copy.weightedValues = CopyWeights(weightedValues);

            return copy;
        }

        // Override for getting a state
        public override AIState GetState(AIController controller)
        {
            float weightTotal = 0;
            WeightedNode returnNode = null;

            // Calculate total weight
            foreach (WeightedNode value in weightedValues)
                weightTotal += value.currentWeight;

            // Copies weighted values
            List<WeightedNode> values = new List<WeightedNode>(weightedValues);
            int selectedNode = -1;

            while (values.Count > 0 && returnNode == null)
            {
                // Calculate random weight
                float randomWeight = Random.Range(0, weightTotal);

                // Find weightedValue corresponding to that weight
                for (int i = 0; i < values.Count; i++)
                {
                    WeightedNode value = values[i];
                    // If found sets returnNode and selectedNode, and breaks out of for loop
                    if (randomWeight <= value.currentWeight)
                    {
                        returnNode = value;
                        selectedNode = i;
                        break;
                    }
                    // Otherwise, reduces randomWeight by the values weight
                    else randomWeight -= value.currentWeight;
                }

                // If found a node, and but can't get a state,
                if (returnNode != null && !returnNode.node.CanGetState(controller))
                {
                    // Removes from list, resets selectedNode and returnNode, and reduces weightTotal by the nodes weight
                    values.RemoveAt(selectedNode);
                    selectedNode = -1;
                    weightTotal -= returnNode.currentWeight;
                    returnNode = null;
                }

            }

            // If found a returnNode
            if (returnNode != null)
            {
                // For each weightedValue
                for (int i = 0; i < weightedValues.Length; i++)
                {
                    // Sets returnNodes currentWeight to its return weight
                    if (i == selectedNode) weightedValues[i].currentWeight = weightedValues[i].returnWeight;
                    // Or adds other nodes weightModifier to their currentWeight
                    else weightedValues[i].currentWeight += weightedValues[i].weightModifier;
                }
            }

            // Returns the nodes state
            return returnNode.node.GetState(controller);
        }

        // Used for copying the class
        protected WeightedNode[] CopyWeights(WeightedNode[] arr)
        {
            WeightedNode[] copy = new WeightedNode[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                copy[i] = new WeightedNode(arr[i].node.Copy(), arr[i].weightModifier, arr[i].returnWeight);
            }

            return copy;
        }

        // Returns true if any of its states can be accessed
        public override bool CanGetState(AIController controller)
        {
            // If any are possible
            foreach (WeightedNode node in weightedValues)
                if (node.node.CanGetState(controller))
                    return true;

            return false;
        }
    }
}
