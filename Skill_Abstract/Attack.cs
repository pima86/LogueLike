using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Attack_Default : Skill_Frame
{
    public override string GetTitle(int plus) => title + " " + GetDamage(plus).ToString();

    public override bool GetTarget(Vector2Int point, string tagName)
    {
        RaycastHit2D hit = Physics2D.Raycast(point, transform.forward, 0.9f, LayerMask.GetMask("Unit"));
        return hit && hit.collider.tag == tagName;
    }

    public override IEnumerator Action(ObjectPlan plan, string tagName)
    {
        MoveCam.Inst.ImmediateMovement(plan.transform.position);
        yield return new WaitForSeconds(0.2f);

        Vector3 target = plan.lineRenderer.GetPosition(plan.lineRenderer.positionCount - 1);
        RaycastHit2D hit = Physics2D.Raycast(target, transform.forward, 0.9f, LayerMask.GetMask("Unit"));

        if (!hit) yield break;

        ObjectInfo info = hit.collider.GetComponentInParent<ObjectInfo>();
        if (hit.transform.tag == tagName)
        {
            //카메라 액션
            string arrow = GetArrow(plan, hit.transform.position);
            MoveCam.Inst.ImmediateMovement(hit.transform.position);
            ActionCam.Inst.ActionBattle(arrow);

            //이펙트
            PlayEffectClip(effect[0], hit.transform.position, arrow);

            //몬스터의 체력을 감소
            info.HP -= GetDamage(plan.info.plusDamage);
        }
        yield return new WaitForSeconds(0.2f);

        if (info.HP <= 0)
            plan.personality.Write("Kill");
    }

    public override Vector2Int Patten(ObjectPlan plan, Vector2Int pos, string tagName)
    {
        //가능한 선택지 나열하기
        List<Vector2Int> nodePos = TileNavigation.Inst.SkillRangeTile(this, range, pos);
        List<Vector2Int> selectPos = new List<Vector2Int>();

        for (int i = 0; i < nodePos.Count; i++)
        {
            if (GetTarget(nodePos[i], tagName))
                selectPos.Add(nodePos[i]);
        }

        float distance = Mathf.Infinity;
        int posIndex = 0;
        Collider2D target = NearestTarget(pos, tagName);

        //타겟이 시야에 들어와있는지 없는지
        bool isDiscover = false;
        if (plan.TryGetComponent(out MonsterInfo monsterInfo))
            isDiscover = monsterInfo.eyes.GetCurrentAnimatorClipInfo(0)[0].clip.name == "FadeOut";

        //실행
        if (isDiscover && target != null && selectPos.Count > 0)
        {
            for (int i = 0; i < selectPos.Count; i++)
            {
                float temp = Vector2.Distance(selectPos[i], target.transform.position);
                if (temp < distance)
                {
                    distance = temp;
                    posIndex = i;
                }
            }
            return selectPos[posIndex];
        }
        else
            return new Vector2Int(-1, -1);

    }
}
