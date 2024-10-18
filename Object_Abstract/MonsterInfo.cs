using System.Collections;
using UnityEngine;

public class MonsterInfo : ObjectInfo
{
    [Header("컴포넌트")]
    public Animator eyes;

    [Header("상태")]
    bool isPanic;

    public void Discover()
    {
        if (!isPanic)
        {
            isPanic = true;
            objectPlan.StateSetAnimator("Panic");
        }
    }

    public override IEnumerator Action_Dead()
    {
        deadSR.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        while (objectSR.color.r > 0.2f)
        {
            objectSR.color = Color.Lerp(objectSR.color, new Color(0, 0, 0, 1), Time.deltaTime * 10f);
            deadSR.color = Color.Lerp(objectSR.color, new Color(0, 0, 0, 1), Time.deltaTime * 10f);
            yield return new WaitForSeconds(0);
        }

        yield return StartCoroutine(ItemManager.Inst.GetItem(objectPlan.personality.DropItem(), transform.position));

        Destroy(gameObject);
    }
}
