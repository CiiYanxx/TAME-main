using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 3f; 

    public void OnInteractButtonPressed() 
    {
        if (PlayerState.Instance == null || PlayerState.Instance.playerBody == null) return;

        Transform playerBody = PlayerState.Instance.playerBody.transform; 

        // 1. NPC Interaction
        NPC[] npcs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None); 
        foreach (NPC npc in npcs)
        {
            if (npc.playerInRange && !npc.isTalkingWithPlayer)
            {
                npc.StartConversation();
                return; 
            }
        }

        // 2. Animal Mission Interaction (Debris -> Feeding -> Minigame)
        AnimalMissionLogic[] missions = Object.FindObjectsByType<AnimalMissionLogic>(FindObjectsSortMode.None);
        foreach(AnimalMissionLogic mission in missions)
        {
            if(Vector3.Distance(mission.transform.position, playerBody.position) <= interactionRange)
            {
                mission.OnPlayerInteract();
                return;
            }
        }
    }
}