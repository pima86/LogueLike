using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal_Default : Skill_Frame
{
    public override string GetTitle(int plus) => title + " " + GetDamage(plus).ToString();

    public override bool GetTarget(Vector2Int point, string tagName)
    {
        RaycastHit2D hit = Physics2D.Raycast(point, transform.forward, 0.9f, LayerMask.GetMask("Unit"));
        return hit;
    }

    public override IEnumerator Action(ObjectPlan plan, string tagName)
    {
        Vector3 target = plan.lineRenderer.GetPosition(plan.lineRenderer.positionCount - 1);

        RaycastHit2D hit = Physics2D.Raycast(target, transform.forward, 0.9f, LayerMask.GetMask("Unit"));
        if (hit)
        {
            //카메라 액션
            string arrow = GetArrow(plan, hit.transform.position);
            MoveCam.Inst.ImmediateMovement(hit.transform.position);
            ActionCam.Inst.ActionBattle(arrow);

            //이펙트
            PlayEffectClip(effect[0], hit.transform.position, arrow);

            //몬스터의 체력을 감소
            ObjectInfo info = hit.collider.GetComponentInParent<ObjectInfo>();
            info.HP += GetDamage(plan.info.plusDamage);
        }

        yield return new WaitForSeconds(0.2f);
    }
}
