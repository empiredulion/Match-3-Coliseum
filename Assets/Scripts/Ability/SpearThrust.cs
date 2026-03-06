using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Spear Thrust", menuName = "Make3/Ability/Spear Thrust")]
public class SpearThrust : Ability
{
    [SerializeField] int HpDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override IEnumerator TriggerAbility(Gladiator caster)
    {
        ConsumeEnergy(caster);

        caster.DealPhysicalDamage(HpDamage);
        
        yield return null;
    }

    public override string GetDescription()
    {
        return string.Format(description, HpDamage);
    }
}
