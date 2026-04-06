using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject arrowPrefab; 
    
    [Header("Adjustments")]
    public float verticalOffset = 0.1f; 
    public float forwardOffset = 1.0f;  
    public float arrowScale = 1.0f;
    public float smoothSpeed = 10f; 

    [Header("Mission Settings")]
    public float hideDistance = 3.0f;
    public float searchInterval = 0.5f; // Gaano kadalas maghahanap ng target (seconds)

    private GameObject spawnedArrow;
    private AnimalMissionLogic cachedMission;
    private float nextSearchTime;

    void Start()
    {
        if (arrowPrefab != null)
        {
            spawnedArrow = Instantiate(arrowPrefab);
            spawnedArrow.SetActive(false); 
        }
    }

    void LateUpdate()
    {
        if (spawnedArrow == null) return;

        // --- OPTIMIZATION: Hindi tayo naghahanap ng script bawat frame ---
        if (Time.time >= nextSearchTime)
        {
            cachedMission = Object.FindAnyObjectByType<AnimalMissionLogic>();
            nextSearchTime = Time.time + searchInterval; 
        }

        // Logic check gamit ang cached reference
        if (cachedMission != null && cachedMission.gameObject.scene.name != null && cachedMission.enabled)
        {
            float distanceToTarget = Vector3.Distance(transform.position, cachedMission.transform.position);

            if (distanceToTarget > hideDistance)
            {
                if (!spawnedArrow.activeSelf) spawnedArrow.SetActive(true);

                // Position Calculation
                Vector3 directionToAnimal = cachedMission.transform.position - transform.position;
                directionToAnimal.y = 0; 

                Vector3 targetPos = transform.position + (directionToAnimal.normalized * forwardOffset);
                targetPos.y += verticalOffset;

                // Smooth Movement
                spawnedArrow.transform.position = Vector3.Lerp(spawnedArrow.transform.position, targetPos, Time.deltaTime * smoothSpeed);
                spawnedArrow.transform.localScale = Vector3.one * arrowScale;

                // Smooth Rotation
                if (directionToAnimal.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToAnimal);
                    spawnedArrow.transform.rotation = Quaternion.Slerp(spawnedArrow.transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
                }
            }
            else
            {
                if (spawnedArrow.activeSelf) spawnedArrow.SetActive(false);
            }
        }
        else
        {
            if (spawnedArrow.activeSelf) spawnedArrow.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (spawnedArrow != null) Destroy(spawnedArrow);
    }
}