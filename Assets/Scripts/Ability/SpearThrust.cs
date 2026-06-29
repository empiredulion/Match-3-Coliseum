using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Spear Thrust", menuName = "Make3/Ability/Spear Thrust")]
public class SpearThrust : Ability
{
    [SerializeField] int HpDamage;

    public override IEnumerator TriggerAbility(Gladiator caster)
    {
        ConsumeEnergy(caster);

        caster.DealPhysicalDamage(HpDamage);
        TurnMaster.GetInstance().PlaySoundEffect(sound);
        
        yield return null;
    }

    public override string GetDescription()
    {
        return string.Format(description, HpDamage);
    }
}
