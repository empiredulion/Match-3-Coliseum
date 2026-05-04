using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text cost;
    [SerializeField] Button button;
    [SerializeField] GameObject blocker;

    Gladiator gladiator;
    Ability ability;

    public void AssignAbility(Ability inAbility, Gladiator inGladiator, bool isEnemy)
    {
        ability = inAbility;
        icon.sprite = ability.GetIcon();
        cost.text = ability.GetCost().ToString();
        cost.color = ability.GetRequiredResource() == RequiredResource.STAMINA ? Color.indianRed : Color.cyan;
        gladiator = inGladiator;
        blocker.SetActive(isEnemy);

        if (!isEnemy)
        {
            gladiator.StaminaChanged.AddListener(UpdateAbilityState);
            TurnMaster.GetInstance().TurnEnds.AddListener(TurnEnd);

            UpdateAbilityState(null, 0);
        }
    }

    void UpdateAbilityState(Gladiator g, int change)
    {
        button.interactable = !gladiator.GetHasUsedAbilityThisTurn() && gladiator.HasEnoughToUseAbility(ability);
    }

    void TurnEnd()
    {
        button.interactable = gladiator.HasEnoughToUseAbility(ability);
    }

    public void OnPointerDown()
    {
        gladiator.UseAbility(ability);
    }

    public void OnPointerEnter()
    {
        TurnMaster.GetInstance().GetSkillDetails().Show(ability);
    }

    public void OnPointerExit()
    {
        TurnMaster.GetInstance().GetSkillDetails().Close();
    }
}
