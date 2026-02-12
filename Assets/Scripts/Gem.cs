using UnityEngine;
using System.Collections;

public enum GemType
    {
        NONE,
        ATTACK,
        MAGIC,
        STAMINA,
        MANA,
        HEAL,
        SHIELD
    }

public class Gem : MonoBehaviour
{
    [SerializeField] GemType gemType;
    [SerializeField] RectTransform rectTransform;

    [SerializeField]    int gridX;
    [SerializeField] int gridY;
    Board board;
    readonly float speed = 5f;
    bool isSelected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignPosition(int inX, int inY)
    {
        gridX = inX;
        gridY = inY;
    }

    public void AssignBoard(Board inBoard)
    {
        board = inBoard;
    }

    public int GetX()
    {
        return gridX;
    }

    public int GetY()
    {
        return gridY;
    }

    public string GetXY()
    {
        return $"({gridX}, {gridY})";
    }

    public GemType GetGemType()
    {
        return gemType;
    }

    public void SelectMe()
    {
        isSelected = true;
        GetComponent<UnityEngine.UI.Image>().color = Color.grey;
        board.UnSelectCurrentGem();
        board.SelectGem(this);
    }

    public void DeselectMe()
    {
        isSelected = false;
        GetComponent<UnityEngine.UI.Image>().color = Color.white;
    }

    public void OnPointerDown()
    {
        Gem thatBitch = board.GetGem(gridX, gridY);
        if (thatBitch != this)
        {
            Debug.Log("Board-chan think I'm " + thatBitch.GetX() + thatBitch.GetY());
        }

        if (isSelected)
        {
            DeselectMe();
            board.UnSelectCurrentGem();
        }
        else
        {
            if (board.IsOneGemAlreadySelected())
            {
                if (board.IsSelectedGemAdjacent(this))
                {
                    StartCoroutine(board.SwapGem(this));
                }
                else
                {
                    SelectMe();
                }
            }
            else
            {
                SelectMe();
            }
        }
    }

    public IEnumerator SwapMovement(Vector2 targetPosition)
    {
        float duration = .5f;
        float elapsedTime = 0f;
        Vector3 startPosition = rectTransform.localPosition;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);            
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localPosition = targetPosition;
    }

    public IEnumerator FallMovement(Vector2 targetPosition)
    {
        board.gemFallingCounts++;
        Vector3 startPosition = rectTransform.localPosition;

        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, 
                targetPosition, 
                speed * Time.deltaTime * 100
            );
            yield return null;
        }
        
        transform.localPosition = targetPosition;
        board.gemFallingCounts--;
        yield return null;
    }
}
