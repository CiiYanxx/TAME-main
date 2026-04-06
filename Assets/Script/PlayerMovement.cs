using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("Input & Camera References")]
    public VirtualJoystick moveJoystick; 
    public FixedTouchField touchField; 
    public Camera playerCamera;         
    public Transform cameraOrbitTarget; 
    public CharacterCustomizer customizer;
    public GameObject mainCanvas;

    [Header("Cinematic Settings")]
    public bool canControl = false; 
    public List<Transform> introWaypoints = new List<Transform>(); 
    public float flySpeed = 15.0f;
    public Transform rotationWaypoint; 
    public float rotation360Speed = 100f, fixedXTilt = 10f; 

    [Header("Movement Settings")]
    public float moveSpeed = 5f, orbitRotationSpeed = 10f, sensitivity = 0.15f;
    public float minPitch = 1f, maxPitch = 30f;
    [HideInInspector] public bool isInteracting = false; 

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(165.94f, 0f, 142.88f); 

    // --- DAGDAG: CAMERA COLLISION SETTINGS SA INSPECTOR ---
    [Header("Camera Collision Settings")]
    public LayerMask collisionLayers; // Piliin dito ang "Default" o "Ground/Wall" layers
    public float collisionRadius = 0.4f;
    public float minCollisionDistance = 0.8f;
    public float cameraSmoothSpeed = 15f;

    private CharacterController controller;
    private Animator anim;
    private float cameraPitch, cameraYaw, defaultDistance, currentCameraDistance, verticalVelocity;

    private float autoSaveTimer = 0f;

    void Awake() { if (Instance == null) Instance = this; }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        Application.targetFrameRate = 60;
        if (playerCamera == null) playerCamera = Camera.main;
        
        if (playerCamera != null && cameraOrbitTarget != null)
        {
            // Kunin ang original distance para ito ang maging "max" zoom out
            defaultDistance = Vector3.Distance(playerCamera.transform.position, cameraOrbitTarget.position);
            currentCameraDistance = defaultDistance;
            Vector3 angles = playerCamera.transform.eulerAngles;
            cameraPitch = angles.x; cameraYaw = angles.y;
        }

        // LOAD PROGRESS
        LoadPlayerPosition();

        if (introWaypoints.Count > 0)
        {
            StartCoroutine(PlayFullCinematicSequence());
        }
        else 
        { 
            SkipIntroCinematic(); 
        }
    }

    private void LoadPlayerPosition()
    {
        if (!SaveSystem.HasSave()) 
        {
            if (controller != null) controller.enabled = false;
            // Force 0 sa Y para laging nakatapak
            Vector3 spawnPos = new Vector3(defaultSpawnPosition.x, 0f, defaultSpawnPosition.z);
            transform.position = spawnPos;
            if (controller != null) controller.enabled = true;
            Debug.Log("<color=green>New Game:</color> Player spawned at: " + spawnPos);
            return; 
        }

        GameData data = SaveSystem.Load();
        if (data != null)
        {
            Vector3 savedPos = new Vector3(data.playerPos[0], data.playerPos[1], data.playerPos[2]);
            if (savedPos != Vector3.zero)
            {
                if (controller != null) controller.enabled = false;
                transform.position = savedPos;
                if (controller != null) controller.enabled = true;
            }

            if (NPC.Instance != null) NPC.Instance.totalCompletedMissions = data.completedMissions;
            if (customizer != null) customizer.LoadCharacter();
            if (RescuePointsHandler.Instance != null) 
                RescuePointsHandler.Instance.currentPoints = data.playerPoints;
        }
    }

    private void DoAutoSave()
    {
        if (NPC.Instance == null) return;

        string appearance = "";
        if (customizer != null)
        {
            foreach (var part in customizer.GetCustomizationParts()) appearance += part.currentIndex + ",";
        }

        int pts = (RescuePointsHandler.Instance != null) ? RescuePointsHandler.Instance.currentPoints : 0;

        SaveSystem.Save(
            NPC.Instance.totalCompletedMissions,
            pts,
            transform.position,
            PlayerPrefs.GetString("Character_Name", "Rescue Hero"),
            appearance
        );
    }

    void OnApplicationPause(bool pause) { if (pause) DoAutoSave(); }
    void OnApplicationQuit() { DoAutoSave(); }

    public void SkipIntroCinematic()
    {
        StopAllCoroutines(); 
        TogglePlayerVisuals(true);
        canControl = true;
        if (mainCanvas != null) mainCanvas.SetActive(true);

        Quaternion orbitRot = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        playerCamera.transform.position = cameraOrbitTarget.position + (orbitRot * Vector3.back * defaultDistance);
        playerCamera.transform.rotation = orbitRot;
    }

    private void OnTriggerEnter(Collider other) { CheckDebris(other, true); }
    private void OnTriggerStay(Collider other) { CheckDebris(other, true); }
    private void OnTriggerExit(Collider other) { CheckDebris(other, false); }

    private void CheckDebris(Collider other, bool state)
    {
        DebrisItem debris = other.GetComponent<DebrisItem>();
        if (debris != null) {
            AnimalMissionLogic mission = Object.FindAnyObjectByType<AnimalMissionLogic>();
            if (mission != null) mission.UpdateDebrisDetection(debris, state);
        }
    }

    void Update() 
    { 
        if (canControl && !isInteracting) 
        {
            HandleMovement(); 
            autoSaveTimer += Time.deltaTime;
            if(autoSaveTimer >= 10f) { DoAutoSave(); autoSaveTimer = 0; }
        }
        else if (isInteracting && anim != null) anim.SetFloat("Speed", 0f);
    }

    void LateUpdate() { if (canControl && !isInteracting) HandleCameraRotation(); }

    void HandleCameraRotation()
    {
        if (touchField == null || cameraOrbitTarget == null || playerCamera == null) return;

        cameraYaw += touchField.TouchDist.x * sensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch - (touchField.TouchDist.y * sensitivity), minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 dir = rotation * Vector3.back;

        // --- IBALIK: CAMERA COLLISION LOGIC GAMIT ANG INSPECTOR SETTINGS ---
        float targetD = defaultDistance;
        RaycastHit hit;
        
        // SphereCast para detect kung may pader sa pagitan ng camera at player gamit ang settings sa Inspector
        if (Physics.SphereCast(cameraOrbitTarget.position, collisionRadius, dir, out hit, defaultDistance, collisionLayers))
        {
            // Ilalapit ang camera para hindi lumusot sa pader
            targetD = Mathf.Clamp(hit.distance, minCollisionDistance, defaultDistance);
        }

        // Smooth zoom effect
        currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetD, Time.deltaTime * cameraSmoothSpeed);

        playerCamera.transform.rotation = rotation;
        playerCamera.transform.position = cameraOrbitTarget.position + (dir * currentCameraDistance);
    }

    void HandleMovement()
    {
        if (moveJoystick == null || playerCamera == null) return;
        
        // Gravity: Mas malakas na diin para sa ground
        if (controller.isGrounded)
        {
            verticalVelocity = -2.0f; 
        }
        else
        {
            verticalVelocity -= 9.81f * Time.deltaTime;
        }

        Vector2 input = moveJoystick.Direction;
        Vector3 camF = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camR = Vector3.Scale(playerCamera.transform.right, new Vector3(1, 0, 1)).normalized;
        
        Vector3 moveDir = (camF * input.y) + (camR * input.x);
        Vector3 finalMove = moveDir * moveSpeed; finalMove.y = verticalVelocity;

        if (moveDir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * orbitRotationSpeed);
            if(anim != null) anim.SetFloat("Speed", input.magnitude);
        }
        else if(anim != null) anim.SetFloat("Speed", 0f);

        controller.Move(finalMove * Time.deltaTime);
    }

    IEnumerator PlayFullCinematicSequence()
    {
        canControl = false; if (mainCanvas != null) mainCanvas.SetActive(false); 
        TogglePlayerVisuals(false);
        foreach (Transform wp in introWaypoints)
        {
            while (Vector3.Distance(playerCamera.transform.position, wp.position) > 0.1f)
            {
                playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position, wp.position, flySpeed * Time.deltaTime);
                playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, wp.rotation, flySpeed * 0.5f * Time.deltaTime);
                yield return null;
            }
        }
        yield return StartCoroutine(ReturnToDefaultView());
        SkipIntroCinematic(); 
    }

    IEnumerator ReturnToDefaultView()
    {
        float t = 0; Vector3 startP = playerCamera.transform.position; Quaternion startR = playerCamera.transform.rotation;
        while (t < 1.0f) {
            t += Time.deltaTime * 1.5f; 
            Quaternion orbitR = Quaternion.Euler(cameraPitch, cameraYaw, 0);
            Vector3 targetP = cameraOrbitTarget.position + (orbitR * Vector3.back * defaultDistance);
            playerCamera.transform.position = Vector3.Lerp(startP, targetP, t);
            playerCamera.transform.rotation = Quaternion.Slerp(startR, orbitR, t);
            yield return null;
        }
    }

    private void TogglePlayerVisuals(bool show)
    {
        if (customizer == null) return;
        foreach (var data in customizer.GetCustomizationParts()) {
            if (data.changeMethod == CharacterCustomizer.CustomizationType.ToggleObject) {
                if (data.objectOptions != null && data.objectOptions.Length > data.currentIndex)
                    data.objectOptions[data.currentIndex].SetActive(show);
            }
            else if (data.targetRenderer != null) data.targetRenderer.enabled = show;
        }
    }
}