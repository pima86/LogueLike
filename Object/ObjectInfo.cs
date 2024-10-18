using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ItemInfoUI.Equipment;

public abstract class ObjectInfo : MonoBehaviour
{
    [HideInInspector] public Sprite sprite;
    [HideInInspector] public ObjectPlan objectPlan;
     public int playableID;
    [HideInInspector] public SpriteRenderer deadSR;
    [HideInInspector] public SpriteRenderer objectSR;
    //[HideInInspector] 
    public List<BuffInfo> buffs;

    [Header("컴포넌트")]
    [SerializeField] TextMeshPro TextHp;
    [SerializeField] TextMeshPro TextEmoji;

    [Header("기본 능력치")]
    public int view;
    public int maxHp;
    public Color hpColor;
    public int dmg;
    public int defense;

    [Header("가중치")]
    public int plus_movement;
    public int plusHP
    {
        set => plus_hp = value;
        get => maxHp + plus_hp;
    }
    int plus_hp;

    public int plusDamage 
    { 
        set => plus_dmg = value; 
        get => dmg + plus_dmg; 
    }
    int plus_dmg;

    public int plusDefense
    {
        set => plus_defense = value;
        get => defense + plus_defense;
    }
    int plus_defense;

    [Header("기술")]
    public Skill_Frame[] skills;


    void Start()
    {
        View = view;
    }

    public virtual IEnumerator Action_Dead()
    {
        yield break;
    }

    //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    #region View
    public virtual int View
    {
        set
        {
            view = value;
        }
        get
        {
            return view;
        }
    }
    #endregion
    #region HP
    public float HP
    {
        set
        {
            int dmg = (int)(value - hp);

            //피해
            if (dmg < 0)
            {
                dmg += plusDefense;
                if (dmg > 0)
                {
                    dmg = -1;
                }
            }

            //최대체력을 넘지 않도록
            if (plusHP < value) value = plusHP;

            //데미지 텍스트 출력
            if (hp != 0)
                TextPooling.Inst.DamageCall(dmg, transform.position);

            //공격을 받음
            if (Shield != 0 && dmg < 0)
            {
                if (Shield >= dmg * -1)
                {
                    Shield += dmg;
                    dmg = 0;
                }
                else
                {
                    dmg += Shield;
                    Shield = 0;
                }
            }

            //혈액
            if (dmg < 0)
                SpreadTilemap.Inst.SpreadBloodTilemap(new Vector2Int(
                                                      Mathf.RoundToInt(transform.position.x),
                                                      Mathf.RoundToInt(transform.position.y)));

            if (hp != value)
            {
                //체력바 갱신
                hp += dmg;

                HpSetting();

                //사망
                if (hp <= 0)
                    StartCoroutine(Action_Dead());
            }
        }
        get
        {
            return hp;
        }
    }public float hp;
    #endregion
    #region Shield
    public int Shield
    {
        set
        {
            //데미지 텍스트 출력
            if (shield != value && hp != 0)
                TextPooling.Inst.DamageCall(value - shield, transform.position);

            if (value > 0)
            {
                BuffPooling.inst.AddBuff(ref buffs, "보호막");

                shield = value;
            }
            else
            {
                int index = buffs.FindIndex(x => x.buffName == "보호막");
                if (index != -1)
                    buffs.RemoveAt(index);

                shield = 0;
            }

            HpSetting();
        }
        get
        {
            return shield;
        }
    }
    [HideInInspector] public int shield;
    #endregion

    void HpSetting()
    {
        if (Shield == 0)
        {
            TextEmoji.text = "<sprite=0>";
            TextHp.color = new Color(255 / 255f, 50 / 255f, 50 / 255f);
            TextHp.text = hp.ToString() + "/" + plusHP.ToString();
        }
        else
        {
            TextEmoji.text = "<sprite=1>";
            TextHp.color = new Color(75/255f, 100/255f, 255/255f);
            TextHp.text = Shield.ToString();
        }
    }
}
