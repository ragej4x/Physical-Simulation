using UnityEngine;

public class CubeButtonAnimation : MonoBehaviour
{
    public Animator targetAnimator; // Assign the Animator of the object you want to animate
    public string triggerName = "PlayAnimation"; // Animator parameter

    void OnMouseDown()
    {
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger(triggerName);
        }
    }
}
