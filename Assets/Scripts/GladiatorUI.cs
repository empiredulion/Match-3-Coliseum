using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;

public class GladiatorUI : MonoBehaviour
{
    [SerializeField] Image avatar;
    [SerializeField] Image model;
    [SerializeField] TMP_Text displayName;
    [SerializeField] Image hpBar;
    [SerializeField] Image staminaBar;
    [SerializeField] Image manaBar;
    [SerializeField] GameObject armorUI;
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text staminaText;
    [SerializeField] TMP_Text manaText;
    [SerializeField] TMP_Text armorText;

    [SerializeField] GameObject abilityPrefab;
    [SerializeField] GameObject abilitiesHolder;
    Gladiator gladiator;

    public bool isEnemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignGladiator(Gladiator inGlad, bool isPlayer)
    {
        gladiator = inGlad;
        displayName.text = gladiator.displayName;
        avatar.sprite = gladiator.avatar;
        model.sprite = gladiator.model;
        hpText.text = $"{gladiator.currentHP}/{gladiator.GetMaxHP()}";
        hpBar.fillAmount = 1;
        staminaText.text = $"{gladiator.currentStamina}/{gladiator.GetMaxStamina()}";
        staminaBar.fillAmount = (float)gladiator.currentStamina/gladiator.GetMaxStamina();
        manaText.text = $"{gladiator.currentMana}/{gladiator.GetMaxMana()}";
        manaBar.fillAmount = (float)gladiator.currentMana/gladiator.GetMaxMana();
        armorUI.SetActive(false);

        foreach (Ability a in gladiator.abilities)
        {
            GameObject newAbility = Instantiate(abilityPrefab, abilitiesHolder.transform);
            newAbility.GetComponent<AbilityUI>().AssignAbility(a, gladiator, isEnemy);
        }
        
        gladiator.HPChanged.AddListener(UpdateHP);
        gladiator.StaminaChanged.AddListener(UpdateStamina);
        gladiator.ManaChanged.AddListener(UpdateMana);
        gladiator.ArmorChanged.AddListener(UpdateArmor);
    }

    public void MatchEnd()
    {
        gladiator.HPChanged.RemoveListener(UpdateHP);
        gladiator.StaminaChanged.RemoveListener(UpdateStamina);
        gladiator.ManaChanged.RemoveListener(UpdateMana);
        gladiator.ArmorChanged.RemoveListener(UpdateArmor);
    }

    public void UpdateHP(int change)
    {
        hpText.text = $"{gladiator.currentHP}/{gladiator.GetMaxHP()}";
        hpBar.fillAmount = (float)gladiator.currentHP/gladiator.GetMaxHP();
    }

    public void UpdateStamina(int change)
    {
        staminaText.text = $"{gladiator.currentStamina}/{gladiator.GetMaxStamina()}";
        staminaBar.fillAmount = (float)gladiator.currentStamina/gladiator.GetMaxStamina();
    }

    public void UpdateMana(int change)
    {
        manaText.text = $"{gladiator.currentMana}/{gladiator.GetMaxMana()}";
        manaBar.fillAmount = (float)gladiator.currentMana/gladiator.GetMaxMana();
    }

    public void UpdateArmor(int change)
    {
        if (gladiator.GetArmor() > 0)
        {
            armorUI.SetActive(true);
            armorText.text = gladiator.GetArmor().ToString();
        }
        else
        {
            armorUI.SetActive(false);
        }
    }
}
