using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayableInfo : ObjectInfo
{
    [Header("컴포넌트")]
    [SerializeField] Light2D light2D;

    [Header("장비")]
    public EquipmentInfo[] useEquips;
    [SerializeField] EquipmentObject[] testEquips;

    private void OnEnable()
    {
        useEquips = new EquipmentInfo[5];

        for (int i = 0; i < testEquips.Length; i++)
            EquipInstall(testEquips[i]);
    }

    void EquipInstall(EquipmentObject equip)
    {
        int equipIndex = -1;
        switch (equip.info.equipType)
        {
            case EquipmentInfo.Type.머리:
                equipIndex = 0; break;
            case EquipmentInfo.Type.갑옷:
                equipIndex = 1; break;
            case EquipmentInfo.Type.무기:
                equipIndex = 2; break;
            case EquipmentInfo.Type.신발:
                equipIndex = 3; break;
            case EquipmentInfo.Type.악세:
                equipIndex = 4; break;
        }

        if (equipIndex != -1)
        {
            useEquips[equipIndex] = new EquipmentInfo();

            useEquips[equipIndex].origin = equip;
            useEquips[equipIndex].equipType = equip.info.equipType;
            useEquips[equipIndex].equipName = equip.info.equipName;
            useEquips[equipIndex].equipSprites = equip.info.equipSprites;
            useEquips[equipIndex].equipRate = equip.info.equipRate;
            useEquips[equipIndex].RandomState();
        }
        else
            Debug.Log(equip.info.equipName + "이라는 장비가 장착에 실패했습니다.");

        EquipStateEffect();
        equip.AbilitySetUp(this, equip.info.equipRate);
    }

    void EquipStateEffect()
    {
        plusHP = 0;
        plusDefense = 0;
        plusDamage = 0;


        for (int i = 0; i < useEquips.Length; i++)
        {
            int length = 0;

            if (useEquips[i] != null)
                length = useEquips[i].states.Length;

            for (int j = 0; j < length; j++) 
            {
                string type = useEquips[i].states[j].type.ToString();
                int power = useEquips[i].states[j].power;
                switch (type)
                {
                    case "체력":
                        plusHP += power;
                        break;
                    case "방어력":
                        plusDefense += power;
                        break;
                    case "공격력":
                        plusDamage += power;
                        break;
                }
            }
        }
    }

    public override int View
    {
        set
        {
            view = value;
            light2D.pointLightInnerRadius = value;
            light2D.pointLightOuterRadius = value + 4;
        }
        get
        {
            return view;
        }
    }

    public override IEnumerator Action_Dead()
    {
        deadSR.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        while (objectSR.color.r > 0.1f)
        {
            objectSR.color = Color.Lerp(objectSR.color, new Color(0, 0, 0, 1), Time.deltaTime * 10f);
            deadSR.color = Color.Lerp(objectSR.color, new Color(0, 0, 0, 1), Time.deltaTime * 10f);
            yield return new WaitForSeconds(0);
        }

        gameObject.SetActive(false);
    }
}
