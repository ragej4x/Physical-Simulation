using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SwipeControlledAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public string animationName = "left-arm";
    [Range(0.1f, 5f)] public float animationSpeed = 1f;
    public float swipeThreshold = 50f;

    [Header("Frame Control")]
    public float framesPerSecond = 30f;
    public float reverseStopFrame = 15f;
    [Range(0.1f, 2f)] public float reverseSpeedMultiplier = 1f;

    [Header("Camera System")]
    public CombinedCameraFadeSystem cameraSystem;
    public int maxSwipes = 6;

    [Header("Audio Settings")]
    public bool enableAudio = true;
    public AudioClip audioClip;

    [Header("Child Object Settings")]
    public string childObjectName = "SpecificChild"; // Name of the child to hide/show
    private GameObject specificChild;

    private Animator anim;
    private AudioSource audioSource;
    private bool audioPlayed = false;
    private bool isAnimationMoving = false;
    private bool isSwiping = false;
    private int swipeCount = 0;
    private bool firstSwipeDone = false;

    private Vector2 swipeStartPos;
    private bool isReversing = false;
    private float lastNormalizedTime;
    private float playbackDirection = 1f;
    private float currentFrame;
    private float animationLength;
    private float reverseStopTime;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        specificChild = transform.Find(childObjectName)?.gameObject;
        if (specificChild == null)
        {
            Debug.LogWarning($"Child object with name '{childObjectName}' not found in {gameObject.name}");
        }
    }

    void Start()
    {
        animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
        reverseStopTime = reverseStopFrame / (framesPerSecond * animationLength);
        anim.speed = 0;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (enableAudio && audioClip != null)
        {
            audioSource.clip = audioClip;
        }

        SetChildVisibility(false);
    }

    void Update()
    {
        if (cameraSystem == null)
        {
            SetChildVisibility(false);
            audioPlayed = false;
            return;
        }

        bool isFocusing = cameraSystem.IsCameraFocusing();
        SetChildVisibility(isFocusing);

        if (!isFocusing)
        {
            audioPlayed = false;
            return;
        }

        if (enableAudio && !audioPlayed && audioSource.clip != null)
        {
            audioSource.Play();
            audioPlayed = true;
        }

        if (enableAudio && audioSource.isPlaying) return;

        currentFrame = GetCurrentFrame();
        UpdateAnimationMovementState();
        HandleInput();
        UpdateReversePlayback();
    }

    void SetChildVisibility(bool visible)
    {
        if (specificChild != null)
        {
            specificChild.SetActive(visible);
        }
    }

    void HandleInput()
    {
        if (isAnimationMoving) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    swipeStartPos = touch.position;
                    isSwiping = true;
                    break;
                case TouchPhase.Ended when isSwiping:
                    ProcessSwipe(touch.position);
                    break;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                swipeStartPos = Input.mousePosition;
                isSwiping = true;
            }
            else if (Input.GetMouseButtonUp(0) && isSwiping)
            {
                ProcessSwipe(Input.mousePosition);
            }
        }
    }

    void ProcessSwipe(Vector2 endPos)
    {
        Vector2 swipeDelta = endPos - swipeStartPos;

        if (Mathf.Abs(swipeDelta.x) > swipeThreshold)
        {
            if (swipeDelta.x > 0 && !firstSwipeDone)
            {
                isSwiping = false;
                return;
            }

            if (swipeDelta.x > 0)
            {
                StartReversePlayback();
                if (isAnimationMoving) swipeCount++;
            }
            else
            {
                firstSwipeDone = true;
                StartForwardPlayback();
                if (isAnimationMoving) swipeCount++;
            }
        }
        isSwiping = false;
    }

    void StartForwardPlayback()
    {
        isReversing = false;
        playbackDirection = 1f;
        isAnimationMoving = true;
        anim.Play(animationName, 0, anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        anim.speed = animationSpeed;
    }

    void StartReversePlayback()
    {
        isReversing = true;
        playbackDirection = -1f;
        isAnimationMoving = true;
        lastNormalizedTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        anim.Play(animationName, 0, lastNormalizedTime);
        anim.speed = 0;
    }

    void UpdateReversePlayback()
    {
        if (!isReversing) return;

        float delta = Time.deltaTime * animationSpeed * reverseSpeedMultiplier / animationLength;
        lastNormalizedTime = Mathf.Clamp01(lastNormalizedTime + delta * playbackDirection);
        anim.Play(animationName, 0, lastNormalizedTime);

        if (GetCurrentFrame() <= reverseStopFrame)
        {
            isAnimationMoving = false;
            isReversing = false;
            anim.Play(animationName, 0, reverseStopTime);
            anim.speed = 0;
            CheckSwipeLimit();
        }
    }

    void UpdateAnimationMovementState()
    {
        if (playbackDirection > 0 && currentFrame >= framesPerSecond * animationLength)
        {
            isAnimationMoving = false;
            anim.speed = 0;
            CheckSwipeLimit();
        }
        else if (playbackDirection < 0 && currentFrame <= reverseStopFrame)
        {
            isAnimationMoving = false;
            anim.speed = 0;
            anim.Play(animationName, 0, reverseStopTime);
            CheckSwipeLimit();
        }
    }

    void CheckSwipeLimit()
    {
        if (swipeCount >= maxSwipes)
        {
            swipeCount = 0;

            anim.Play(animationName, 0, 0f);
            anim.speed = 0;
            anim.Update(0f);

            transform.position = initialPosition;
            transform.rotation = initialRotation;

            isReversing = false;
            playbackDirection = 1f;
            isAnimationMoving = false;
            lastNormalizedTime = 0f;
            firstSwipeDone = false;

            if (cameraSystem != null)
            {
                cameraSystem.ResetCameraState();
            }

            SetChildVisibility(false);
        }
    }

    public float GetCurrentFrame()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime * framesPerSecond * animationLength;
    }
}