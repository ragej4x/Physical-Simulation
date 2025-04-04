using UnityEngine;
using TMPro;
using System.Collections;

public class CombinedCameraFadeSystem : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public Transform buttonCube;
    public float focusSpeed = 5f;
    public float focusDistance = 2f;
    public Vector3 positionOffset = new Vector3(0f, 2f, 0f);
    public Vector3 rotationAngles = new Vector3(0f, 0f, 0f);
    public float maxRotationAngle = 5f;
    public float smoothSpeed = 5f;

    [Header("Text Fade Settings")]
    public TMP_Text infoText;
    public float fadeDuration = 0.5f;
    public float focusedTextAlpha = 1f;
    public float unfocusedTextAlpha = 0f;

    private Vector3 targetPosition;
    private bool isFocusing = false;
    private Quaternion startRotation;
    private Vector3 focusDirection;
    private Quaternion baseFocusRotation;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Coroutine currentFadeCoroutine;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            startRotation = mainCamera.transform.rotation;
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
        }
        else
        {
            Debug.LogWarning("No camera assigned and no main camera found!");
        }

        if (infoText == null)
        {
            Debug.LogError("infoText is not assigned! Please assign a TextMeshPro object.");
            return;
        }

        // Ensure text is visible on start for debugging
        //infoText.text = "Debug Text!";
        //infoText.color = Color.red;
        //infoText.fontSize = 50;
        SetTextAlpha(unfocusedTextAlpha);
        Debug.Log("Text initialized. Alpha: " + unfocusedTextAlpha);
    }

    void Update()
    {
        HandleCameraControl();
    }

    void HandleCameraControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && buttonCube != null && hit.collider.transform == buttonCube)
                {
                    if (!isFocusing)
                    {
                        StartCameraFocus();
                        FadeText(focusedTextAlpha);
                    }
                }
            }
        }

        if (isFocusing)
        {
            Vector3 focusPoint = targetPosition - focusDirection * focusDistance;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, focusPoint, Time.deltaTime * focusSpeed);
            HandleSubtleMouseRotation(true);
        }
        else
        {
            HandleSubtleMouseRotation(false);
        }

        if (isFocusing && Input.GetKeyDown(KeyCode.Escape))
        {
            ResetCameraState();
            FadeText(unfocusedTextAlpha);
        }
    }

    void StartCameraFocus()
    {
        if (buttonCube != null)
        {
            targetPosition = buttonCube.position + positionOffset;
            focusDirection = (targetPosition - mainCamera.transform.position).normalized;
            baseFocusRotation = Quaternion.Euler(rotationAngles);
            isFocusing = true;
        }
    }

    void HandleSubtleMouseRotation(bool duringFocus)
    {
        if (mainCamera == null) return;

        float mouseX = (Input.mousePosition.x / Screen.width - 0.5f) * 2;
        float mouseY = (Input.mousePosition.y / Screen.height - 0.5f) * 2;

        if (duringFocus)
        {
            Quaternion mouseRotation = Quaternion.Euler(-mouseY * maxRotationAngle, mouseX * maxRotationAngle, 0);
            Quaternion targetRotation = baseFocusRotation * mouseRotation;
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }
        else
        {
            Quaternion targetRotation = startRotation * Quaternion.Euler(-mouseY * maxRotationAngle, mouseX * maxRotationAngle, 0);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }
    }

    public void ResetCameraState()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = originalCameraPosition;
            mainCamera.transform.rotation = originalCameraRotation;
            isFocusing = false;

            SetTextAlpha(unfocusedTextAlpha);
            Debug.Log("Camera reset, text alpha forced to: " + unfocusedTextAlpha);
        }
    }

    public bool IsCameraFocusing()
    {
        return isFocusing;
    }

    #region Text Fading Functions
    private void FadeText(float targetAlpha)
    {
        if (infoText == null)
        {
            Debug.LogWarning("FadeText: infoText is NULL!");
            return;
        }

        // Stop any existing fade coroutine
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        Debug.Log($"Fading text to: {targetAlpha}");

        if (fadeDuration <= 0)
        {
            SetTextAlpha(targetAlpha);
            return;
        }

        currentFadeCoroutine = StartCoroutine(FadeTextCoroutine(targetAlpha, fadeDuration));
    }

    private IEnumerator FadeTextCoroutine(float targetAlpha, float duration)
    {
        float startAlpha = infoText.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            SetTextAlpha(newAlpha);
            yield return null;
        }

        SetTextAlpha(targetAlpha);
    }

    private void SetTextAlpha(float alpha)
    {
        if (infoText != null)
        {
            Color color = infoText.color;
            color.a = alpha;
            infoText.color = color;
            //Debug.Log($"SetTextAlpha called! New Alpha: {alpha}");
        }
        else
        {
            Debug.LogWarning("SetTextAlpha: infoText is NULL!");
        }
    }
    #endregion
}
