using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ItemInfoUI.Equipment;

public class EquipmentObject : MonoBehaviour
{
    public EquipmentInfo info;

    public virtual string[] AbilityContent
    {
        get
        {
            string[] result = new string[4];
            result[0] = "방어력 +2";
            result[1] = "방어력 +4";
            result[2] = "자연회복 +2";
            result[3] = "받는 피해의 50%를 적에게 되돌려줍니다.";
            return result;
        }
    }

    public virtual void AbilitySetUp(PlayableInfo info, int rate)
    {
        info.plusDefense += 2;
        if (rate > 1)
            info.plusDefense += 4;
    }
}

[System.Serializable]
public class EquipmentInfo
{
    public EquipmentObject origin;

    public enum Type { 머리, 갑옷, 무기, 신발, 악세 };
    public Type equipType;

    //스프라이트
    public Sprite equipSpirte() => equipSprites[equipRate];
    public Sprite[] equipSprites;

    public string equipName;
    public int equipRate;

    public State[] states;

    public void RandomState()
    {
        int rate = equipRate + 1;
        states = new State[rate];

        //랜덤수치
        for (int i = 0; i < rate; i++)
        {
            State s = new State();
            s.RandomType();
            states[i] = s;
        }
    }

    public string[] StateContent()
    {
        int amount = states.Length;
        string[] result = new string[amount];
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < result.Length; i++)
        {
            string type = states[i].type.ToString();
            int power = states[i].power;

            sb.Append(type);
            if (power > 0)
                sb.Append("+");
            else
                sb.Append("-");
            sb.Append(power.ToString());

            result[i] = sb.ToString();
            sb.Clear();
        }

        return result;
    }

    [System.Serializable]
    public class State
    {
        public enum Types { 체력, 방어력 };
        public Types type;
        public int rate;
        public int power;

        public void RandomType()
        {
            var enumValues = System.Enum.GetValues(enumType: typeof(Types));
            type = (Types)enumValues.GetValue(Random.Range(0, enumValues.Length));

            int n = Random.Range(0, 100);
            if (n < 50) rate = 0; //50%
            else if (n < 80) rate = 1; //30%
            else if (n < 95) rate = 2; //15%
            else rate = 3; //5%

            RandomAmount(type);
        }

        void RandomAmount(Types type)
        {
            int min = 0;
            int max = 0;

            switch (type)
            {
                case Types.체력:
                    switch (rate)
                    {
                        case 0:
                            min = 1;
                            max = 5;
                            break;
                        case 1:
                            min = 6;
                            max = 10;
                            break;
                        case 2:
                            min = 11;
                            max = 15;
                            break;
                        case 3:
                            min = 16;
                            max = 20;
                            break;
                    }
                    break;
                case Types.방어력:
                    switch (rate)
                    {
                        case 0:
                            min = 1;
                            max = 2;
                            break;
                        case 1:
                            min = 3;
                            max = 5;
                            break;
                        case 2:
                            min = 8;
                            max = 11;
                            break;
                        case 3:
                            min = 12;
                            max = 15;
                            break;
                    }
                    break;
            }

            power = Random.Range(min, max);
        }
    }
}
