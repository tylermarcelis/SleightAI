using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSlinger.AI
{
    public class AIManager : MonoBehaviour
    {
        [SerializeField]
        protected int maximumTokens = 3;
        public float tokenCooldown = 1;

        // List of all currently active AIControllers in the scene
        protected List<AIController> aiControllers = new List<AIController>();
        // A dictionary to easily get an AIController from a Character
        protected Dictionary<Character, AIController> charDict = new Dictionary<Character, AIController>();

        // List of all AIControllers with tokens
        protected List<AIController> currentTokenHolders = new List<AIController>();
        // List of all AIControllers who have requested tokens, but are yet to recieve them
        protected List<AIController> tokenRequests = new List<AIController>();

        // The current number of available tokens
        protected int availableTokens;

        // Weights are used to determine which enemy should receive a token first, when multiple enemy requests are active
        // based on the time since their last token, and whether the player is looking at them
        public float timePriorityWeight = 1;
        public float lookAtPriorityWeight = 5;
        [Range(0,1)]
        public float lookAtDamping = 0.5f;

        // Updates the bool reference whether in combat or not
        public BoolReference inCombat;

        // Singleton
        private static AIManager instance;
        public static AIManager Instance
        {
            get
            {
                if (instance)
                    return instance;
                else
                {
                    GameObject go = new GameObject("AI Manager");
                    instance = go.AddComponent<AIManager>();
                    return instance;
                }
            }
        }

        // Property for adjusting maximum tokens at runtime
        public int MaximumTokens
        {
            get
            {
                return maximumTokens;
            }

            set
            {
                availableTokens += value - maximumTokens;
                maximumTokens = value;
            }
        }

        private void Awake()
        {
            instance = this;

            if (inCombat == null)
                inCombat = new BoolReference();

            if (aiControllers.Count == 0)
            {
                availableTokens = maximumTokens;
                inCombat.Value = false;
            }
        }

        // Called by AIControllers OnEnable function to add themselves to the AIManagers list of controllers
        public void AddController(AIController controller)
        {
            if (controller)
            {
                // Add controller and their Character component to the list and dictionary
                aiControllers.Add(controller);
                charDict.Add(controller.Character, controller);

                // Add listener to the character's OnDeathCallback to remove them from lists when they die
                controller.Character.OnDeathCallback += OnAIDeath;

                if (!inCombat)
                    inCombat.Value = true;
            }
        }

        // Used to remove AIControllers from all lists
        public void RemoveController(AIController controller)
        {
            if (controller)
            {
                // Returns the AIControllers token if it has one
                ReturnToken(controller);

                // Removes controller from all lists and dictionaries
                aiControllers.Remove(controller);
                tokenRequests.Remove(controller);
                charDict.Remove(controller.Character);

                // Sets inCombat to false if aiControllers list is now empty
                if (aiControllers.Count == 0 && inCombat != null)
                    inCombat.Value = false;
            }
        }

        // Remove dead AI from lists, and return token if they have one
        protected void OnAIDeath(Character character, Character source)
        {
            RemoveController(character.GetComponent<AIController>());
        }

        // Allow AIControllers to request a token
        public void RequestToken(AIController controller)
        {
            // If controller does not already have a request, and it does not already have a token
            if (!controller.HasToken && !tokenRequests.Contains(controller) && !currentTokenHolders.Contains(controller))
            {
                // If any tokens are available grant one immediately
                if (availableTokens > 0)
                    GrantToken(controller);
                // Otherwise add to list of current requests
                else
                    tokenRequests.Add(controller);
            }
        }

        // Allow AIControllers to revoke a token request
        public void RevokeTokenRequest(AIController controller)
        {
            tokenRequests.Remove(controller);
        }

        // If controller currently has a token, remove them from the list and start coroutine for cooldown
        public void ReturnToken(AIController controller)
        {
            if (currentTokenHolders.Contains(controller))
            {
                currentTokenHolders.Remove(controller);
                StartCoroutine(TokenCooldown(tokenCooldown));
            }
        }

        // Gives token to target AIController
        protected void GrantToken(AIController controller)
        {
            tokenRequests.Remove(controller);
            if (controller.GrantToken())
            {
                // Reduces tokens by one and adds controller to list of currentTokenHolders
                availableTokens--;
                currentTokenHolders.Add(controller);
            }
        }

        // Prevent tokens from becoming immediately available after use
        protected IEnumerator TokenCooldown(float duration)
        {
            yield return new WaitForSeconds(duration);
            availableTokens++;
            GrantNextTokens();
        }

        // Finds and provides tokens to AIControllers, called when one becomes available
        protected void GrantNextTokens()
        {
            // Get a list of AIControllers, sorted by their priority
            List<AIController> requests = GetPrioritizedList();

            // Keeps attempting to provide tokens to AI until there are none remaining
            for (int i = 0; i < requests.Count; i++)
            {
                GrantToken(requests[i]);
                if (availableTokens <= 0)
                    break;
            }
        }

        // Copies and returns tokenRequests, sorted by their token priority
        protected List<AIController> GetPrioritizedList()
        {
            List<AIController> requests = new List<AIController>(tokenRequests);
            requests.Sort(Prioritize);
            return requests;
        }

        // Compare function for sorting
        protected int Prioritize(AIController controller1, AIController controller2)
        {
            return GetPriority(controller2).CompareTo(GetPriority(controller1));
        }

        // Calcluates the priority of an AIController for comparison
        protected float GetPriority(AIController controller)
        {
            float priorityValue = 0;

            // Add priority based on time since the controller last received a token
            priorityValue += controller.TimeSinceLastToken * timePriorityWeight;

            // Add priority based on if the player is currently looking at the AI,
            // This ensures enemies the player is currently looking at are more likely to receive a token
            Camera cam = Camera.main;
            float lookStrength = Vector3.Dot(cam.transform.forward, (controller.transform.position - cam.transform.position).normalized);
            if (lookAtDamping > 0)
            {
                lookStrength = Mathf.Clamp(lookStrength, 0, lookAtDamping);
                lookStrength *= 1 / lookAtDamping;
            }
            priorityValue += lookStrength * lookAtPriorityWeight;

            // Multiplies by the AIControllers personal priority modifier
            priorityValue *= controller.priorityMod;
            return priorityValue;
        }

        // Kills all active AI
        public void KillAll()
        {
            for (int i = aiControllers.Count - 1; i >= 0; i--)
            {
                aiControllers[i].Character.Die(null);
            }
        }

    }
}
