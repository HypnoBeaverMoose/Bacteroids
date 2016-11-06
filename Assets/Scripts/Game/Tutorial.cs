using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tutorial : MonoBehaviour
{
    public enum HintEvent
    {
        PlayerDead = 0,
        BacteriaDead,
        BacteriaSplit,
        BacteriaSplitAlone,
        BacteriaSplitByPlayer,
        BacteriaMutate,
        BacteriaMutateByHit,
        BacteriaMutateByConsume,
        BacteriaMutatesByBacteria,
        BacteriaMutatesByTouch,
        BacteriaMutatesBySplit,
        EnergyConsumedByPlayer,
        EnergyConsumedByBacteria,
        PlayerChangesColor

    }
    public static bool NeedsTutorial { get { return PlayerPrefs.GetString("Tutorial", "False") == "False"; } }
    public static Tutorial Instance { get; private set; }
    public static bool SavePlayer { get { return !PlayerPrefs.HasKey(HintEvent.PlayerDead.ToString()); } }

    private Player _player = null;
    public event System.Action TutorialComplete;
    private SpawnController _spawn;
    private float _lastMessage;
    [SerializeField]
    private float _shipMovementAmount;
    [SerializeField]
    private int _shipFireAmount;


    void Awake()
    {
        
        Instance = this;
        Player.PlayerSpawned += (Player p) => { _player = p; };
        _spawn = GetComponent<SpawnController>();
    }

    public void StartTutorial()
    {
        StartCoroutine(TutorialRoutine());
    }

    public void ShowHintMessage(HintEvent hint, Vector2 position)
    {
        if (PlayerPrefs.HasKey(hint.ToString()) || (Time.time - _lastMessage) < Tooltip.DefaultDelay)
        {            
            return;
        }
        ShowHintMessage(hint);
        EventBox.Instance.Show(position);
    }

    public void ShowHintMessage(HintEvent hint)
    {
        if (PlayerPrefs.HasKey(hint.ToString()) || (Time.time - _lastMessage) < Tooltip.DefaultDelay)
        {
            return; 
        }
        _lastMessage = Time.time;
        switch (hint)
        {
            case HintEvent.PlayerDead:
                Tooltip.Instance.ShowText("TOUCHING THE BACTERIA CAN <color=#a52a2aff>KILL</color> <color=#a52a2aff>YOU</color>");
                break;
            case HintEvent.BacteriaDead:
                Tooltip.Instance.ShowText("GREAT JOB! NOW DON'T FORGET TO COLLECT THE <color=#a52a2aff>ENERGY</color>");
                break;
            case HintEvent.BacteriaSplitAlone:
                Tooltip.Instance.ShowText("<color=#a52a2aff>BACTERIA</color> CAN <color=#a52a2aff>SPLIT</color> WHEN BIG ENOUGH");
                break;
            case HintEvent.BacteriaSplitByPlayer:
                Tooltip.Instance.ShowText("BIG BACTERIA <color=#a52a2aff>SPLIT</color> WHEN YOU HIT THEM.");
                break;
            case HintEvent.BacteriaMutate:
                Tooltip.Instance.ShowText("A BACTERIA HAS DEVELOPED <color=#a52a2aff>RESISTANCE</color>");
                break;
            case HintEvent.BacteriaMutatesByTouch:
                Tooltip.Instance.ShowText("TOUCHING A BACTERIA DEVELOPS <color=#a52a2aff>RESISTANCE</color>!");
                break;
            case HintEvent.BacteriaMutateByHit:
                Tooltip.Instance.ShowText("BACTERIAS THAT GET HIT CAN <color=#a52a2aff>MUTATE</color>");
                break;
            case HintEvent.BacteriaMutatesBySplit:
                Tooltip.Instance.ShowText("SPLITTING MIGHT CAUSE BACTERIA TO <color=#a52a2aff>MUTATE</color>");
                break;
            case HintEvent.BacteriaMutatesByBacteria:
                Tooltip.Instance.ShowText("<color=#a52a2aff>MUTATED</color> BACTERIA CAN INFECT <color=#a52a2aff>OTHERS</color>");
                break;
            case HintEvent.BacteriaMutateByConsume:
                Tooltip.Instance.ShowText("MUTATED ENERGY CAUSES BACTERIA TO <color=#a52a2aff>MUTATE</color>");
                break;
            case HintEvent.EnergyConsumedByPlayer:
                Tooltip.Instance.ShowText("AWESOME! <color=#a52a2aff>ENEGY</color> GIVES YOU <color=#a52a2aff>POINTS</color>");
                break;
            case HintEvent.EnergyConsumedByBacteria:
                Tooltip.Instance.ShowText("ENERGY CAUSES BACTERIA TO <color=#a52a2aff>GROW</color>");
                break;
            case HintEvent.PlayerChangesColor:
                Tooltip.Instance.ShowText("AWESOME! YOU FOUND A WAY TO BEAT THEIR <color=#a52a2aff>RESISTANCE</color>!");
                break;
            default:
                break;
        }
        StartCoroutine(SlowTime());
        PlayerPrefs.SetInt(hint.ToString(), 0);

    }

    private IEnumerator TutorialRoutine()
    {
        
        while (_player == null) { yield return new WaitForSeconds(1.0f); }

        yield return StartCoroutine(ShowMoveText("USE <color=#a52a2aff>W</color> / <color=#a52a2aff>RMB</color> TO MOVE FORWARD", 1.0f));
        yield return StartCoroutine(ShowMoveText("THE PROBE WILL FOLLOW THE <color=#a52a2aff>MOUSE</color> FOR DIRECTION", 1.0f));

        yield return StartCoroutine(ShowMoveText("USE <color=#a52a2aff>LEFT</color> <color=#a52a2aff>SHIFT</color> TO GET A SPEED BOOST", 1.0f));
       
        Tooltip.Instance.ShowText("PRESS <color=#a52a2aff>LMB</color> / <color=#a52a2aff>SPACE</color> TO FIRE", 0);
        yield return StartCoroutine(WaitForShipToFire());
        Tooltip.Instance.Hide();

        Tooltip.Instance.ShowText("<color=#a52a2aff>BACTERIA</color>!\n QUICK, <color=#a52a2aff>KILL</color> THEM QUICKLY BEFORE THEY MULTIPLY");
        CompleteTutorial();
    }
    private IEnumerator SlowTime()
    {
        float vel = 0;        
        while (Time.timeScale > 0.41f)
        {
            Time.timeScale = Mathf.SmoothDamp(Time.timeScale, 0.2f, ref vel, 0.1f);
            yield return null;
        }
        Time.timeScale = 0.4f;
        float timer = 1.0f;
        while (timer > 0) { timer -= Time.unscaledDeltaTime; yield return null; }

        while (Time.timeScale < 0.9f)
        {
            Time.timeScale = Mathf.SmoothDamp(Time.timeScale, 1.0f, ref vel, 0.1f);
            yield return null;
        }
        Time.timeScale = 1.0f;
    }
    private IEnumerator ShowMoveText(string text,float delay)
    {
        Tooltip.Instance.ShowText(text, 0);
        yield return StartCoroutine(WaitForShipToMove());
        Tooltip.Instance.Hide();
        yield return new WaitForSeconds(delay);
    }

    
    private IEnumerator WaitForShipToMove()
    {
        float moveAmount = 0;
        while (moveAmount < _shipMovementAmount)
        {
            moveAmount += _player.GetComponent<Rigidbody2D>().velocity.magnitude;
            yield return null;
        }

    }

    private IEnumerator WaitForShipToFire()
    {
        int fires = 0;
        while (fires < _shipFireAmount)
        {
            fires += (Input.GetMouseButtonDown(0) | Input.GetKeyDown(KeyCode.Space)) ? 1 : 0;
            yield return null;
        }

    }
    private IEnumerator WaitForKill(Bacteria[] bacteria)
    {
        bool alive = true;
        do
        {
            alive = false;
            foreach (var bac in bacteria)
            {
                if(bac != null) { alive = true;  break; }
            }           
            yield return null;
        } while (alive) ;
    }
    private IEnumerator WaifToKillAllBacteria()
    {

        while (GameController.Instance.Spawn.Enemies.Length > 0)
        {
            yield return new WaitForSeconds(1.0f);
        }
    }
    private Bacteria SpawnBacteria(float growthRate = 0, bool canMutate = false)
    {        
        Bacteria bacteria = _spawn.GetSpawnedBacteria();
        bacteria.GetComponent<BacteriaGrowth>().GrowthRate = growthRate;
        bacteria.GetComponent<BacteriaMutate>().CanMutate = canMutate;
        return bacteria;
    }
    public void CompleteTutorial()
    {
        PlayerPrefs.SetString("Tutorial", "True");
        
        if (TutorialComplete != null)
        {
            TutorialComplete();
        }
    }
}

