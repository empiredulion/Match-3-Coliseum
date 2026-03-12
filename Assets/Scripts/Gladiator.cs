using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public delegate void Notify(int damage);

[CreateAssetMenu(fileName = "New Gladiator", menuName = "Make3/New Gladiator", order = 1)]
public class Gladiator : ScriptableObject
{
    [Header("Stat")]
    [SerializeField] int maxHP;
    public int currentHP {get; private set;}

    [SerializeField] int maxStamina;
    public int currentStamina {get; private set;}

    [SerializeField] int maxMana;
    public int currentMana {get; private set;}

    int currentArmor;

    [SerializeField] int strength;
    [SerializeField] int magic;
    [SerializeField] int healPerGem;
    [SerializeField] int armorPerGem;
    [SerializeField] int staminaPerGem;
    [SerializeField] int manaPerGem;

    [Header("Visual")]
    public string displayName;
    public Sprite avatar;
    public Sprite model;

    [Header("Other")]
    public List<Ability> abilities = new();
    [SerializeField] FightingStyle fightingStyle;

    [HideInInspector] public UnityEvent<int> HPChanged;
    [HideInInspector] public UnityEvent<int> StaminaChanged;
    [HideInInspector] public UnityEvent<int> ManaChanged;
    [HideInInspector] public UnityEvent<int> ArmorChanged;

    bool hasUsedAbilityThisTurn = false;
    Board board;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartMatch(Board inboard)
    {
        currentHP = maxHP;
        currentStamina = 100;
        currentMana = 0;
        currentArmor = 0;
        hasUsedAbilityThisTurn = false;
        board = inboard;

        TurnMaster.GetInstance().TurnEnds.AddListener(TurnEnd);
    }

    void TurnEnd()
    {
        hasUsedAbilityThisTurn = false;
        GainArmor(-currentArmor);
    }

    public int GetMaxHP()
    {
        return maxHP;
    }

    public int GetMaxStamina()
    {
        return maxStamina;
    }

    public int GetMaxMana()
    {
        return maxMana;
    }

    public int GetArmor()
    {
        return currentArmor;
    }

    public bool GetHasUsedAbilityThisTurn()
    {
        return hasUsedAbilityThisTurn;
    }

    public void SetHasUsedAbilityThisTurn(bool b)
    {
        hasUsedAbilityThisTurn = b;
        StaminaChanged?.Invoke(0); // Hopefully nothing breaks
    }

    public void PopGems(GemType type, int num)
    {
        switch (type)
        {
            case GemType.ATTACK:
            {
                PopAttackGems(num);
                break;
            }
            case GemType.MAGIC:
            {
                PopMagicGems(num);
                break;
            }
            case GemType.STAMINA:
            {
                PopStaminaGems(num);
                break;
            }
            case GemType.MANA:
            {
                PopManaGems(num);
                break;
            }
            case GemType.SHIELD:
            {
                PopShieldGems(num);
                break;
            }
            case GemType.HEAL:
            {
                PopHealGems(num);
                break;
            }
        }
    }

    public void PopAttackGems(int inAmount)
    {
        TurnMaster.GetInstance().DealPhysicalDamage(inAmount*strength);
    }

    public void PopMagicGems(int inAmount)
    {
        TurnMaster.GetInstance().DealMagicalDamage(inAmount*magic);
    }

    public void PopHealGems(int inAmount)
    {
        GainHeal(inAmount * healPerGem);
    }

    public void PopShieldGems(int inAmount)
    {
        GainArmor(inAmount * armorPerGem);
    }

    public void PopStaminaGems(int inAmount)
    {
        StaminaChange(inAmount * staminaPerGem);
    }

    public void PopManaGems(int inAmount)
    {
        ManaChange(inAmount * manaPerGem);
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Stat calculating
    public void DealPhysicalDamage(float inDamage)
    {
        TurnMaster.GetInstance().DealPhysicalDamage(inDamage);
    }

    public void GainArmor(int inArmor)
    {
        currentArmor += inArmor;
        ArmorChanged?.Invoke(inArmor);
    }

    public void GainHeal(int inHeal)
    {
        int newHP = currentHP + inHeal;
        currentHP = newHP > maxHP ? maxHP : newHP;
        HPChanged?.Invoke(inHeal);
    }

    public void StaminaChange(int amount)
    {
        int newStam = currentStamina + amount;
        currentStamina = newStam < 0 ? 0 : (newStam > maxStamina ? maxStamina : newStam);
        StaminaChanged?.Invoke(amount);
    }

    public void ManaChange(int amount)
    {
        int newMana = currentMana + amount;
        currentMana = newMana < 0 ? 0 : (newMana > maxMana ? maxMana : newMana);
        ManaChanged?.Invoke(amount);
    }

    public void TakePhysicalDamage(float inDamage)
    {
        float remainingDamage = inDamage;
        if (currentArmor > 0)
        {
            remainingDamage = ArmorTakesDamage(inDamage);
        }

        if (remainingDamage >= currentHP)
        {
            currentHP = 0;
            HPChanged?.Invoke(-currentHP);
            TurnMaster.GetInstance().PlayerDead(this);
        }
        else
        {
            int damage = (int)remainingDamage;
            currentHP -= damage;
            HPChanged?.Invoke(-damage);
        }
    }

    public void TakeMagicalDamage(float inDamage)
    {
        
    }

    public float ArmorTakesDamage(float inDamage)
    {
        if (currentArmor <= inDamage)
        {
            float remainingDamage = inDamage - currentArmor;
            currentArmor = 0;
            ArmorChanged?.Invoke(-currentArmor);
            return remainingDamage;
        }
        else
        {
            int damage = (int)inDamage;
            currentArmor -= damage;
            ArmorChanged?.Invoke(-damage);
            return 0;
        }
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Choosing next move
    public PotentialMatch ChooseBestMatch(List<PotentialMatch> inBestMatches)
    {
        return inBestMatches
            .OrderByDescending(m => m.gemCount)
            .ThenBy(m => Array.IndexOf(fightingStyle.gemTypePriorities, m.gemType))
            .First();
    }

    public void Act()
    {
        List<Ability> usableAbilities = GetUsableAbilities();
        if (usableAbilities.Count > 0)
        {
            UseAbility(usableAbilities[0]);
        }

        PotentialMatch bestMove = ChooseBestMatch(board.FindPotentialMatches());
        board.SwapGem_ExternalCall(bestMove.mainGem, bestMove.swapGem, false);
    }

    public bool HasEnoughToUseAbility(Ability a)
    {
        return a.GetRequiredResource() == RequiredResource.STAMINA ? currentStamina >= a.GetCost() : currentMana >= a.GetCost();
    }

    public List<Ability> GetUsableAbilities()
    {
        List<Ability> usableAbilities = new();
        foreach (Ability a in abilities)
        {
            if (HasEnoughToUseAbility(a)) usableAbilities.Add(a);
        }

        return usableAbilities;
    }

    public void UseAbility(Ability a)
    {
        if (!GetHasUsedAbilityThisTurn())
        {
            TurnMaster.GetInstance().TriggerAbility(this, a);
            SetHasUsedAbilityThisTurn(true);
        }
    }
}
