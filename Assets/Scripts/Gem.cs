using UnityEngine;
using System.Collections;

public enum GemType
{
    NONE = 0,
    ATTACK = 1,
    MAGIC = 2,
    STAMINA = 3,
    MANA = 4,
    SHIELD = 5,
    HEAL = 6
}

public class Gem : MonoBehaviour
{
    [SerializeField] GemType gemType;
    [SerializeField] RectTransform rectTransform;

    [SerializeField]int gridX;
    [SerializeField]int gridY;
    public int heightOffset;
    Board board;
    readonly float speed = 8f;
    bool isSelected;
    bool isJustMoved;
    public (int x, int y) OldCord = (-1, -1);
    readonly float shrinkDuration = 0.3f;
    [SerializeField] AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isJustMoved = true;
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
        // Debug.Log("My cord " + gridX + " " + gridY);
        // Gem thatBitch = board.GetGem(gridX, gridY);
        // if (thatBitch != this)
        // {
        //     Debug.Log("Board-chan think I'm " + thatBitch.GetX() + thatBitch.GetY());
        // }

        // board.MakeBoardPlayable();

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
                    TurnMaster.GetInstance().EnqueueAction(board.SwapGem(this));
                    TurnMaster.GetInstance().EnqueueChangeTurn(false);
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
            if (this == null) yield break;

            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);            
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localPosition = targetPosition;
        //yield return new WaitForSeconds(.5f);
        
        isJustMoved = true;
        ResetOldCord();
    }

    public IEnumerator FallMovement()
    {
        board.runningCoroutines++;
        Vector2 targetPosition = board.GridToWorldPosition(gridX, gridY);

        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            if (this == null) yield break;

            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, 
                targetPosition, 
                speed * Time.deltaTime * 100
            );
            yield return null;
        }

        transform.localPosition = targetPosition;
        //yield return new WaitForSeconds(.5f);
        
        board.runningCoroutines--;
        isJustMoved = true;
        yield return null;
        ResetOldCord();
    }

    public  IEnumerator ShrinkAndDestroy()
    {
        board.runningCoroutines++;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 originalScale = rectTransform.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;
            
            // Apply the curve to get smooth shrinking
            float curveValue = shrinkCurve.Evaluate(t);
            rectTransform.localScale = originalScale * curveValue;
            
            yield return null; // Wait one frame
        }
        
        // Ensure it's completely shrunk
        rectTransform.localScale = Vector3.zero;
        
        // Destroy the game object
        Destroy(gameObject);

        board.runningCoroutines--;
    }

    public void SetIsJustMovedFalse()
    {
        isJustMoved = false;
    }

    public void ResetOldCord()
    {
        OldCord = (-1, -1);
    }
}
