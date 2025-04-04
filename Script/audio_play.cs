using UnityEngine;
using System.Collections; // Add this line to fix the error

public class DisableColliderDuringExternalAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The GameObject that contains the AudioSource to monitor")]
    public GameObject audioSourceObject;

    [Header("Collider Settings")]
    [Tooltip("If empty, uses this GameObject's collider")]
    public Collider colliderToDisable;

    private AudioSource externalAudio;
    private Collider targetCollider;

    void Awake()
    {
        // Set up the collider reference
        targetCollider = colliderToDisable != null ? colliderToDisable : GetComponent<Collider>();
        
        // Verify we have a collider
        if (targetCollider == null)
        {
            Debug.LogError("No collider found or assigned!", this);
            return;
        }

        // Set up audio monitoring
        if (audioSourceObject != null)
        {
            externalAudio = audioSourceObject.GetComponent<AudioSource>();
            if (externalAudio == null)
            {
                Debug.LogError("No AudioSource found on the specified GameObject!", this);
                return;
            }
        }
        else
        {
            Debug.LogError("No audio source GameObject assigned!", this);
            return;
        }

        // Start monitoring the audio
        StartCoroutine(MonitorAudio());
    }

    IEnumerator MonitorAudio()
    {
        // Initial state (audio might already be playing)
        if (externalAudio.isPlaying)
        {
            targetCollider.enabled = false;
        }

        // Continuous monitoring
        while (true)
        {
            if (externalAudio.isPlaying && targetCollider.enabled)
            {
                targetCollider.enabled = false;
            }
            else if (!externalAudio.isPlaying && !targetCollider.enabled)
            {
                targetCollider.enabled = true;
            }
            yield return null; // Wait one frame
        }
    }

    // Clean up when disabled
    void OnDisable()
    {
        if (targetCollider != null)
        {
            targetCollider.enabled = true;
        }
    }
}