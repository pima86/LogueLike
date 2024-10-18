using System;
using System.Collections;
using System.Numerics;
using TMPro;
using UnityEngine;

public abstract class ObjectPlan : MonoBehaviour
{
    [Header("LineRenderer")]
    public LineRenderer lineRenderer;

    [Header("SpriteRenderer")]
    public SpriteRenderer deadSR;
    public SpriteRenderer characterSR;

    [Header("TextMeshPro")]
    [SerializeField] TextMeshPro dialogue;

    [Header("Animator")]
    public Animator stateAnim;
    public Animator characterAnim;

    [Header("Component")]
    [HideInInspector] public ObjectInfo info;
    [HideInInspector] public Personality personality;
    [HideInInspector] public Skill_Frame skill;

    private void Start()
    {
        info = GetComponent<ObjectInfo>();
        info.objectPlan = this;
        info.objectSR = characterSR;
        info.deadSR = deadSR;

        personality = GetComponent<Personality>();
        personality.dialogue = dialogue;
        personality.info = info;
        personality.plan = this;
    }

    public virtual IEnumerator Movement()
    {
        yield break;
    }

    public void ClearSkill()
    {
        skill = null;
    }

    public void StateSetAnimator(string state)
    {
        stateAnim.SetInteger(state, 0);
        stateAnim.Play(state, 0, 0);

        TextPooling.Inst.StateCall(state, transform.position, Color.red);
    }

    public void NextTurnState()
    {
        string stateName = stateAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if(stateName != "Null")
            stateAnim.SetInteger(stateName, stateAnim.GetInteger(stateName) + 1);
    }
}
