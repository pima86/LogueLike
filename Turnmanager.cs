using System.Collections;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst;
    private void Awake() => Inst = this;

    [HideInInspector] public GameObject tuto;
    [HideInInspector] public int turnNumber;

    public enum Turn { Stanby, Main, Battle, End };
    public Turn turn
    {
        set
        {
            if (value == tn) return;
            tn = value;

            switch (value)
            {
                case Turn.Main:
                    StartCoroutine(MainPhase());
                    break;
                case Turn.Battle:
                    StartCoroutine(BattlePhase());
                    break;
                case Turn.End:
                    StartCoroutine(EndPhase());
                    break;
            }
        }
        get
        {
            return tn;
        }
    }
    [Header("턴")]
    [SerializeField] Turn tn;

    [Header("Turn TextMeshPro")]
    [SerializeReference] TextMeshProUGUI playerTurnTmp;
    [SerializeReference] TextMeshProUGUI enemyTurnTmp;


    [Header("Canvas")]
    [SerializeField] CanvasGroup panelGroup;
    [SerializeField] CanvasGroup playerGroup;

    [SerializeField] GameObject mapPanelCanvas;
    [SerializeField] GameObject turnPanelCanvas;
    [SerializeField] GameObject enemyPanelCanvas;
    [SerializeField] GameObject talkCanvas;

    [Header("Animator")]
    [SerializeField] Animator mainAnimator;
    [SerializeField] Animator enemyAnimator;
    [SerializeField] Animator cameraAnimator;

    [Header("Tutorial")]
    [SerializeField] GameObject firstTimeEnter;


    private void Start()
    {
        StartCoroutine(StanbyPhase());
    }

    IEnumerator StanbyPhase()
    {
        panelGroup.CanvasOneActive(0);

        do yield return new WaitForSeconds(0.1f);
        while (mainAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);

        turn = Turn.Main;
    }

    IEnumerator MainPhase()
    {
        turnNumber++;
        playerTurnTmp.text = turnNumber.ToString() + "턴";
        enemyTurnTmp.text = turnNumber.ToString() + "턴";


        //PlayableMenu.Inst.SkillClear(0);
        PlayableObjs.Inst.NextTurn();
        panelGroup.CanvasOneActive(1);
        playerGroup.CanvasOneActive(0);

        yield return new WaitForSeconds(1);

        PlayableObjs.Inst.stayTimer = 0;
    }

    IEnumerator BattlePhase()
    {
        panelGroup.CanvasOneActive(-1);
        playerGroup.CanvasOneActive(-1);

        //SpreadTilemap.Inst.ClearView();
        Camera.main.orthographicSize = 4;
        PlayableObjs.Inst.stayTimer = -1;
        //TileNavigation.Inst.markObject.SetActive(false);
        TileNavigation.Inst.Clear();

        //플레이어 개체들
        GameObject[] units = PlayableObjs.Inst.units;
        PlayablePlan[] infos = new PlayablePlan[units.Length] ;
        for(int i = 0; i < units.Length; i++)
            infos[i] = units[i].GetComponent<PlayablePlan>();

        for (int i = 0; i < infos.Length - 1; i++)
        {
            if (infos[i].skill == null && infos[i + 1].skill != null)
            {
                PlayablePlan temp = infos[i];
                infos[i] = infos[i + 1];
                infos[i + 1] = temp;
                i = -1;
            }
            else if (infos[i].skill != null && infos[i + 1].skill != null)
            {
                if (infos[i].skill.planOrder < infos[i + 1].skill.planOrder)
                {
                    PlayablePlan temp = infos[i];
                    infos[i] = infos[i + 1];
                    infos[i + 1] = temp;
                    i = -1;
                }
            }
        }

        
        for (int i = 0; i < infos.Length; i++)
        {
            if (infos[i].gameObject.activeSelf && infos[i].skill != null)
                yield return StartCoroutine(infos[i].Movement());
        }
        
        turn = Turn.End;
    }

    IEnumerator EndPhase()
    {
        //전투가 시작합니다!
        panelGroup.CanvasOneActive(2);
        //enemyPanelCanvas.SetActive(true);

        do yield return new WaitForSeconds(0.1f);
        while (enemyAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Spawn" ||
               enemyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);

        //몬스터들 행동 계시!
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, Mathf.Infinity, LayerMask.GetMask("Unit"));
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].tag == "Mob")
            {
                ObjectPlan plan = hit[i].GetComponentInParent<ObjectPlan>();
                yield return StartCoroutine(plan.Movement());
            }
        }

        //전투 종료!!
        do yield return new WaitForSeconds(0.1f);
        while (cameraAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);
        turn = Turn.Main;
    }
}
