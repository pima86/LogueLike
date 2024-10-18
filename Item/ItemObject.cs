using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public ItemInfo info;
}

[System.Serializable]
public class ItemInfo
{
    public enum Type { 재화, 재료, 요리, 음식 };

    public Type type;
    public Sprite sprite;
    public string itemName;
    public string content;
    public int amount;
}
