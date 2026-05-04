using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDetails : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text skillName;
    [SerializeField] TMP_Text skillCost;
    [SerializeField] TMP_Text skillDes;

    public void Show(Ability a)
    {
        icon.sprite = a.GetIcon();
        skillName.text = a.GetName();
        string cost = a.GetRequiredResource() == RequiredResource.STAMINA ? $"<color=#FF7F00>{a.GetCost()}</color>" : $"<color=#00FFFF>{a.GetCost()}</color>";
        skillCost.text = "Cost: " + cost;
        skillDes.text = a.GetDescription();

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
