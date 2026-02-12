using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
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

        public ClearableMatch(List<Gem> inGems)
        {
            gems.Clear();
            gems.AddRange(inGems);
        }
    }

    [SerializeField] int xDim;
    [SerializeField] int yDim;
    [SerializeField] Transform boardTransform;
    [SerializeField] List<GameObject> TilePrefabs;
    [SerializeField] private Vector2 gridOffset; // padding for the whole board
    float gemSize = 100f; // actual size is 90, 10 is for spacing
    public Gem[,] grid;
    Gem selectedGem;
    bool isAnimating;
    public int gemFallingCounts = 0;
    public TestBoard testBoard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new Gem[xDim, yDim];
        for (int row = 0; row < xDim; row++) {
            for (int column = 0; column < yDim; column++) {
                MakeNewGem(row, column);
            }
        }
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
        Vector3 tile_pos = GridToWorldPosition(x, y + 5);
        GameObject newTile = Instantiate(TilePrefabs[Random.Range(0, TilePrefabs.Count)], tile_pos, Quaternion.identity);
        newTile.transform.SetParent(boardTransform, false);
                    
        grid[x, y] = newTile.GetComponent<Gem>();
        grid[x, y].GetComponent<Gem>().AssignPosition(x, y);
        grid[x, y].GetComponent<Gem>().AssignBoard(this);

        // Gem falls coroutine
        StartCoroutine(grid[x, y].FallMovement(GridToWorldPosition(x, y)));
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

        ClearAllValidMatches();
    }

    Vector2 GridToWorldPosition(int x, int y)
    {
        return new Vector2(x * gemSize, y * gemSize) + gridOffset;
    }

    public void ClearAllValidMatches()
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
            StartCoroutine(FillEmptySpaces());
        }
    }

    public void ClearGem(int x, int y)
    {
        if (grid[x, y] != null)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    List<ClearableMatch> GetMatches()//Gem inGem, int newX, int newY)
    {
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

                if (!currentGem)
                {
                    if (newMatch.Count >= 3)
                    {
                        horizontalMatches.Add(new ClearableMatch(newMatch));
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

        //Merge match
        List<ClearableMatch> finalMatches = new();
        bool[] willBeMerged = new bool[verticalMatches.Count];

        for (int i = 0; i < horizontalMatches.Count; i++)
        {
            bool isMerged = false;
            for (int j = 0; j < verticalMatches.Count; j++)
            {
                if (horizontalMatches[i].gems.Intersect(verticalMatches[j].gems).Any())
                {
                    finalMatches.Add(new ClearableMatch(horizontalMatches[i].gems.Union(verticalMatches[j].gems).ToList()));
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

        while (gemFallingCounts > 0)
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

        while (gemFallingCounts > 0)
        {
            yield return null;
        }

        ClearAllValidMatches();
    }

    public void OnPointerDown()
    {
        testBoard.Recreate();
    }
}