using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Notifier : MonoBehaviour
{
    [SerializeField] Image banner;
    [SerializeField] TMP_Text skillName;
    [SerializeField] RectTransform rectTransform;

    [SerializeField] Sprite myBanner;
    [SerializeField] Sprite enemyBanner;

    [SerializeField] Vector3 center = new Vector3(0, 0, 0);
    [SerializeField] Vector3 left = new Vector3(-1500, 0, 0);
    [SerializeField] Vector3 right = new Vector3(1500, 0, 0);

    readonly float moveDuration = 0.5f;
    readonly float stopDuration = 0.5f;

    static WaitForSeconds _waitForSeconds1 = new(1.0f);
    static WaitForSeconds _waitForSeconds2 = new(0.5f);

    public void StartMovingCoroutine(bool fromLeft)
    {
        StartMovingCoroutine(fromLeft);
    }

    public IEnumerator Slide_Coroutine(bool fromLeft, string inSkillName = "")
    {
        skillName.text = inSkillName == "" ? (fromLeft ? "Your Turn" : "Enemy Turn") : inSkillName;
        banner.sprite = fromLeft ? myBanner : enemyBanner;

        Vector3 startPos = right;
        Vector3 endPos = left;

        if (fromLeft)
        {
            (startPos, endPos) = (endPos, startPos);
        }

        float phase1Elapsed = 0;
        while (phase1Elapsed < moveDuration)
        {
            rectTransform.localPosition = Vector3.Lerp(startPos, center, phase1Elapsed / moveDuration);
            phase1Elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.localPosition = center;

        yield return _waitForSeconds1;

        float phase2Elapsed = 0;
        while (phase2Elapsed < moveDuration)
        {
            rectTransform.localPosition = Vector3.Lerp(center, endPos, phase2Elapsed / moveDuration);
            phase2Elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.localPosition = endPos;

        yield return _waitForSeconds2;
    }
}
