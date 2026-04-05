using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AbsorbingGemsEffect : MonoBehaviour
{
    readonly float phase1Duration = 0.3f;
    readonly float phase2Duration = 1.0f;
    [SerializeField] Image image;
    [SerializeField] List<Sprite> sprites;
    public IEnumerator AbsorbingGemsEffect_Coroutine()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float phase2Elapsed = 0f;
        Color startColor2 = image.color;
        Color targetColor2 = new Color(startColor2.r, startColor2.g, startColor2.b, 0);
        
        while (phase2Elapsed < phase2Duration)
        {
            phase2Elapsed += Time.deltaTime;
            float t = phase2Elapsed / phase2Duration;
            
            float curveValue = 1.0f + t * 0.3f;
            rectTransform.localScale = Vector3.one * curveValue;
            
            image.color = Color.Lerp(startColor2, targetColor2, phase2Elapsed / phase2Duration);

            yield return null;
        }

        Destroy(gameObject);
    }

    public IEnumerator Shake_Coroutine()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float phase2Elapsed = 0f;
        Color startColor1 = image.color;
        Color startColor2 = new Color(startColor1.r, startColor1.g, startColor1.b, 1);
        Color targetColor2 = new Color(startColor2.r, startColor2.g, startColor2.b, 0);
        
        while (phase2Elapsed < phase2Duration)
        {
            phase2Elapsed += Time.deltaTime;
            float t = phase2Elapsed / phase2Duration;
            
            float curveValue = 1.0f + t * 0.3f;
            rectTransform.localScale = Vector3.one * curveValue;
            
            image.color = Color.Lerp(startColor2, targetColor2, phase2Elapsed / phase2Duration);

            yield return null;
        }

        Destroy(gameObject);
    }

    public void StartAbsorbingGemsEffect(int index)
    {
        image.sprite = sprites[index];
        if (index == 7)
        {
            StartCoroutine(Shake_Coroutine());
        }
        else
        {
            StartCoroutine(AbsorbingGemsEffect_Coroutine());
        }
    }
}
