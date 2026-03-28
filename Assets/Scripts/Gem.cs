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

    [SerializeField] FlyingIcon flyingIcon;

    public int heightOffset;
    Board board;
    Transform Canvas;
    readonly float speed = 8f;
    bool isSelected;
    bool isJustMoved;
    public (int x, int y) OldCord = (-1, -1);
    readonly float shrinkDuration = 0.3f;
    [SerializeField] AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    bool  runningCoroutines;
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

    public void AssignBoard(Board inBoard, Transform inCanvas, Transform inPlayer1, Transform inPlayer2)
    {
        board = inBoard;
        flyingIcon.board = inBoard;
        Canvas = inCanvas;
        flyingIcon.player1 = inPlayer1;
        flyingIcon.player2 = inPlayer2;
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
                    TurnMaster.GetInstance().EnqueueEndTurn();
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
        
        isJustMoved = true;
        ResetOldCord();
    }

    public IEnumerator FallMovement()
    {
        board.runningCoroutines++;
        runningCoroutines = true;
        Vector2 targetPosition = board.GridToWorldPosition(gridX, gridY);

        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            yield return null; // Wait for next frame

            // Check if we were destroyed while waiting for the next frame
            if (this == null) yield break;

            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, 
                targetPosition, 
                speed * Time.deltaTime * 100
            );
        }     

        transform.localPosition = targetPosition;
        board.runningCoroutines--;
        runningCoroutines = false;
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
        
        flyingIcon.gameObject.transform.SetParent(Canvas, true);
        flyingIcon.StartFlying(gemType);

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
        board.runningCoroutines--;
        Destroy(gameObject);
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
