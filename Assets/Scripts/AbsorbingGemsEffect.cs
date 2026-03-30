using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AbsorbingGemsEffect : MonoBehaviour
{
    readonly float phase1Duration = 0.3f;
    readonly float phase2Duration = 1.0f;
    [SerializeField] Image image;

    public IEnumerator AbsorbingGemsEffect_Coroutine(GemType gemtype)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // --- PHASE 1: FADE IN QUICKLY ---
        float phase1Elapsed = 0f;
        Color startColor = image.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0.8f);

        while (phase1Elapsed < phase1Duration)
        {
            phase1Elapsed += Time.deltaTime;
            image.color = Color.Lerp(startColor, targetColor, phase1Elapsed / phase1Duration);
            yield return null;
        }
        image.color = targetColor;

        // --- PHASE 2: FLOAT UP AND FADE AWAY ---
    }
}
