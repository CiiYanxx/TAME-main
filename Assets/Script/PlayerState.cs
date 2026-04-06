using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }
    
    [Header("Player Body Reference")]
    public Transform playerBody; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (playerBody == null) playerBody = transform; 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}