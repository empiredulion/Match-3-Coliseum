using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GemTypePriority
{
    public GemType gemType;
    public int priority;

    public GemTypePriority(GemType inGemType, int inPriority)
    {
        gemType = inGemType;
        priority = inPriority;
    }
}

[CreateAssetMenu(fileName = "FightingStyle", menuName = "Make3/Fighting Style")]
public class FightingStyle : ScriptableObject
{
    public GemType[] gemTypePriorities = new GemType[6];

    public FightingStyle()
    {
        gemTypePriorities[0] = GemType.ATTACK;
        gemTypePriorities[1] = GemType.MAGIC;
        gemTypePriorities[2] = GemType.STAMINA;
        gemTypePriorities[3] = GemType.MANA;
        gemTypePriorities[4] = GemType.HEAL;
        gemTypePriorities[5] = GemType.SHIELD;
    }
}
