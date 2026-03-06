using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Bulwark", menuName = "Make3/Ability/Bulwark")]
public class Bulwark : Ability
{
    [SerializeField] int armorAmount;

    public override IEnumerator TriggerAbility(Gladiator caster)
    {
        ConsumeEnergy(caster);

        caster.GainArmor(armorAmount);
        
        yield return null;
    }

    public override string GetDescription()
    {
        return string.Format(description, armorAmount);
    }
}
