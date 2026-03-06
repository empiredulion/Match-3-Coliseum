using Unity.VisualScripting;

public class PotentialMatch
{
    public Gem mainGem { get; private set;}
    public Gem swapGem { get; private set;}
    public int gemCount { get; private set;}
    public GemType gemType { get; private set;}
    public PotentialMatch(Gem inMain, Gem inSwap, int inCount, GemType inType)
    {
        mainGem = inMain;
        swapGem = inSwap;
        gemCount = inCount;
        gemType = inType;
    }
}
