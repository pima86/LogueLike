using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Frame : MonoBehaviour
{
    //이동을 디폴트 값으로 갖고 있음
    //필요에 따라 override로 수정할 것

    public enum SkillType { 위치, 공격, 버프, 대기 };
    public SkillType skillType;
    public int planOrder;

    //스킬명
    public virtual string GetTitle(int plus) => title;
    public string title;

    //피해량
    public virtual int GetDamage(int plus) => damage + plus;
    public int damage;

    //사거리
    public virtual int GetRange(int n) => range + n;
    public int range;

    //타겟
    public virtual bool GetTarget(Vector2Int point, string tagName)
    {
        bool bo = false;
        RaycastHit2D hit = Physics2D.Raycast(point, transform.forward, 0.9f, LayerMask.GetMask("Unit"));

        if (tagName == "Mob")
            bo = !hit && !Physics2D.Raycast(point, transform.forward, 0.9f, LayerMask.GetMask("Line"));
        else
            bo = !hit && !Physics2D.Raycast(point, transform.forward, 0.9f, LayerMask.GetMask("Event"));


        return bo;
    }

    //액션
    [Header("이펙트")]
    public RuntimeAnimatorController[] effect;
    public virtual IEnumerator Action(ObjectPlan plan, string tagName)
    {
        yield break;
    }

    public string GetArrow(ObjectPlan plan, Vector3 pos2)
    {
        SpriteRenderer characterSR = plan.characterSR;
        Vector3 pos1 = plan.transform.position;

        if (pos1.x > pos2.x)
        {
            characterSR.flipX = false;
            return "Left";
        }
        else if (pos1.x < pos2.x)
        {
            characterSR.flipX = true;
            return "Right";
        }
        else if (pos1.y > pos2.y) return "Down";
        else return "Up";
    }

    public void PlayEffectClip(RuntimeAnimatorController smokeFX, Vector2 pos, string str)
    {
        var effect = EffectPooling.Inst.Call();

        effect.transform.position = pos;
        effect.runtimeAnimatorController = smokeFX;
        effect.Play(str);
    }

    //몬스터 AI
    public virtual Vector2Int Patten(ObjectPlan plan, Vector2Int pos, string tagName)
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

        if (isDiscover && target != null)
        {
            List<Vector2Int> tempPos = new List<Vector2Int>();

            for (int i = 0; i < selectPos.Count; i++)
            {
                float temp = Vector2.Distance(selectPos[i], target.transform.position);
                if (temp < distance)
                {
                    tempPos.Clear();
                    tempPos.Add(selectPos[i]);
                    distance = temp;
                }
                else if (temp == distance)
                    tempPos.Add(selectPos[i]);
            }

            distance = Mathf.Infinity;
            for (int i = 0; i < tempPos.Count; i++)
            {
                float temp = Vector2.Distance(pos, tempPos[i]);
                if (temp < distance)
                {
                    distance = temp;
                    posIndex = i;
                }
            }
            return tempPos[posIndex];
        }
        else
        {
            int random = Random.Range(-1, selectPos.Count);

            if (random >= 0)
                return selectPos[random];
            else
                return new Vector2Int(-1, -1);
        }

    }

    //가장 가까운 타겟을 가져옴
    public Collider2D NearestTarget(Vector2Int pos, string tagName)
    {
        float distance = Mathf.Infinity;
        Collider2D target = null;
        Collider2D[] hit = Physics2D.OverlapCircleAll(pos, 10, LayerMask.GetMask("Unit"));

        for (int h = 0; h < hit.Length; h++)
        {
            if (hit[h].tag != tagName) continue;

            float temp = Vector2.Distance(pos, hit[h].transform.position);
            if (temp < distance)
            {
                distance = temp;
                target = hit[h];
            }
        }
        return target;
    }
}
