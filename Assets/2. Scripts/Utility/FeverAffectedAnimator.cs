using System.Collections;
using UnityEngine;

public class FeverAffectedAnimator : MonoBehaviour
{
    private Animator animator;
    private Fever fever;

    private IEnumerator Start()
    {
        animator = GetComponentInChildren<Animator>();
        fever = FindObjectOfType<Fever>();

        yield return null;

        if (fever != null && fever.isFeverModeActive)
            animator.speed = 2.0f;
        else
            animator.speed = 1.0f;
    }
}
