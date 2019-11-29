using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName = "AI/Actions/Summon At Target")]
    public class SummonAtTargetAction : AIAction
    {
        [SerializeField]
        protected GameObject summonObject;
        public override void Act(AIController controller)
        {
            if (controller.target.Value)
            {
                // Instantiate a game object at the controller's position
                GameObject go = Instantiate(summonObject, controller.target.Value.position, summonObject.transform.rotation);
                // Set up any ICaster interfaces
                ICaster[] castComps = go.GetComponentsInChildren<ICaster>();
                foreach (ICaster cast in castComps)
                    cast.SetCaster(controller.Character);
            }
        }
    }
}
