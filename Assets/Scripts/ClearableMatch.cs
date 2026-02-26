using System;
using System.Collections.Generic;

public class ClearableMatch
    {
        // public enum MatchType
        // {
        //     NONE,
        //     HORIZONTAL,
        //     VERTICAL,
        //     BOTH,
        // }
        public List<Gem> gems = new();
        public int gemCount { get; private set;}
        public Gem centerGem { get; private set;}

        public ClearableMatch(List<Gem> inGems, Gem inGem)
        {
            gems.Clear();
            gems.AddRange(inGems);
            gemCount = gems.Count;
            centerGem = inGem;
        }

        public ClearableMatch(List<Gem> inGems)
        {
            gems.Clear();
            gems.AddRange(inGems);
            gemCount = gems.Count;
            centerGem = gems[gemCount/2];
        }
    }
