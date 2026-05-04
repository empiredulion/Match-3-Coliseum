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
    bool isPlayerTurn = false;
    public int runningCoroutines = 0;

    [SerializeField] Gladiator player1;
    [SerializeField] Gladiator player2;
    [SerializeField] GladiatorUI gladiatorUI1;
    [SerializeField] GladiatorUI gladiatorUI2;
    [SerializeField] GameObject BoardBlocker;
    [SerializeField] Board board;

    [SerializeField] GameObject EndScreen;
    [SerializeField] GameObject WinScreen;
    [SerializeField] GameObject LostScreen;
    [SerializeField] GameObject damageNumberPrefab;
    [SerializeField] GameObject canvas;

    [SerializeField] ModelUI model1;
    [SerializeField] ModelUI model2;
    [SerializeField] Notifier notifier;
    [SerializeField] SkillDetails skillDetails;

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

        player1.HPChanged.AddListener(SpawnHpText);
        player2.HPChanged.AddListener(SpawnHpText);
        player1.ArmorChanged.AddListener(SpawnArmorText);
        player2.ArmorChanged.AddListener(SpawnArmorText);
        player1.StaminaChanged.AddListener(SpawnStaminaText);
        player2.StaminaChanged.AddListener(SpawnStaminaText);
        player1.ManaChanged.AddListener(SpawnManaText);
        player2.ManaChanged.AddListener(SpawnManaText);

        gladiatorUI1.AssignGladiator(player1, true);
        gladiatorUI2.AssignGladiator(player2, false);

        matchState = MatchState.ON_GOING;
    }

    public GameObject GetPlayer1()
    {
        return gladiatorUI1.gameObject;
    }

    public GameObject GetPlayer2()
    {
        return gladiatorUI2.gameObject;
    }

    public void EnqueueAction(IEnumerator coroutine)
    {
        PendingActions.Enqueue(EnablePlayerControl(false));

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

        yield return notifier.Slide_Coroutine(isPlayerTurn);

        if (isPlayerTurn)
        {
            
            BoardBlocker.SetActive(!isPlayerTurn);
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

    public bool GetIsMyTurn(Gladiator inGladiator)
    {
        return isPlayerTurn == (inGladiator == player1);
    }

    public void PlayerDead(Gladiator inPlayer)
    {
        matchState = inPlayer == player1 ? MatchState.PLAYER_2_WON : MatchState.PLAYER_1_WON;
        EndScreen.SetActive(true);

        if (matchState == MatchState.PLAYER_1_WON)
        {
            WinScreen.SetActive(true);
            LostScreen.SetActive(false);
        }
        else
        {
            WinScreen.SetActive(false);
            LostScreen.SetActive(true);
        }
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
        (isPlayerTurn ? model2 : model1).IsHit();
    }

    public void DealMagicalDamage(float inDamage)
    {
        (isPlayerTurn ? player2 : player1).TakeMagicalDamage(inDamage);
        (isPlayerTurn ? model2 : model1).IsHit();
    }

    public void RemoveEnemyStamina(int amount)
    {
        (isPlayerTurn ? player2 : player1).StaminaChange(-amount);
    }

    public void StartAbsorbingEffect(bool onMe, int effectIndex)
    {
        (onMe == isPlayerTurn ? model1 : model2).StartGemsAbsorbingEffect(effectIndex);
    }

    public void EnqueueSkillNotifier(Gladiator inG, string inSkillName)
    {
        bool fromLeft = inG == player1;
        EnqueueAction(notifier.Slide_Coroutine(fromLeft, inSkillName));
    }

    public SkillDetails GetSkillDetails()
    {
        return skillDetails;
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Flying Numbers
    void SpawnHpText(Gladiator g, int change)
    {
        if (change == 0) return;

        GameObject newFlyingNumber = Instantiate(damageNumberPrefab, canvas.transform);
        newFlyingNumber.GetComponent<RectTransform>().position = (g == player1 ? model1 : model2).gameObject.GetComponent<RectTransform>().position;

        if (change > 0)
        {
            newFlyingNumber.GetComponent<DamageNumberContainer>().SetHeal(change);
        }
        else
        {
            newFlyingNumber.GetComponent<DamageNumberContainer>().SetHpLost(change);
        }
    }

    void SpawnArmorText(Gladiator g, int change)
    {
        if (change == 0) return;

        GameObject newFlyingNumber = Instantiate(damageNumberPrefab, canvas.transform);
        newFlyingNumber.GetComponent<RectTransform>().position = (g == player1 ? model1 : model2).gameObject.GetComponent<RectTransform>().position;

        if (change > 0)
        {
            newFlyingNumber.GetComponent<DamageNumberContainer>().SetArmor(change);
        }
        else
        {
            newFlyingNumber.GetComponent<DamageNumberContainer>().SetArmorLost(change);
        }
    }

    void SpawnStaminaText(Gladiator g, int change)
    {
        if (change == 0) return;

        GameObject newFlyingNumber = Instantiate(damageNumberPrefab, canvas.transform);
        newFlyingNumber.GetComponent<RectTransform>().position = (g == player1 ? model1 : model2).gameObject.GetComponent<RectTransform>().position;

        if (change > 0)
        {
            newFlyingNumber.GetComponent<DamageNumberContainer>().SetStamina(change);
        }
    }

    void SpawnManaText(Gladiator g, int change)
    {
        if (change == 0) return;
        
        GameObject newFlyingNumber = Instantiate(damageNumberPrefab, canvas.transform);
        newFlyingNumber.GetComponent<RectTransform>().position = (g == player1 ? model1 : model2).gameObject.GetComponent<RectTransform>().position;

        if (change > 0)
        {
            newFlyingNumber.GetComponent<DamageNumberContainer>().SetMana(change);
        }
    }
}
