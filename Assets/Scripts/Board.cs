using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using System.Runtime.InteropServices;

public class Board : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds2 = new(2);
    [SerializeField] int xDim;
    [SerializeField] int yDim;
    [SerializeField] Transform boardTransform;
    [SerializeField] List<GameObject> TilePrefabs;
    [SerializeField] private Vector2 gridOffset; // padding for the whole board
    float gemSize = 100f; // actual size is 90, 10 is for spacing
    Gem[,] grid;
    Gem selectedGem;
    bool isAnimating;
    public int runningCoroutines = 0;
    System.Random random = new();
    List<GemType> gemTypes = new();
    public TestBoard testBoard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gemTypes.Add(GemType.ATTACK);
        gemTypes.Add(GemType.MAGIC);
        gemTypes.Add(GemType.STAMINA);
        gemTypes.Add(GemType.MANA);
        gemTypes.Add(GemType.SHIELD);
        gemTypes.Add(GemType.HEAL);

        grid = new Gem[xDim, yDim];
        for (int row = 0; row < xDim; row++) {
            for (int column = 0; column < yDim; column++) {
                MakeNewGem(row, column);
            }
        }

        MakeBoardPlayable();

        // for (int x = 0; x < xDim; x++)
        // {
        //     for (int y = 0; y < yDim; y++)
        //     // Gem falls coroutine
        //     StartCoroutine(grid[x, y].FallMovement(GridToWorldPosition(x, y)));
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool GetTileIsNull(int x, int y)
    {
        return grid[x, y] == null;
    }

    public Gem GetGem(int x, int y)
    {
        return grid[x, y];
    }

    void MakeNewGem(int x, int y) {
        Vector3 tile_pos = GridToWorldPosition(x, y);
        GameObject newTile = Instantiate(TilePrefabs[UnityEngine.Random.Range(0, TilePrefabs.Count)], tile_pos, Quaternion.identity);
        newTile.transform.SetParent(boardTransform, false);
                    
        grid[x, y] = newTile.GetComponent<Gem>();
        grid[x, y].GetComponent<Gem>().AssignPosition(x, y);
        grid[x, y].GetComponent<Gem>().AssignBoard(this);
    }

    void MakeNewGem(int x, int y, int inType) {
        Vector3 tile_pos = GridToWorldPosition(x, y);
        GameObject newTile = Instantiate(TilePrefabs[inType], tile_pos, Quaternion.identity);
        newTile.transform.SetParent(boardTransform, false);
                    
        grid[x, y] = newTile.GetComponent<Gem>();
        grid[x, y].GetComponent<Gem>().AssignPosition(x, y);
        grid[x, y].GetComponent<Gem>().AssignBoard(this);
    }

    public bool IsOneGemAlreadySelected()
    {
        return selectedGem != null;
    }

    public void SelectGem(Gem inGem)
    {
        selectedGem = inGem;
    }

    public void UnSelectCurrentGem()
    {
        if (selectedGem)
        {
            selectedGem.DeselectMe();
        }
        selectedGem = null;
    }

    public bool IsSelectedGemAdjacent(Gem inNewGem)
    {
        int rowDiff = Mathf.Abs(selectedGem.GetX() - inNewGem.GetX());
        int colDiff = Mathf.Abs(selectedGem.GetY() - inNewGem.GetY());
        return (rowDiff + colDiff) == 1;
    }
    public IEnumerator SwapGem(Gem inNewGem)
    {
        int x1 = selectedGem.GetX();
        int y1 = selectedGem.GetY();
        int x2 = inNewGem.GetX();
        int y2 = inNewGem.GetY();

        StartCoroutine(selectedGem.SwapMovement(GridToWorldPosition(x2, y2)));
        yield return inNewGem.SwapMovement(GridToWorldPosition(x1, y1));

        inNewGem.AssignPosition(x1, y1);
        selectedGem.AssignPosition(x2, y2);
        grid[x1, y1] = inNewGem;
        grid[x2, y2] = selectedGem;

        UnSelectCurrentGem();

        StartCoroutine(ClearAllValidMatches());
        StartCoroutine(TurnMaster.GetInstance().ProcessAction());
    }

    void SwapGemNoAnim(int x1, int y1, int x2, int y2)
    {
        (grid[x1, y1], grid[x2, y2]) = (grid[x2, y2], grid[x1, y1]);
        grid[x1, y1].AssignPosition(x1, y1);
        grid[x2, y2].AssignPosition(x2, y2);

        grid[x1, y1].gameObject.transform.localPosition = GridToWorldPosition(x1, y1);
        grid[x2, y2].gameObject.transform.localPosition = GridToWorldPosition(x2, y2);

        Debug.Log("Furry Swap Gems at: " + x1 + " " + y1 + " and " + x2 + " " + y2);
    }

    Vector2 GridToWorldPosition(int x, int y)
    {
        return new Vector2((x + 0.5f) * gemSize, (y + 0.5f) * gemSize) + gridOffset;
    }

    IEnumerator ClearAllValidMatches()
    {
        List<ClearableMatch> matches = GetMatches();

        if (matches.Count > 0)
        {
            foreach (ClearableMatch match in matches)
            {
                foreach (Gem gem in match.gems)
                {
                    ClearGem(gem.GetX(), gem.GetY());
                }
            }

            while (runningCoroutines > 0)
            {
                yield return null;
            }

            yield return FillEmptySpaces();
        }

        yield return null;
    }

    public void ClearGem(int x, int y)
    {
        if (grid[x, y] != null)
        {
            StartCoroutine(grid[x, y].ShrinkAndDestroy());
            grid[x, y] = null;
        }
    }

    List<ClearableMatch> GetMatches()//Gem inGem, int newX, int newY)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        List<ClearableMatch> horizontalMatches = new();
        List<ClearableMatch> verticalMatches = new();
        List<Gem> newMatch = new();

        //Check every row
        for (int y = 0; y < yDim; y++)
        {
            int x = 0;
            GemType currentType = GemType.NONE;

            while (x < xDim)
            {
                Gem currentGem = grid[x, y];
                currentGem.SetIsJustMovedFalse();

                if (!currentGem)
                {
                    if (newMatch.Count >= 3)
                    {
                        horizontalMatches.Add(new ClearableMatch(newMatch, null));
                    }

                    newMatch.Clear();
                    x++;
                }
                else
                {
                    if (newMatch.Count == 0)
                    { // First gem
                        newMatch.Add(currentGem);
                        currentType = currentGem.GetGemType();
                        x++;
                    }
                    else
                    {
                        if (currentType == currentGem.GetGemType())
                        { // New gem of same type
                            newMatch.Add(currentGem);
                            x++;
                        }
                        else
                        { // New gem of different type
                            // Long enough
                            if (newMatch.Count >= 3)
                            {
                                horizontalMatches.Add(new ClearableMatch(newMatch));
                            }

                            // Either way, streak is lost
                            newMatch.Clear();
                            newMatch.Add(currentGem);
                            currentType = currentGem.GetGemType();
                            x++;
                        }
                    }
                }
            }
            // End of row
            if (newMatch.Count >= 3)
            {
                horizontalMatches.Add(new ClearableMatch(newMatch));
            }
            newMatch.Clear();
        }

        //Check every column
        for (int x = 0; x < xDim; x++)
        {
            int y = 0;
            GemType currentType = GemType.NONE;

            while (y < yDim)
            {
                Gem currentGem = grid[x, y];

                if (!currentGem)
                {
                    if (newMatch.Count >= 3)
                    {
                        horizontalMatches.Add(new ClearableMatch(newMatch));
                    }

                    newMatch.Clear();
                    y++;
                }
                else
                {
                    if (newMatch.Count == 0)
                    { // First gem
                        newMatch.Add(currentGem);
                        currentType = currentGem.GetGemType();
                        y++;
                    }
                    else
                    {
                        if (currentType == currentGem.GetGemType())
                        { // New gem of same type
                            newMatch.Add(currentGem);
                            y++;
                        }
                        else
                        { // New gem of different type
                            if (newMatch.Count >= 3)
                            { // Old streak is long enough
                                verticalMatches.Add(new ClearableMatch(newMatch));
                            }

                            // Either way, streak is lost
                            newMatch.Clear();
                            newMatch.Add(currentGem);
                            currentType = currentGem.GetGemType();
                            y++;
                        }
                    }
                }
            }
            // End of column
            if (newMatch.Count >= 3)
            {
                verticalMatches.Add(new ClearableMatch(newMatch));
            }
            newMatch.Clear();
        }

        watch.Stop();
        Debug.Log("Furry milliseconds: " + watch.ElapsedTicks);

        //Merge match
        List<ClearableMatch> finalMatches = new();
        bool[] willBeMerged = new bool[verticalMatches.Count];

        for (int i = 0; i < horizontalMatches.Count; i++)
        {
            bool isMerged = false;
            for (int j = 0; j < verticalMatches.Count; j++)
            {
                List<Gem> sameGem = horizontalMatches[i].gems.Intersect(verticalMatches[j].gems).ToList();
                if (sameGem.Count > 0)
                {
                    finalMatches.Add(new ClearableMatch(horizontalMatches[i].gems.Union(verticalMatches[j].gems).ToList(), sameGem[0]));
                    isMerged = true;
                    willBeMerged[j] = true;
                }
            }

            if (!isMerged)
            {
                finalMatches.Add(horizontalMatches[i]);
            }
        }

        for (int i = 0; i < verticalMatches.Count; i++)
        {
            if (!willBeMerged[i])
            {
                finalMatches.Add(verticalMatches[i]);
            }
        }

        return finalMatches;
    }

    IEnumerator FillEmptySpaces()
    {
        for (int x = 0; x < xDim; x++)
        {
            // Fall first
            int moveDownSteps = 0;
            for (int y = 0; y < yDim; y++)
            {
                if (grid[x, y] == null)
                { // All above gems have to move down 1 more tile
                    moveDownSteps++;
                }
                else
                { // Fall bitch
                    if (moveDownSteps > 0)
                    { // Probabaly more efficient
                        Gem gem = grid[x, y];
                        StartCoroutine(gem.FallMovement(GridToWorldPosition(x, y - moveDownSteps)));
                        gem.AssignPosition(x, y - moveDownSteps);
                        grid[x, y - moveDownSteps] = gem;
                        grid[x, y] = null;
                    }
                }
            }
        }
        while (runningCoroutines > 0)
        {
            yield return null;
        }

        // Now create new gems
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (grid[x, y] == null)
                {
                    MakeNewGem(x, y);
                }
            }
        }
        while (runningCoroutines > 0)
        {
            yield return null;
        }
        
        yield return ClearAllValidMatches();
    }

    // Link for ref: https://gamedev.stackexchange.com/questions/84501/how-to-implement-a-hint-system-for-nearby-matches-in-a-match-3-puzzle-game
    public void FindPotentialMatches()
    {
        List<PotentialMatch> bestMatches = new();
        for (int y = 0; y < yDim; y++)
        {
            int x = 0;
            GemType currentType = GemType.NONE;
            List<PotentialMatch> potentialMatches = new();

            while (x < xDim)
            {
                Gem currentGem = grid[x, y];
                currentType = currentGem.GetGemType();

                //          || || || 01 || || ||
                //          || || 12 NN 02 || ||
                //          || 11 NW    NE 03 ||
                //          10 WW    CC    EE 04
                //          || 09 SW    SE 05 ||
                //          || || 08 SS 06 || ||
                //          || || || 07 || || ||
                Gem gemN, gemNE, gemE, gemSE, gemS, gemSW, gemW, gemNW;
                gemN = gemNE = gemE = gemSE = gemS = gemSW = gemW = gemNW = null;

                if (y+3 < yDim)                 gemN    = grid[x, y+2];
                if (x+1 < xDim && y+1 < yDim)   gemNE   = grid[x+1, y+1];
                if (x+3 < xDim)                 gemE    = grid[x+2, y];
                if (x+1 < xDim && y-1 >= 0)     gemSE   = grid[x+1, y-1];
                if (y-3 >= 0)                   gemS    = grid[x, y-2];
                if (x-1 >= 0 && y-1 > 0)        gemSW   = grid[x-1, y-1];
                if (x-3 >= 0)                   gemW    = grid[x-2, y];
                if (x-1 >= 0 && y+1 < yDim)     gemNW   = grid[x-1, y+1];

                bool matchNN01, matchNE02, matchNE03, matchEE04, matchSE05, matchSE06, matchSS07, matchSW08, matchSW09, matchWW10, matchNW11, matchNW12;
                matchNN01 = matchNE02 = matchNE03 = matchEE04 = matchSE05 = matchSE06 = matchSS07 = matchSW08 = matchSW09 = matchWW10 = matchNW11 = matchNW12 = false;
                bool matchWNE, matchNSE, matchESW, matchSWN;
                matchWNE = matchNSE = matchESW = matchSWN = false;

                if (gemN && gemN.GetGemType() == currentType)
                {
                    if (grid[x, y+2].GetGemType() == currentType)
                    {
                        matchNN01 = true;
                    }
                }

                if (gemE && gemE.GetGemType() == currentType)
                {
                    if (grid[x + 3, y].GetGemType() == currentType)
                    {
                        matchEE04 = true;
                    }
                }

                if (gemS && gemS.GetGemType() == currentType)
                {
                    if (grid[x, y-2].GetGemType() == currentType)
                    {
                        matchSS07 = true;
                    }
                }

                if (gemW && gemW.GetGemType() == currentType)
                {
                    if (grid[x-3, y].GetGemType() == currentType)
                    {
                        matchWW10 = true;
                    }
                }

                if (gemNE && gemNE.GetGemType() == currentType)
                {
                    if (x+2 < xDim && grid[x+2, y+1].GetGemType() == currentType)
                    {
                        matchNE03 = true;
                    }
                    if (y+2 < yDim && grid[x+1, y+2].GetGemType() == currentType)
                    {
                        matchNE02 = true;
                    }
                    if (gemNW && gemNW.GetGemType() == currentType)
                    {
                        matchWNE = true;
                    }
                    if (gemSE && gemSE.GetGemType() == currentType)
                    {
                        matchNSE = true;
                    }
                }

                if (gemSW && gemSW.GetGemType() == currentType)
                {
                    if (x-2 >= 0 && grid[x-2, y-1].GetGemType() == currentType)
                    {
                        matchSW09 = true;
                    }
                    if (y-2 >= 0 && grid[x-1, y-2].GetGemType() == currentType)
                    {
                        matchSW08 = true;
                    }
                    if (gemSE && gemSE.GetGemType() == currentType)
                    {
                        matchESW = true;
                    }
                    if (gemNW && gemNW.GetGemType() == currentType)
                    {
                        matchSWN = true;
                    }
                }

                if (gemSE && gemSE.GetGemType() == currentType)
                {
                    if (x+2 < xDim && grid[x+2, y-1].GetGemType() == currentType)
                    {
                        matchSE05 = true;
                    }
                    if (y-2 >= 0 && grid[x+1, y-2].GetGemType() == currentType)
                    {
                        matchSE06 = true;
                    }
                }

                if (gemNW && gemNW.GetGemType() == currentType)
                {
                    if (x-2 >= 0 && grid[x-2, y+1].GetGemType() == currentType)
                    {
                        matchNW11 = true;
                    }
                    if (y+2 < yDim && grid[x-1, y+2].GetGemType() == currentType)
                    {
                        matchNW12 = true;
                    }
                }

                if (matchNN01)
                {
                    if (matchNE03 && matchNW11)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y+1], 7, currentType));
                    }
                    else if (matchWNE && (matchNE03 || matchNW11))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y+1], 6, currentType));
                    }
                    else if (matchNE03 || matchNW11)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y+1], 5, currentType));
                    }
                    else if ((matchWNE && matchNE03 && !matchNW11) || (matchWNE && !matchNE03 && matchNW11))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y+1], 4, currentType));
                    }
                    else
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y+1], 3, currentType));
                    }
                }

                if (matchEE04)
                {
                    if (matchNE02 && matchSE06)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x+1, y], 7, currentType));
                    }
                    else if (matchNSE && (matchNE02 || matchSE06))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x+1, y], 6, currentType));
                    }
                    else if (matchNE02 || matchSE06)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x+1, y], 5, currentType));
                    }
                    else if ((matchNSE && matchNE02 && !matchSE06) || (matchNSE && !matchNE02 && matchSE06))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x+1, y], 4, currentType));
                    }
                    else
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x+1, y], 3, currentType));
                    }
                }

                if (matchSS07)
                {
                    if (matchSE05 && matchSW09)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y-1], 7, currentType));
                    }
                    else if (matchESW && (matchSE05 || matchSW09))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y-1], 6, currentType));
                    }
                    else if (matchSE05 || matchSW09)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y-1], 5, currentType));
                    }
                    else if ((matchESW && matchSW09 && !matchSE05) || (matchESW && !matchSW09 && matchSE05))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y-1], 4, currentType));
                    }
                    else
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x, y-1], 3, currentType));
                    }
                }

                if (matchWW10)
                {
                    if (matchSW08 && matchNW12)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x-1, y], 7, currentType));
                    }
                    else if (matchSWN && (matchSW08 || matchNW12))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x-1, y], 6, currentType));
                    }
                    else if (matchSW08 || matchNW12)
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x-1, y], 5, currentType));
                    }
                    else if ((matchSWN && matchSW08 && !matchNW12) || (matchSWN && !matchSW08 && matchNW12))
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x-1, y], 4, currentType));
                    }
                    else
                    {
                        potentialMatches.Add(new PotentialMatch(currentGem, grid[x-1, y], 3, currentType));
                    }
                }

                PotentialMatch bestMatch = null;
                if (potentialMatches.Count > 0)
                {
                    bestMatch = potentialMatches.OrderByDescending(x => x.gemCount)
                                                .ThenByDescending(x => x.gemType)
                                                .First();
                    bestMatches.Add(bestMatch);
                }
                
                x++;
            }
        }

        PotentialMatch bestOfBests = null;
        if (bestMatches.Count > 0)
        {
            bestOfBests = bestMatches.OrderByDescending(x => x.gemCount)
                                    .ThenByDescending(x => x.gemType)
                                    .First();
        }
        return;
    }

    void RandomizeBoard()
    {
        int n = xDim * yDim;
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);

            int iRow = i / yDim;
            int iCol = i % xDim;
            int jRow = j / yDim;
            int jCol = j % xDim;

            SwapGemNoAnim(iRow, iCol, jRow, jCol);
        }
    }

    // No initial matches but has valid moves
    public void MakeBoardPlayable()
    {
        List<ClearableMatch> clearableMatches = GetMatches();
        if (clearableMatches.Count > 0)
        {
            List<Gem> swappedGems = new();
            foreach (ClearableMatch match in clearableMatches)
            {
                if (match.gemCount < 4)
                {
                    Gem middleGem = match.gems[1];

                    if (swappedGems.Contains(middleGem))
                        break;

                    GemType gemType = middleGem.GetGemType();
                    int mX = middleGem.GetX();
                    int mY = middleGem.GetY();

                    if (mX+1 < xDim && grid[mX+1, mY].GetGemType() != gemType && middleGem.OldCord != (mX+1, mY))
                    {
                        middleGem.OldCord = (mX, mY);
                        grid[mX+1, mY].OldCord = (mX+1, mY);

                        SwapGemNoAnim(mX, mY, mX+1, mY);

                        swappedGems.Add(middleGem);
                        swappedGems.Add(grid[mX+1, mY]);
                    }
                    else if (mX-1 > -1 && grid[mX-1, mY].GetGemType() != gemType && middleGem.OldCord != (mX-1, mY))
                    {
                        middleGem.OldCord = (mX, mY);
                        grid[mX-1, mY].OldCord = (mX-1, mY);

                        SwapGemNoAnim(mX, mY, mX-1, mY);

                        swappedGems.Add(middleGem);
                        swappedGems.Add(grid[mX-1, mY]);
                    }
                    else if (mY+1 < yDim && grid[mX, mY+1].GetGemType() != gemType && middleGem.OldCord != (mX, mY+1))
                    {
                        middleGem.OldCord = (mX, mY);
                        grid[mX, mY+1].OldCord = (mX, mY+1);

                        SwapGemNoAnim(mX, mY, mX, mY+1);

                        swappedGems.Add(middleGem);
                        swappedGems.Add(grid[mX, mY+1]);
                    }
                    else if (mY-1 > -1 && grid[mX, mY-1].GetGemType() != gemType && middleGem.OldCord != (mX, mY-1))
                    {
                        middleGem.OldCord = (mX, mY);
                        grid[mX, mY-1].OldCord = (mX, mY-1);

                        SwapGemNoAnim(mX, mY, mX, mY-1);

                        swappedGems.Add(middleGem);
                        swappedGems.Add(grid[mX, mY-1]);
                    }
                    else
                    {
                        ChangeGemTypeRandom(mX, mY);
                    }
                }
                else
                {
                    ChangeGemTypeRandom(match.centerGem.GetX(), match.centerGem.GetY());
                }
            }

            MakeBoardPlayable();
        }
    }

    public void OnPointerDown()
    {
        //testBoard.Recreate();
    }

    public void ChangeGemTypeRandom(int gX, int gY)
    {
        List<GemType> randomGemTypes = new();
        randomGemTypes.AddRange(gemTypes);
        randomGemTypes.Remove(grid[gX, gY].GetGemType());
        
        if (gX+1 < xDim)
        {
            randomGemTypes.Remove(grid[gX+1, gY].GetGemType());
        }
        else if (gX - 1 > -1)
        {
            randomGemTypes.Remove(grid[gX-1, gY].GetGemType());
        }
        else if (gY+1 < yDim)
        {
            randomGemTypes.Remove(grid[gX, gY+1].GetGemType());
        }
        else if (gY-1 > -1)
        {
            randomGemTypes.Remove(grid[gX, gY-1].GetGemType());
        }

        Destroy(grid[gX, gY].gameObject);
        grid[gX, gY] = null;

        MakeNewGem(gX, gY, UnityEngine.Random.Range(0, randomGemTypes.Count));
    }

    public void ResetBoard()
    {
        foreach (Gem gem in grid)
        {
            Destroy(gem.gameObject);
        }

        System.Array.Clear(grid, 0, grid.Length);
        
        grid = new Gem[xDim, yDim];
        for (int row = 0; row < xDim; row++) {
            for (int column = 0; column < yDim; column++) {
                MakeNewGem(row, column);
            }
        }

        StartCoroutine(ResetBoardCoroutine());
    }

    IEnumerator ResetBoardCoroutine()
    {
        yield return _waitForSeconds2;
        MakeBoardPlayable();
    }
}