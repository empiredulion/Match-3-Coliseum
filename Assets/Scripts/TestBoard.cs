using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TestBoard : MonoBehaviour
{
    public Gem[,] grid;
    public Board board;

    public GameObject prefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = board.grid;
    }

    // Update is called once per frame
    void Update()
    {
        grid = board.grid;
    }

    public void Recreate()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // Get the child GameObject at the current index
            GameObject child = transform.GetChild(i).gameObject;
            // Destroy the child GameObject
            Destroy(child);
        }

        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                Gem oldGem = grid[x, y];
                if (oldGem != null)
                {
                    GameObject newGemObject = Instantiate(oldGem.gameObject);
                    newGemObject.transform.SetParent(gameObject.transform, false);
                }
                else
                {
                    GameObject newEmptyObject = Instantiate(prefab);
                    newEmptyObject.transform.SetParent(gameObject.transform, false);
                }
            }
        }
    }
}
