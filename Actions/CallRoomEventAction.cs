using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CardSlinger.AI {
    [CreateAssetMenu(menuName = "AI/Actions/Call Room Event")]
    public class CallRoomEventAction : AIAction
    {
        public string tag;

        public override void Act(AIController controller)
        {
            // If AI is on navmesh
            if (controller.Agent.isOnNavMesh)
            {
                // Gets the NavMeshSurface it is on
                NavMeshSurface surface = controller.Agent.navMeshOwner as NavMeshSurface;
                if (surface)
                {
                    // Finds the RoomEvents component
                    RoomEvents events = surface.gameObject.GetComponentInChildren<RoomEvents>();
                    if (events)
                    {
                        // And calls the event with corrisponding tag
                        events.CallEvent(tag);
                    }
                }
            }
        }
    }
}