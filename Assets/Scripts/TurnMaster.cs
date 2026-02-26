using System.Collections;
using UnityEngine;

public class TurnMaster : MonoBehaviour
{
    bool isPlayerTurn = false;
    bool isAnimating = false;
    public int runningCoroutines = 0;

    [SerializeField] Gladiator player1;
    [SerializeField] Gladiator player2;

    [SerializeField] GameObject BoardBlocker;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MatchStart()
    {
        isPlayerTurn = true;
        isAnimating = false;
    }

    public IEnumerator ProcessAction()
    {
        isAnimating = true;
        EnableBoard(false);

        while (runningCoroutines > 0)
        {
            yield return null;
        }

        isPlayerTurn = !isPlayerTurn;
        EnableBoard(isPlayerTurn);

        isAnimating = false;
    }

    void EnableBoard(bool isEnabled)
    {
        //BoardBlocker.SetActive(isEnabled);
    }
}
