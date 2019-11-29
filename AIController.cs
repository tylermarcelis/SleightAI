using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CardSlinger.Cards.Effects;

namespace CardSlinger.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Character))]
    public class AIController : MonoBehaviour
    {
        // The main behaviour tree for the AI to follow
        [SerializeField]
        protected AITreeNode aiTree;
        public AITreeNode AITreeNode
        {
            get { return aiTree; }
            set
            {
                SetAITree(value);
            }
        }

        [SerializeField]
        protected float speed = 1;

        public float Speed
        {
            get { return speed; }
            set {
                speed = value;
                ChangeSpeed(CharacterStats.Stat.Speed, Character.stats.GetStat(CharacterStats.Stat.Speed));
            }
        }
        
        // Events to be called on trigger and on collision
        public delegate void AICollision(AIController controller, Collision collision);
        public event AICollision OnAICollision;

        public delegate void AITrigger(AIController controller, Collider collision);
        public event AITrigger OnAITrigger;

        // The current state that the AI is performing
        public AIState CurrentState
        {
            get;
            protected set;
        }

        // Token AI
        [SerializeField]
        protected AICondition[] tokenConditions;

        [SerializeField]
        protected AITreeNode tokenTree;


        public AITreeNode TokenTree
        {
            get { return tokenTree; }
            set
            {
                SetTokenTree(value);
            }
        }

        // The cooldown after returning a token, before the AI can request again
        [SerializeField]
        protected float tokenCooldown = 1;

        // A modifier for how much the AI should take priority over other AI when requesting a token
        [Min(0.00001f)]
        public float priorityMod = 1;

        // A delay before the AI begins to act
        public float enableDelay = 0;

        // Reference to the singleton AIManager
        protected AIManager aiManager;

        // If the AI can currently follow actions
        public bool Active
        {
            get;
            protected set;
        }

        public bool IsTokenOnCooldown
        {
            get;
            protected set;
        } = false;

        public bool HasToken
        {
            get;
            protected set;
        } = false;

        // How long the AI has been in the current state
        public float StateDuration {
            get;
            private set;
        }

        // Reference to an attatch NavMeshAgent component
        public NavMeshAgent Agent
        {
            private set;
            get;
        }

        // Reference to an attatch Character component
        public Character Character
        {
            private set;
            get;
        }

        // Reference to an attatch Rigidbody component
        public Rigidbody Rigidbody
        {
            private set;
            get;
        }

        // Reference to an attatch Animator component
        public Animator Animator
        {
            private set;
            get;
        }

        // Reference to an attatch PowerManager component
        public PowerManager PowerManager
        {
            private set;
            get;
        }

        // Checks whether the AI is on the ground by raycasting downward
        public bool IsGrounded
        {
            get
            {
                if (Agent.isOnNavMesh)
                    return true;

                if (Rigidbody.isKinematic || Rigidbody.velocity.y < 0)
                {
                    LayerMask layerMask = GetCollisionMask();
                    Ray ray = new Ray(transform.position - new Vector3(0,Agent.baseOffset), -Vector3.up);
                    return Physics.Raycast(ray, 1, layerMask, QueryTriggerInteraction.Ignore);
                }
                return false;
            }
        }

        // How long since the AI has last had a token
        public float TimeSinceLastToken
        {
            get
            {
                return Time.time - lastTokenTime;
            }
        }

        // The current target for the AI
        public TransformReference target;

        // A mask storing what the AI collides with, based on its layer
        protected LayerMask collisionMask;

        protected float lastTokenTime;

        Coroutine disableForCoroutine;

        // Events for tokens and enabling and disabling
        public delegate void AIEvent();
        public event AIEvent OnReceiveToken;
        public event AIEvent OnReturnToken;
        public event AIEvent OnTokenCooldownFinish;

        public event AIEvent OnDisableEvent;
        public event AIEvent OnEnableEvent;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Character = GetComponent<Character>();
            Rigidbody = GetComponent<Rigidbody>();
            Animator = GetComponentInChildren<Animator>();
            PowerManager = GetComponentInChildren<PowerManager>();
            CopyTrees();
        }


        private void OnEnable()
        {
            // Subscribe to the characters speed stat to adjust NavMeshAgent accordingly
            Character.stats.SubscribeOnChange(CharacterStats.Stat.Speed, ChangeSpeed);
            ChangeSpeed(CharacterStats.Stat.Speed, Character.stats.GetStat(CharacterStats.Stat.Speed));

            Character.OnStun += Character_OnStun;
            Active = false;

            // Getting and adding itself to the AIManager
            aiManager = AIManager.Instance;
            aiManager.AddController(this);


            // Cancel powers if AI becomes disabled
            if (PowerManager)
                OnDisableEvent += PowerManager.CancelCast;

            // Checks whether to disable AI for a duration
            if (enableDelay > 0)
                DisableFor(enableDelay);
            else
                EnableAI();

        }

        private void Character_OnStun(Character character)
        {
            // Break out of state on stun
            ChangeState(null);

            // And return token, if it has one
            if (HasToken)
                ReturnToken();
        }

        private void OnDisable()
        {
            // Exit current state
            if (CurrentState)
                CurrentState.ExitState(this);

            CurrentState = null;

            // Unsubscribe from events
            Character.stats.UnsubscribeOnChange(CharacterStats.Stat.Speed, ChangeSpeed);

            Character.OnStun -= Character_OnStun;

            if (PowerManager)
                OnDisableEvent -= PowerManager.CancelCast;

            if (disableForCoroutine != null)
                StopCoroutine(disableForCoroutine);

            // Remove self from AIManager
            if (aiManager)
                aiManager.RemoveController(this);


            DisableAI();
        }

        private void Update()
        {
            // If inactive, does nothing
            if (!Active)
                return;

            if (!Character.IsStunned)
            {
                // If has a state, updates it
                if (CurrentState)
                {
                    CurrentState.UpdateState(this);
                }

                // Trys to enter a new state, if it has none, or has been in state long enough
                GotoNextState(!CurrentState || StateDuration > CurrentState.stateDuration);

                // Request token if able to
                if ((CurrentState && CurrentState.interuptable) && CanRequestToken())
                    RequestToken();
            }
            StateDuration += Time.deltaTime;

            // Stop Agent movement if stunned
            if (Agent.isOnNavMesh)
            {
                Agent.isStopped = Character.IsStunned;
            }

            SetAnimatorValues();
        }

        public void ChangeState(AIState state)
        {
            // Exits previous state
            if (CurrentState)
                CurrentState.ExitState(this);

            CurrentState = state;
            StateDuration = 0;

            // Enters new state
            if (CurrentState)
                CurrentState.EnterState(this);
        }

        // Set agent speed and animator speed when Character speed is changed
        void ChangeSpeed(CharacterStats.Stat stat, float value)
        {
            Agent.speed = speed * value;
            if (Animator)
                Animator.SetFloat("Speed", Agent.speed);
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnAICollision?.Invoke(this, collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            OnAITrigger?.Invoke(this, other);
        }

        // Requests a token from the AIManager
        protected void RequestToken()
        {
            if (aiManager)
                aiManager.RequestToken(this);
        }

        // Returns a token to the AIManager, after use
        protected void ReturnToken()
        {
            if (HasToken && aiManager)
            {
                HasToken = false;
                aiManager.ReturnToken(this);
                StartCoroutine(TokenCooldown(tokenCooldown));
                OnReturnToken?.Invoke();
            }
        }

        // Called by the AIManager to grant the AI the ability to use its token actions
        public bool GrantToken()
        {
            if (!CanRequestToken())
                return false;

            HasToken = true;
            ChangeState(tokenTree.GetState(this));
            OnReceiveToken?.Invoke();

            return true;
        }

        // Finds next state based on current state, or finds one from the AITree
        protected void GotoNextState(bool finished)
        {
            // If controller has an AITree
            if (aiTree)
            {
                AIState nextState = null;
                // Try to get next state from current state
                if (CurrentState)
                    nextState = CurrentState.GetNextState(this, finished);

                // If finished previous state, and got no state from previous state
                if (finished && !nextState)
                {
                    // Return token if it has one
                    if (HasToken)
                        ReturnToken();

                    // Get next state from AITree
                    nextState = aiTree.GetState(this);
                }

                // If found nextState, and is either not the same as current state, or state is marked as repeatable
                if (nextState && (nextState != CurrentState || nextState.repeatable))
                {
                    // Change State to new state
                    ChangeState(nextState);
                }
            }
        }

        public bool CanRequestToken()
        {
            // If AI doesn't already have a token, has a isn't on cooldown and can get a state from the tree
            if (HasToken || !tokenTree || IsTokenOnCooldown || !tokenTree.CanGetState(this))
                return false;

            // Check conditions
            foreach (AICondition condition in tokenConditions)
            {
                if (!condition.Decide(this))
                    return false;

            }

            return true;
        }

        // Function for creating a layer mask based on gameobjects layer
        protected LayerMask GetCollisionMask()
        {
            if (collisionMask != 0)
                return collisionMask;

            int layerMask = 0;

            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
                {
                    layerMask = layerMask | 1 << i;
                }
            }

            collisionMask = layerMask;
            return collisionMask;
        }

        protected void SetAnimatorValues()
        {
            if (Animator)
            {
                Animator.SetFloat("Speed", (Character.IsStunned) ? 0 : speed);
                Animator.SetFloat("CurrentMovementSpeed", Agent.velocity.magnitude);
                Animator.SetBool("Grounded", IsGrounded);
            }
        }

        // Coroutine for personal token request cooldowns
        IEnumerator TokenCooldown(float duration)
        {
            IsTokenOnCooldown = true;
            yield return new WaitForSeconds(duration);
            IsTokenOnCooldown = false;
            OnTokenCooldownFinish?.Invoke();
        }

        // Copy trees on startup
        private void CopyTrees()
        {
            SetAITree(aiTree);

            SetTokenTree(tokenTree);
        }

        // Setting AITree to a copy of an AITreeNode
        public void SetAITree(AITreeNode treeNode)
        {
            if (treeNode)
                aiTree = treeNode.Copy();
        }

        // Setting TokenTree to a copy of an AITreeNode
        public void SetTokenTree(AITreeNode treeNode)
        {
            if (treeNode)
                tokenTree = treeNode.Copy();
        }

        public void DisableFor(float duration)
        {
            StartCoroutine(DisableForCoroutine(duration));
        }

        // Coroutine to allow to disable AI for a time
        IEnumerator DisableForCoroutine(float delay)
        {
            DisableAI();
            yield return new WaitForSeconds(delay);
            EnableAI();
            disableForCoroutine = null;
        }

        // Disabling AI elements
        protected void DisableAI()
        {
            Agent.enabled = false;
            if (Active)
                OnDisableEvent?.Invoke();
            Active = false;
        }

        // Enabling AI elements
        protected void EnableAI()
        {
            Agent.enabled = true;
            Rigidbody.isKinematic = true;
            lastTokenTime = Time.time;
            if (Active)
                OnEnableEvent?.Invoke();
            Active = true;
        }
    }
}
