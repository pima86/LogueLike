using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayablePlan : ObjectPlan
{
    [Header("컴포넌트")]
    public SpriteRenderer placeSR;

    public override IEnumerator Movement()
    {
        NextTurnState();

        placeSR.gameObject.SetActive(false);

        //액션
        yield return StartCoroutine(skill.Action(this, "Mob"));

        //종료 후 패널 애니메이션 대기
        yield return new WaitForSeconds(0.1f);

        lineRenderer.positionCount = 0;
        ClearSkill();
    }
}
