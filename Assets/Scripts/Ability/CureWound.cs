using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Cure Wound", menuName = "Make3/Ability/Cure Wound")]
public class CureWound : Ability
{
    [SerializeField] int heal;

    public override IEnumerator TriggerAbility(Gladiator caster)
    {
        ConsumeEnergy(caster);

        caster.GainHeal(heal);
        
        yield return null;
    }

    public override string GetDescription()
    {
        return string.Format(description, heal);
    }
}
