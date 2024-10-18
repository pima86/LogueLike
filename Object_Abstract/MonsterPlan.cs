using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinPlan : ObjectPlan
{
    Skill_Frame MonsterPatten()
    {
        Skill_Frame[] skills = info.skills;

        for (int i = 0; i < skills.Length; i++)
        {
            Vector2Int pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            Vector2Int target = skills[i].Patten(this, pos, "Player");
            if (target.x < 0) continue; //해당 패턴을 사용할 수 없음

            List<Node> FinalNodeList = TileNavigation.Inst.FindingWay(pos, target);

            lineRenderer.positionCount = FinalNodeList.Count;
            for (int f = 0; f < lineRenderer.positionCount; f++)
                lineRenderer.SetPosition(f, new Vector3(FinalNodeList[f].x, FinalNodeList[f].y));

            return skills[i];
        }
        return null;
    }

    public override IEnumerator Movement()
    {
        //몬스터 개성
        yield return StartCoroutine(personality.Stanby());

        string stateName = stateAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        if (stateName != "Null")
        {
            //상태이상 턴
            NextTurnState();

            switch (stateName)
            {
                case "Fear":
                    skill = info.skills[info.skills.Length - 1];
                    Vector2Int pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
                    Vector2Int target = skill.Patten(this, pos, "Null");

                    List<Node> FinalNodeList = TileNavigation.Inst.FindingWay(pos, target);

                    lineRenderer.positionCount = FinalNodeList.Count;
                    for (int f = 0; f < lineRenderer.positionCount; f++)
                        lineRenderer.SetPosition(f, new Vector3(FinalNodeList[f].x, FinalNodeList[f].y));
                    break;
                default:
                    yield break;
            }
        }
        else skill = MonsterPatten();
        if (skill == null)
            yield break;

        //액션
        yield return StartCoroutine(skill.Action(this, "Player"));
        //yield return new WaitForSeconds(0.5f);

        lineRenderer.positionCount = 0;
        ClearSkill();
    }
}
