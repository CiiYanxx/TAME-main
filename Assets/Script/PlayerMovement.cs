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
        bool isNewGame = PlayerPrefs.GetInt("IsNewGame", 0) == 1;

        if (SaveSystem.HasSave() && !isNewGame)
        {
            LoadPlayerPosition();
            ResetCameraToDefault();
            FinishIntro();
            yield break;
        }

        PlayerPrefs.SetInt("IsNewGame", 0);
        PlayerPrefs.Save();

        if (controller != null) controller.enabled = false;
        transform.position = defaultSpawnPosition;
        if (controller != null) controller.enabled = true;

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
        yield return new WaitForSeconds(playerSpawnDelay);
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
        FinishIntro();
    }

    private void FinishIntro()
    {
        TogglePlayerVisuals(true);

        if (mainCanvas != null) mainCanvas.SetActive(true);

        // IMPORTANT: cinematic is DONE here, so allow tutorial flow
        canControl = false;

        if (TutorialController.Instance != null)
        {
            StartCoroutine(StartTutorialAfterIntro());
        }
        else
        {
            canControl = true;
        }
    }

    IEnumerator StartTutorialAfterIntro()
    {
        // wait AFTER full cinematic + camera reset + UI restore
        yield return new WaitForSecondsRealtime(1f);

        TutorialController.Instance.Tutorial0_Joystick();
    }

    IEnumerator StartTutorialAfterDelay()
    {
        // 🔥 REQUIRED FIX: delay before Step 0
        yield return new WaitForSecondsRealtime(1f);

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

    // ======================
    // YOUR ORIGINAL LOGIC BELOW (UNCHANGED STRUCTURE)
    // ======================

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
    IEnumerator ReturnToDefaultView() { yield return null; }
    private void TogglePlayerVisuals(bool show) { }
    private void DoAutoSave() { }
}