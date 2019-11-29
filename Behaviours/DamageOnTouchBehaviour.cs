using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    [CreateAssetMenu(menuName ="AI/Behaviours/Damage On Touch")]
    public class DamageOnTouchBehaviour : AIBehaviour
    {
        public float damageAmount;
        public LayerMask validLayers;

        public override void Enter(AIController controller)
        {
            // Subscribes to the controllers onTrigger Function
            controller.OnAITrigger += OnTrigger;
        }

        public override void Exit(AIController controller)
        {
            // Unsubscribes to the controllers onTrigger Function
            controller.OnAITrigger -= OnTrigger;
        }


        protected void OnTrigger(AIController controller, Collider collision)
        {
            // Checks if collider is on a valid layer
            if ((1 << collision.gameObject.layer & validLayers) == 0)
                return;

            // Checks whether the collided object is the same character as the controller's target
            Character targetCharacter = controller.target.Value.GetComponentInParent<Character>();
            Character colliderCharacter = collision.GetComponentInParent<Character>();

            if (targetCharacter == colliderCharacter)
            {
                // If so, damages the character
                targetCharacter.Damage(controller.Character, damageAmount * controller.Character.stats.GetStat(CharacterStats.Stat.Damage));
                // And bounce off
                controller.Rigidbody.velocity = -controller.Rigidbody.velocity;
            }
        }
    }
}