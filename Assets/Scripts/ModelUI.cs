using UnityEngine;
using System.Collections;

public enum EffectIndex
{
    NONE            = 0,
    ATTACK          = 1,
    MAGIC           = 2,
    STAMINA         = 3,
    MANA            = 4,
    SHIELD          = 5,
    HEAL            = 6,
    EARTH_SHATTER   = 7,
    CURE_WOUND      = 8,
}

public class ModelUI : MonoBehaviour
{
    [SerializeField] GameObject absorbingEffectPrefab;
    [SerializeField] EffectAnimation effectAnimation;
    [SerializeField] Animator animator;
    //float shake_distance = 

    public void StartGemsAbsorbingEffect(int index)
    {
        switch (index)
        {
            case 0: break;
            
            case 1:
            {
                effectAnimation.UseSlash();
                break;
            }
                
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            {
                GameObject newEffect = Instantiate(absorbingEffectPrefab, gameObject.transform);
                newEffect.GetComponent<AbsorbingGemsEffect>().StartAbsorbingGemsEffect(index);
                break;
            }

            default: break;
        }
    }

// Animator /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void ShakeModel(bool b = false)
    {
        animator.SetBool("useShake", true);
    }

    public void ShakeModel_End()
    {
        animator.SetBool("useShake", false);
    }

    public void IsHit(bool b = false)
    {
        animator.SetBool("isHit", true);
    }

    public void IsHit_End()
    {
        animator.SetBool("isHit", false);
    }
}

