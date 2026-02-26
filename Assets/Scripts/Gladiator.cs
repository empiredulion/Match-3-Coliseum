using UnityEngine;

public class Gladiator : ScriptableObject
{
    [SerializeField] int maxHP;
    int currentHP;

    [SerializeField] int maxStamina;
    int currentStamina;

    [SerializeField] int maxMana;
    int currentMana;

    int armorPerGem;
    int currentArmor;

    [SerializeField] int strength;
    [SerializeField] int magic;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopGems()
    {
        
    }

    public void PopAttackGems(int inAmount)
    {
        
    }

    public void PopMagicGems(int inAmount)
    {
        
    }

    public void PopHealGems(int inAmount)
    {
        
    }

    public void PopArmorGems(int inAmount)
    {
        
    }

    public void PopStaminaGems(int inAmount)
    {
        
    }

    public void PopManaGems(int inAmount)
    {
        
    }

    public void GainArmor()
    {
        
    }

    public void DealPhysicalDamage(float inDamage)
    {
        
    }

    public void TakePhysicalDamage(float inDamage)
    {
        float remainingDamage = inDamage;
        if (currentArmor > 0)
        {
            
        }
    }
}
