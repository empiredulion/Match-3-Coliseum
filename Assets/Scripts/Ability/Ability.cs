using UnityEngine;
using System.Collections;

public enum RequiredResource
{
    STAMINA,
    MANA
}

public abstract class Ability : ScriptableObject
{
    [SerializeField] protected string abilityName = "Ability";
    [SerializeField] protected Sprite icon;
    [SerializeField] protected RequiredResource requiredResource;
    [SerializeField] protected int cost;
    [SerializeField] protected string description;

    [SerializeField] protected AudioClip sound;

    public abstract IEnumerator TriggerAbility(Gladiator caster);

    protected void ConsumeEnergy(Gladiator caster)
    {
        if (requiredResource == RequiredResource.STAMINA) caster.StaminaChange(-cost);
        else caster.ManaChange(-cost);
    }

    public string GetName()
    {
        return abilityName;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public RequiredResource GetRequiredResource()
    {
        return requiredResource;
    }

    public int GetCost()
    {
        return cost;
    }

    public abstract string GetDescription();
}
