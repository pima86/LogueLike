using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoUI : MonoBehaviour
{
    public Equipment equipmentUI;

    [System.Serializable]
    public class Equipment
    {
        [SerializeField] Sprite[] stateRate;
        [SerializeField] GameObject gameObject;
        [SerializeField] TextMeshProUGUI itemName;
        [SerializeField] TextMeshProUGUI itemPart;
        [SerializeField] Info itemState;
        [SerializeField] Info itemAbility;

        public void SetActive(bool bo, EquipmentInfo info = null)
        {
            gameObject.SetActive(bo);

            if (bo)
            {
                itemName.text = info.equipName;
                itemPart.text = info.equipType.ToString();
                itemState.StateSetUp(info, stateRate);
                itemAbility.AbilitySetUp(info);
            }
        }

        [System.Serializable]
        public class Info
        {
            public State[] state;

            public void StateSetUp(EquipmentInfo info, Sprite[] spriteRate)
            {
                string[] content = info.StateContent();

                for (int i = 0; i < state.Length; i++)
                {
                    if (content.Length > i)
                    {
                        state[i].stateName.gameObject.SetActive(true);

                        state[i].stateIcon.sprite = spriteRate[info.states[i].rate];
                        state[i].stateName.text = content[i];
                    }
                    else
                        state[i].stateName.gameObject.SetActive(false);
                }
            }

            public void AbilitySetUp(EquipmentInfo info)
            {
                string[] content = info.origin.AbilityContent;

                for (int i = 0; i < state.Length; i++)
                {
                    state[i].stateName.text = content[i];

                    Color color;
                    if (info.equipRate >= i) 
                        color = new Color(1, 1, 1, 1);
                    else 
                        color = new Color(100 / 255f, 100 / 255f, 100 / 255f, 1);

                    state[i].stateIcon.color = color;
                    state[i].stateName.color = color;
                }
            }

            [System.Serializable]
            public class State
            {
                public TextMeshProUGUI stateName;
                public Image stateIcon;
            }
        }
    }
}
