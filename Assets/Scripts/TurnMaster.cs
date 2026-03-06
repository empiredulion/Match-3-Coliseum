using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TurnMaster : MonoBehaviour
{
    bool isPlayerTurn = false;
    public int runningCoroutines = 0;

    [SerializeField] Gladiator player1;
    [SerializeField] Gladiator player2;
    [SerializeField] GladiatorUI gladiatorUI1;
    [SerializeField] GladiatorUI gladiatorUI2;
    [SerializeField] GameObject BoardBlocker;
    [SerializeField] Board board;

    [HideInInspector] public UnityEvent TurnEnds;
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

        player1.StartMatch();
        player2.StartMatch();

        gladiatorUI1.AssignGladiator(player1, true);
        gladiatorUI2.AssignGladiator(player2, false);
    }

    public IEnumerator ProcessAction()
    {
        EnableBoard(false);
        
        while (runningCoroutines > 0)
        {
            yield return null;
        }

        isPlayerTurn = false;
        board.totalClearedGems.Clear();
        board.AIAct();

        while (runningCoroutines > 0)
        {
            yield return null;
        }

        isPlayerTurn = true;
        EnableBoard(true);
        board.totalClearedGems.Clear();
        EndTurn();
    }

    public void LetPlayerAct()
    {
        EnableBoard(false);
        isPlayerTurn = true;
    }

    void EnableBoard(bool isEnabled)
    {
        BoardBlocker.SetActive(!isEnabled);
    }

    void EndTurn()
    {
        TurnEnds?.Invoke();
    }

    public bool GetIsPlayerTurn()
    {
        return isPlayerTurn;
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

    public void TriggerAbility(Gladiator inGladiator, Ability inAbility)
    {
        StartCoroutine(inAbility.TriggerAbility(inGladiator));
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
