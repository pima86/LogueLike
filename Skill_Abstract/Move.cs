using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Move_Default : Skill_Frame
{
    public override IEnumerator Action(ObjectPlan plan, string tagName)
    {
        //시야 바깥쪽 몬스터들의 행동은 스킵
        bool cinematic = true;
        if (plan.TryGetComponent(out MonsterInfo monsterInfo))
            cinematic = monsterInfo.eyes.GetCurrentAnimatorClipInfo(0)[0].clip.name == "FadeOut";

        //카메라
        if (cinematic)
        {
            MoveCam.Inst.ImmediateMovement(plan.transform.position);
            plan.personality.Write("Walk");
            yield return new WaitForSeconds(0.5f);
        }

        //이동
        LineRenderer lineRenderer = plan.lineRenderer;
        for (; lineRenderer.positionCount > 0; lineRenderer.positionCount--)
        {
            Vector3 pos = lineRenderer.GetPosition(0);

            //방향
            string arrow = GetArrow(plan, pos);

            //이펙트
            if (cinematic)
            {
                PlayEffectClip(effect[0], plan.transform.position, arrow);

                //이동
                while (Vector3.Distance(plan.transform.position, pos) > 0.01f)
                {
                    plan.transform.position = Vector3.Lerp(plan.transform.position, lineRenderer.GetPosition(0), 0.2f);
                    MoveCam.Inst.ImmediateMovement(plan.transform.position);
                    yield return new WaitForSeconds(0);
                }
            }

            plan.transform.position = pos;

            //라인렌더러 지우기
            for (int j = 0; j < lineRenderer.positionCount - 1; j++)
                lineRenderer.SetPosition(j, lineRenderer.GetPosition(j + 1));
        }

        if (plan.tag == "Player")
        {
            RaycastHit2D hit = Physics2D.Raycast(plan.transform.position, transform.forward, 1, LayerMask.GetMask("Event"));
            if (hit)
            {
                if (hit.collider.tag == "Chest")
                {
                    Chest_Control chest = hit.collider.GetComponentInParent<Chest_Control>();
                    yield return new WaitForSeconds(chest.Open() * 0.5f);
                }
                else if (hit.collider.tag == "Entrance")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (PlayableObjs.Inst.units[i].activeSelf)
                            PlayableObjs.Inst.units[i].GetComponent<PlayablePlan>().characterAnim.Play("DeSpawn");
                    }
                    yield return new WaitForSeconds(2);
                    TransScene.Inst.LoadEventScene();
                }
            }
        }
    }
}
