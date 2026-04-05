using UnityEngine;
using System.Collections;

public class FlyingIcon : MonoBehaviour
{
    public Board board;
    public Transform player1;
    public Transform player2;
    readonly float expandDuration = 0.3f;
    readonly float flyDuration = 1.0f;
    readonly float attackDuration = .2f;
    readonly float rotationDuration = .5f;
    readonly float shrinkTo = 0.2f;
    readonly float rotationSpeed = 300f;

    bool isMagic = false;
    bool isLeadGem = false;
    int gemCount = 0;
    GemType gemType;
    
    [SerializeField] AnimationCurve expandCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    void Update()
    {
        if (isMagic)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartFlying(GemType inGemType, bool isLeadGem, int gemCount)
    {
        gemType = inGemType;
        StartCoroutine(FlyingCoroutine(gemType));
        isMagic = gemType == GemType.MAGIC;
        this.isLeadGem = isLeadGem;
        if (isLeadGem) this.gemCount = gemCount;
    }

    IEnumerator FlyingCoroutine(GemType gemType)
    {
        board.runningCoroutines++;
        // Expanding
        RectTransform rectTransform = GetComponent<RectTransform>();
        float elapsedTime = 0f;
        
        while (elapsedTime < expandDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / expandDuration;
            
            // Apply the curve to get smooth expanding
            float curveValue = 1.0f - expandCurve.Evaluate(t);
            rectTransform.localScale = Vector3.one * curveValue;
            
            yield return null; // Wait one frame
        }

        rectTransform.localScale = Vector3.one;

        // Moving
        Vector3 startPos = rectTransform.position;
        Vector3 endPos;

        if (gemType == GemType.ATTACK)
        {
            yield return MoveStraightLine_Attack();
        }
        else if (gemType == GemType.MAGIC)
        {
            yield return new WaitForSeconds(0.5f);
            yield return MoveStraightLine_Magic();

        }
        else
        {
            yield return MoveCurveLine();
        }

        board.runningCoroutines--;
        if (isLeadGem)
            TurnMaster.GetInstance().PopGems(gemType, gemCount);
            
        Destroy(gameObject);
    }

    IEnumerator MoveCurveLine()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.position;
        Vector3 endPos = (TurnMaster.GetInstance().GetIsPlayerTurn() ? player1.gameObject : player2.gameObject).transform.position;
        
        float elapsed = 0;
    
        // Distance-based height so the curve scales
        float distance = Vector3.Distance(startPos, endPos);
        float curveHeight = distance * 0.5f; 

        // P0: Start
        // P1: Directly above start (creates the vertical launch feel)
        Vector3 p1 = startPos + (Vector3.up * curveHeight);
        
        // P2: Directly above or offset from the destination (creates the smooth entry)
        // We offset it slightly toward the start so the "drop" isn't too vertical
        Vector3 p2 = endPos + (Vector3.up * curveHeight * 0.5f);
        
        // P3: Destination

        while (elapsed < flyDuration)
        {
            float t = elapsed / flyDuration;
            // Optional: float t = Mathf.SmoothStep(0, 1, elapsed / flyDuration);

            // 1. Calculate Position using Cubic Bezier
            Vector3 currentPos = CalculateCubicBezierPoint(t, startPos, p1, p2, endPos);

            // 2. Calculate Velocity (Derivative) for perfectly smooth rotation
            Vector3 velocity = CalculateCubicBezierVelocity(t, startPos, p1, p2, endPos);

            // 3. Update Transform
            rectTransform.position = currentPos;

            if (velocity != Vector3.zero)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                rectTransform.localRotation = Quaternion.Euler(0, 0, angle - 90);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.position = endPos;
        if (isLeadGem) TurnMaster.GetInstance().StartAbsorbingEffect(true, (int)gemType);
    }

    // Math Helper: Cubic Bezier Point
    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; 
        p += 3 * uu * t * p1; 
        p += 3 * u * tt * p2; 
        p += ttt * p3; 
        return p;
    }

    // Math Helper: Derivative of Cubic Bezier (The exact direction of travel)
    Vector3 CalculateCubicBezierVelocity(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        return 3 * u * u * (p1 - p0) + 
            6 * u * t * (p2 - p1) + 
            3 * t * t * (p3 - p2);
    }

    IEnumerator MoveStraightLine_Attack()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.position;
        Vector3 endPos = TurnMaster.GetInstance().GetIsPlayerTurn() ? player2.position : player1.position;
        Vector3 direction = (endPos - startPos).normalized;

        // --- PHASE 1: ROTATE TO FACE TARGET ---
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion startRotation = rectTransform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle + 45);
        
        float elapsedRotation = 0;
        while (elapsedRotation < rotationDuration)
        {
            rectTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedRotation / rotationDuration);
            elapsedRotation += Time.deltaTime;
            yield return null;
        }
        rectTransform.localRotation = targetRotation; // Ensure perfect alignment

        // --- PHASE 2: MOVE TO TARGET ---
        float elapsedMove = 0;
        while (elapsedMove < attackDuration)
        {
            rectTransform.position = Vector3.Lerp(startPos, endPos, elapsedMove / attackDuration);
            elapsedMove += Time.deltaTime;
            yield return null;
        }

        rectTransform.position = endPos;
        if (isLeadGem) TurnMaster.GetInstance().StartAbsorbingEffect(false, (int)gemType);
    }

    IEnumerator MoveStraightLine_Magic()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.position;
        Vector3 endPos = TurnMaster.GetInstance().GetIsPlayerTurn() ? player2.position : player1.position;

        float elapsedMove = 0;
        while (elapsedMove < attackDuration)
        {
            rectTransform.position = Vector3.Lerp(startPos, endPos, elapsedMove / attackDuration);
            elapsedMove += Time.deltaTime;
            yield return null;
        }

        rectTransform.position = endPos;
        if (isLeadGem) TurnMaster.GetInstance().StartAbsorbingEffect(false, (int)gemType);
    }
}
