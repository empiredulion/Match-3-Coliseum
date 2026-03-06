using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Ogre Slam", menuName = "Make3/Ability/Ogre Slam")]
public class OgreSlam : Ability
{
    [SerializeField] int HpDamage;
    [SerializeField] int staminaDamage;
    [SerializeField] double chance;

    public override IEnumerator TriggerAbility(Gladiator caster)
    {
        ConsumeEnergy(caster);

        System.Random random = new();
        double r = random.NextDouble();
        if (r < chance)
        {
            Debug.Log("Furry random " + r);
            caster.DealPhysicalDamage(HpDamage);
            TurnMaster.GetInstance().RemoveEnemyStamina(staminaDamage);
        }
        
        yield return null;
    }

    public override string GetDescription()
    {
        return string.Format(description, HpDamage, staminaDamage, chance*100);
    }
}
