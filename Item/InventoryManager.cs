using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Inst;
    private void Awake() => Inst = this;

    [Header("재화")]
    public int gold; //골드
    public int bloodStone; //혈석
    public int silverCoin; //은화
    public int goldCoin; //금화
    

    [Header("아이템")]
    public List<ItemInfo> items;

    public void AddItem(ItemInfo item)
    {
        ItemInfo info = items.Find(x => x.itemName == item.itemName);
        if (info == null)
        {
            //재화 처리
            if (item.type == ItemInfo.Type.재화)
            {
                switch (item.itemName)
                {
                    case "골드":
                        gold += item.amount;
                        break;
                    case "혈석":
                        bloodStone += item.amount;
                        break;
                    case "은화":
                        silverCoin += item.amount;
                        break;
                    case "금화":
                        goldCoin += item.amount;
                        break;
                }
            }
            //신규 아이템 처리
            else
            {
                info = new ItemInfo();
                info.type = item.type;
                info.sprite = item.sprite;
                info.itemName = item.itemName;
                info.amount = item.amount;
                items.Add(info);
                items = items.OrderBy(x => x.itemName).ToList();
            }
        }
        //기존 아이템 추가
        else
        {
            info.amount += item.amount;
        }
    }

    public void RemoveItem(string itemName, int amount = 1)
    {
        ItemInfo info = items.Find(x => x.itemName == itemName);
        if (info != null)
        {
            if (info.amount < amount)
                Debug.Log("경고 // " + itemName + "이라는 아이템이 부족합니다.");
            else
                info.amount -= amount;

            if(info.amount == 0)
                items.Remove(info);
        }
        else
        {
            Debug.Log("경고 // " + itemName + "이라는 아이템을 가지고 있지 않습니다.");
        }
    }
}
