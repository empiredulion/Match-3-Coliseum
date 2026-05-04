using UnityEngine;

public class DamageNumberText : MonoBehaviour
{
    [SerializeField] Animator animator;

    public void IsHpLost(bool b = false)
    {
        animator.SetBool("isHpLost", true);
    }

    public void IsArmorLost(bool b = false)
    {
        animator.SetBool("isArmorLost", true);
    }

    public void IsHeal(bool b = false)
    {
        animator.SetBool("isHeal", true);
    }

    public void IsHeal_End()
    {
        animator.SetBool("isHeal", false);
        Destroy(transform.parent.gameObject);
    }

    public void IsArmor(bool b = false)
    {
        animator.SetBool("isArmor", true);
    }

    public void IsArmor_End()
    {
        animator.SetBool("isArmor", false);
        Destroy(transform.parent.gameObject);
    }

    public void IsStamina(bool b = false)
    {
        animator.SetBool("isStamina", true);
    }

    public void IsStamina_End()
    {
        animator.SetBool("isStamina", false);
        Destroy(transform.parent.gameObject);
    }

    public void IsMana(bool b = false)
    {
        animator.SetBool("isMana", true);
    }

    public void IsMana_End()
    {
        animator.SetBool("isMana", false);
        Destroy(transform.parent.gameObject);
    }
}
