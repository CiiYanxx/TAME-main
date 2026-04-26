using UnityEngine;
using UnityEngine.UI;
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

    [Header("Sneak Settings")]
    public bool isSneaking = false; 
    public float sneakSpeed = 2.0f; 
    public bool isRunning = false;  

    [Header("Bus Cinematic Settings")]
    public GameObject busPrefab;
    public Transform busStartPoint, busStopPoint, busEndPoint;
    public float busSpeed = 12f;
    public float waitAtStopDuration = 5f;

    [Header("Camera Waypoint Settings")]
    public List<Transform> introWaypoints = new List<Transform>(); 
    public float flySpeed = 15.0f;

    [Header("Character Spawn Settings")]
    public List<GameObject> objectsToHide = new List<GameObject>();
    public float playerSpawnDelay = 4.0f; 
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(165.94f, 0f, 142.88f);

    [Header("Movement & Camera Settings")]
    public float moveSpeed = 5f, orbitRotationSpeed = 10f, sensitivity = 0.15f;
    public float minPitch = 1f, maxPitch = 30f;
    public LayerMask collisionLayers; 
    public float collisionRadius = 0.4f, minCollisionDistance = 0.8f, cameraSmoothSpeed = 15f;

    private CharacterController controller;
    private Animator anim;
    private float cameraPitch, cameraYaw, defaultDistance, currentCameraDistance, verticalVelocity;
    private float autoSaveTimer = 0f;
    private Coroutine cinematicCoroutine; 

    private bool cameraFullyReset = false;

    [HideInInspector] public bool canControl = false, isInteracting = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        Application.targetFrameRate = 60;

        if (mainCanvas != null) mainCanvas.SetActive(false);

        // 🔥 HIDDEN AT START
        TogglePlayerVisuals(false);

        if (playerCamera == null) playerCamera = Camera.main;

        if (playerCamera != null && cameraOrbitTarget != null)
        {
            defaultDistance = Vector3.Distance(playerCamera.transform.position, cameraOrbitTarget.position);
            currentCameraDistance = defaultDistance;

            Vector3 angles = playerCamera.transform.eulerAngles;
            cameraPitch = angles.x;
            cameraYaw = angles.y;
        }

        StartCoroutine(InitializeGameFlow());
    }

    public void ToggleSneak()
    {
        isSneaking = !isSneaking;
        if (anim != null) anim.SetBool("isCrouching", isSneaking);
    }

    IEnumerator InitializeGameFlow()
    {
        GameData data = SaveSystem.Load();
        bool isNewGame = PlayerPrefs.GetInt("IsNewGame", 0) == 1;

        // 1. CHECK KUNG MAY SAVE DATA AT HINDI NEW GAME
        if (data != null && !isNewGame)
        {
            Debug.Log("<color=green>[PLAYER GAMELOG]</color> Save data found. Resuming game...");

            // I-set ang position base sa save file (playerPos ay float array sa GameData mo)
            if (data.playerPos != null && data.playerPos.Length == 3)
            {
                if (controller != null) controller.enabled = false;
                
                // Convert float array back to Vector3
                Vector3 savedPos = new Vector3(data.playerPos[0], data.playerPos[1], data.playerPos[2]);
                transform.position = savedPos;
                
                if (controller != null) controller.enabled = true;
                Debug.Log("<color=cyan>[PLAYER GAMELOG]</color> Player Teleported to: " + savedPos);
            }

            ResetCameraToDefault();
            
            // Direkta sa laro, skip ang intro/cinematic
            if (mainCanvas != null) mainCanvas.SetActive(true);
            TogglePlayerVisuals(true);
            canControl = true; 
            
            yield break; // Tapos na ang flow dito para sa Resume
        }

        // 2. KUNG NEW GAME O WALANG SAVE DATA
        Debug.Log("<color=yellow>[PLAYER GAMELOG]</color> New Game detected. Starting Cinematic...");
        
        PlayerPrefs.SetInt("IsNewGame", 0);
        PlayerPrefs.Save();

        // I-set sa default spawn position para sa tutorial
        if (controller != null) controller.enabled = false;
        transform.position = defaultSpawnPosition;
        if (controller != null) controller.enabled = true;

        // Simulan ang Cinematic
        cinematicCoroutine = StartCoroutine(PlayFullCinematicMaster());
    }

  IEnumerator PlayFullCinematicMaster()
    {
        canControl = false;

        StartCoroutine(MoveBusSequence());
        StartCoroutine(DelayedPlayerShow());

        foreach (Transform wp in introWaypoints)
        {
            while (Vector3.Distance(playerCamera.transform.position, wp.position) > 0.1f)
            {
                playerCamera.transform.position = Vector3.MoveTowards(
                    playerCamera.transform.position,
                    wp.position,
                    flySpeed * Time.deltaTime
                );

                playerCamera.transform.rotation = Quaternion.Slerp(
                    playerCamera.transform.rotation,
                    wp.rotation,
                    flySpeed * 0.5f * Time.deltaTime
                );

                yield return null;
            }
        }

        yield return StartCoroutine(ReturnToDefaultView());

        cameraFullyReset = true;

        yield return null;

        cinematicCoroutine = null;
        FinishIntro();
    }

    IEnumerator MoveBusSequence()
    {
        if (busPrefab == null || busStartPoint == null) yield break;

        GameObject bus = Instantiate(busPrefab, busStartPoint.position, busStartPoint.rotation);

        yield return StartCoroutine(LerpBus(bus, busStopPoint.position));
        yield return new WaitForSeconds(waitAtStopDuration);
        yield return StartCoroutine(LerpBus(bus, busEndPoint.position));

        Destroy(bus);
    }

    IEnumerator LerpBus(GameObject bus, Vector3 target)
    {
        while (bus != null && Vector3.Distance(bus.transform.position, target) > 0.1f)
        {
            bus.transform.position = Vector3.MoveTowards(
                bus.transform.position,
                target,
                busSpeed * Time.deltaTime
            );

            Vector3 dir = (target - bus.transform.position).normalized;

            if (dir != Vector3.zero)
            {
                bus.transform.rotation = Quaternion.Slerp(
                    bus.transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 5f
                );
            }

            yield return null;
        }
    }

    IEnumerator DelayedPlayerShow()
    {
        yield return new WaitForSecondsRealtime(playerSpawnDelay);
        TogglePlayerVisuals(true);
    }

    public void SkipIntroCinematic()
    {
        if (cinematicCoroutine != null)
        {
            StopCoroutine(cinematicCoroutine);
            cinematicCoroutine = null;
        }

        ResetCameraToDefault();
        cameraFullyReset = true;
        FinishIntro();
    }

    private void FinishIntro()
    {
        if (mainCanvas != null) mainCanvas.SetActive(true);
        TogglePlayerVisuals(true);

        if (TutorialController.Instance != null && PlayerPrefs.GetInt("Tutorial_Completed", 0) == 0)
        {
            canControl = false; // Stop muna para sa tutorial
            StartCoroutine(StartTutorialAfterIntro());
        }
        else
        {
            canControl = true; // Default na true kung tapos na tutorial
        }
    }

    IEnumerator StartTutorialAfterIntro()
    {
        while (!cameraFullyReset)
            yield return null;

        yield return new WaitForSecondsRealtime(0.3f);

        TutorialController.Instance.Tutorial0_Joystick();
    }

    private void ResetCameraToDefault()
    {
        if (playerCamera != null && cameraOrbitTarget != null)
        {
            Quaternion orbitRot = Quaternion.Euler(cameraPitch, cameraYaw, 0);

            playerCamera.transform.position =
                cameraOrbitTarget.position + (orbitRot * Vector3.back * defaultDistance);

            playerCamera.transform.rotation = orbitRot;
        }
    }

    IEnumerator ReturnToDefaultView()
    {
        if (playerCamera == null || cameraOrbitTarget == null)
            yield break;

        Quaternion targetRot = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 targetPos = cameraOrbitTarget.position + (targetRot * Vector3.back * defaultDistance);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetPos,
                t
            );

            playerCamera.transform.rotation = Quaternion.Slerp(
                playerCamera.transform.rotation,
                targetRot,
                t
            );

            yield return null;
        }

        playerCamera.transform.position = targetPos;
        playerCamera.transform.rotation = targetRot;
    }

    void Update()
    {
        if (canControl && !isInteracting)
        {
            HandleMovement();

            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= 10f)
            {
                DoAutoSave();
                autoSaveTimer = 0f;
            }
        }
        else if (isInteracting && anim != null)
        {
            anim.SetFloat("Speed", 0f);
        }
    }

    void LateUpdate()
    {
        if (canControl && !isInteracting)
        {
            HandleCameraRotation();
        }
    }

    void HandleCameraRotation()
    {
        if (touchField == null || cameraOrbitTarget == null || playerCamera == null) return;

        cameraYaw += touchField.TouchDist.x * sensitivity;

        cameraPitch = Mathf.Clamp(
            cameraPitch - (touchField.TouchDist.y * sensitivity),
            minPitch,
            maxPitch
        );

        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 dir = rotation * Vector3.back;

        float targetD = defaultDistance;

        if (Physics.SphereCast(cameraOrbitTarget.position, collisionRadius, dir,
            out RaycastHit hit, defaultDistance, collisionLayers))
        {
            targetD = Mathf.Clamp(hit.distance * 0.9f, minCollisionDistance, defaultDistance);
        }

        currentCameraDistance = Mathf.Lerp(
            currentCameraDistance,
            targetD,
            Time.deltaTime * cameraSmoothSpeed
        );

        playerCamera.transform.rotation = rotation;
        playerCamera.transform.position = cameraOrbitTarget.position + (dir * currentCameraDistance);
    }

    void HandleMovement()
    {
        if (moveJoystick == null || playerCamera == null) return;

        if (controller.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity -= 9.81f * Time.deltaTime;

        Vector2 input = moveJoystick.Direction;

        Vector3 camF = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camR = Vector3.Scale(playerCamera.transform.right, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDir = (camF * input.y) + (camR * input.x);

        float currentSpeed = isSneaking ? sneakSpeed : moveSpeed;
        isRunning = (input.magnitude > 0.7f && !isSneaking);

        Vector3 finalMove = moveDir * currentSpeed;
        finalMove.y = verticalVelocity;

        if (moveDir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir),
                Time.deltaTime * orbitRotationSpeed
            );

            if (anim != null)
                anim.SetFloat("Speed", input.magnitude);
        }
        else if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
        }

        controller.Move(finalMove * Time.deltaTime);

        if (input.magnitude > 0.3f && TutorialController.Instance != null)
        {
            TutorialController.Instance.Tutorial1_Swipe();
        }
    }

    private void LoadPlayerPosition() { }

    private void TogglePlayerVisuals(bool show)
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in rends)
        {
            r.enabled = show;
        }

        if (anim != null)
            anim.enabled = show;
    }

    private void DoAutoSave() { }
    
    public void HardStopMovement()
    {
        if (controller == null) return;

        anim.SetFloat("Speed", 0f);
    }
}