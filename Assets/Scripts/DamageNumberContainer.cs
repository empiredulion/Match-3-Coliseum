using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DamageNumberContainer : MonoBehaviour
{
    [SerializeField] RectTransform widget;
    [SerializeField] DamageNumberText dmt;
    [SerializeField] TMP_Text number;

// Curve
    float duration = 1f;
    float minCurveHeight = 500f;
    float maxCurveHeight = 1000f;
    float maxHorizontal = 500f;
    float endHeight = -100f;
// Curve

    public void SetHeal(int inAmount)
    {
        number.text = "+" + inAmount.ToString();
        ChooseRandomLocation();
        dmt.IsHeal();
    }

    public void SetArmor(int inAmount)
    {
        number.text = "+" + inAmount.ToString();
        ChooseRandomLocation();
        dmt.IsArmor();
    }

    public void SetStamina(int inAmount)
    {
        number.text = "+" + inAmount.ToString();
        ChooseRandomLocation();
        dmt.IsStamina();
    }

    public void SetMana(int inAmount)
    {
        number.text = "+" + inAmount.ToString();
        ChooseRandomLocation();
        dmt.IsMana();
    }

    public void SetHpLost(int inAmount)
    {
        number.text = inAmount.ToString();
        StartCoroutine(DamageFly_Coroutine());
        dmt.IsHpLost();
    }

    public void SetArmorLost(int inAmount)
    {
        number.text = inAmount.ToString();
        StartCoroutine(DamageFly_Coroutine());
        dmt.IsArmorLost();
    }

    IEnumerator DamageFly_Coroutine()
    {
        Vector2 startPos = widget.position;
        Vector2 endPos = new Vector2(Random.Range(startPos.x - maxHorizontal, startPos.x + maxHorizontal), endHeight);
        float randomHeight = Random.Range(minCurveHeight, maxCurveHeight);
        Vector2 controlPoint = Vector2.Lerp(startPos, endPos, 0.5f);
        controlPoint.y = startPos.y + randomHeight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 2. The Quadratic Bezier Formula
            // This interpolates the position along the curve based on t (0 to 1)
            Vector2 m1 = Vector2.Lerp(startPos, controlPoint, t);
            Vector2 m2 = Vector2.Lerp(controlPoint, endPos, t);
            widget.position = Vector2.Lerp(m1, m2, t);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void ChooseRandomLocation()
    {
        Vector2 startPos = widget.position;
        Vector2 newStartPos = new Vector2(Random.Range(startPos.x - 100, startPos.x + 100), Random.Range(startPos.y - 100, startPos.y + 100));
        widget.position = newStartPos;
    }
}
