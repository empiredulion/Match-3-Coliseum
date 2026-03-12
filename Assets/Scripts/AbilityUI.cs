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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

            UpdateAbilityState(0);
        }
    }

    void UpdateAbilityState(int change)
    {
        bool canAct = TurnMaster.GetInstance().GetIsPlayerTurn() && !gladiator.GetHasUsedAbilityThisTurn();

        button.interactable = canAct && gladiator.HasEnoughToUseAbility(ability);
    }

    void TurnEnd()
    {
        button.interactable = gladiator.HasEnoughToUseAbility(ability);
    }

    public void OnPointerDown()
    {
        gladiator.UseAbility(ability);
    }
}
