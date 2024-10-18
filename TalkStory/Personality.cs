using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Personality : MonoBehaviour
{
    [HideInInspector] public TextMeshPro dialogue;
    [HideInInspector] public ObjectPlan plan;
    [HideInInspector] public ObjectInfo info;

    public string characterName;

    [Header("대사")]
    public string[] spawnDialogue;
    public string[] walkDialogue;
    public string[] stayDialogue;
    public string[] killDialogue;
    public string[] CampDialogue;
    
    public virtual IEnumerator Stanby()
    {
        yield break;
    }

    public virtual ItemInfo[] DropItem()
    {
        return null;
    }

    Color colorDialogue;
    public void Write(string str)
    {
        if (plan.stateAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Null")
        {
            return;
        }
        colorDialogue = dialogue.color;

        string[] list = new string[0];
        switch (str)
        {
            case "Spawn":
                list = spawnDialogue;
                break;
            case "Walk":
                list = walkDialogue;
                break;
            case "Stay":
                list = stayDialogue;
                break;
            case "Kill":
                list = killDialogue;
                break;
        }

        int index = Random.Range(0, list.Length);
        string text = list[index];
        StartCoroutine(Talk(text));
    }

    public virtual IEnumerator Talk(string text)
    {
        dialogue.color = colorDialogue + new Color(0,0,0,1);
        while (dialogue == null)
            yield return new WaitForSeconds(1);
        for (int i = 0; i < text.Length; i++)
        {
            dialogue.text += text[i];
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1.5f);

        while (dialogue.color.a > 0)
        {
            dialogue.color += new Color(0, 0, 0, -1) * Time.deltaTime * 4;
            yield return new WaitForSeconds(0);
        }
        dialogue.text = "";
    }
}
