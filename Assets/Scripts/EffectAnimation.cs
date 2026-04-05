using UnityEngine;

public class EffectAnimation : MonoBehaviour
{
    [SerializeField] Animator animator;

    internal void UseSlash(bool b = false)
    {
        animator.SetBool("useSlash", true);
    }

    public void UseSlash_End()
    {
        animator.SetBool("useSlash", false);
    }
}
