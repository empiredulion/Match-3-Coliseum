using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MatchState
{
    ON_GOING,
    PLAYER_1_WON,
    PLAYER_2_WON    
}

public class TurnMaster : MonoBehaviour
{
    public bool isPlayerTurn = false;
    public int runningCoroutines = 0;

    [SerializeField] Gladiator player1;
    [SerializeField] Gladiator player2;
    [SerializeField] GladiatorUI gladiatorUI1;
    [SerializeField] GladiatorUI gladiatorUI2;
    [SerializeField] GameObject BoardBlocker;
    [SerializeField] Board board;

    [HideInInspector] public UnityEvent TurnEnds;

    Queue<IEnumerator> PendingActions = new();
    Coroutine currentRunningCoroutine = null;

    MatchState matchState;

    private static TurnMaster instance;

    private void Awake() {
        if (instance != null) {
            Debug.Log("Found more than one TurnMaster in the scene");
        }
        else instance = this;
    }

    public static TurnMaster GetInstance() {
        return instance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MatchStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MatchStart()
    {
        isPlayerTurn = true;

        player1.StartMatch(board);
        player2.StartMatch(board);

        gladiatorUI1.AssignGladiator(player1, true);
        gladiatorUI2.AssignGladiator(player2, false);

        matchState = MatchState.ON_GOING;
    }

    public void EnqueueAction(IEnumerator coroutine)
    {
        EnablePlayerControl(false);

        PendingActions.Enqueue(coroutine);

        currentRunningCoroutine ??= StartCoroutine(CoroutineCoordinator());
    }

    private IEnumerator CoroutineCoordinator()
    {
        while (PendingActions.Count > 0)
        {
            if (matchState != MatchState.ON_GOING)
            {
                PendingActions.Clear();
                ShowMatchResult();
                break;
            }

            IEnumerator nextCoroutine = PendingActions.Dequeue();
            yield return StartCoroutine(nextCoroutine);
        }

        currentRunningCoroutine = null;
        yield return EnablePlayerControl(true);
    }

    public IEnumerator SetIsPlayerTurn(bool b)
    {
        isPlayerTurn = b;
        yield return null;
    }

    public IEnumerator EnablePlayerControl(bool b)
    {
        BoardBlocker.SetActive(!b);
        yield return null;
    }

    public void EnqueueEndTurn()
    {
        EnqueueAction(EndTurn());
    }

    IEnumerator EndTurn()
    {
        TurnEnds?.Invoke();
        isPlayerTurn = !isPlayerTurn;

        if (isPlayerTurn)
        {
            yield return EnablePlayerControl(true);
        }
        else
        {
            yield return player2.Act();
        }
    }

    public bool GetIsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public void PlayerDead(Gladiator inPlayer)
    {
        matchState = inPlayer == player1 ? MatchState.PLAYER_2_WON : MatchState.PLAYER_1_WON;
    }

    public bool IsMatchFinished()
    {
        return !(matchState == MatchState.ON_GOING);
    }

    void ShowMatchResult()
    {
        //Trigger pop up
        Debug.Log("Match ends");
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Gladiators
    public void PopGems(GemType type, int num)
    {
        if (isPlayerTurn)
        {
            player1.PopGems(type, num);
        }
        else
        {
            player2.PopGems(type, num);
        }
    }

    public void DealPhysicalDamage(float inDamage)
    {
        (isPlayerTurn ? player2 : player1).TakePhysicalDamage(inDamage);
    }

    public void DealMagicalDamage(float inDamage)
    {
        (isPlayerTurn ? player2 : player1).TakeMagicalDamage(inDamage);
    }

    public void RemoveEnemyStamina(int amount)
    {
        (isPlayerTurn ? player2 : player1).StaminaChange(-amount);
    }
}
