using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNote : Note
{
    [SerializeField] public GameObject holdHead;
    [SerializeField] private GameObject holdTail;
    [SerializeField] private SpriteRenderer holdBody;
    private GameObject judgeObj;
    private float endBeat;

    private bool tailShowed;

    public override void Initialize(Transform spawn, Transform judgePos, Transform removePos, float spawnBeatTime, float targetBeat, float secondsPerBeat, AObjectPool pool)
    {
        if (startTime == -1)
        {
            startTime = Time.time;
        }
        spawnPos = spawn;
        this.judgePos = new Vector2(judgePos.position.x, spawnPos.transform.position.y);
        this.removePos = new Vector2(removePos.position.x, spawnPos.transform.position.y);
        moveDistance = Vector2.Distance((Vector2)this.spawnPos.position, this.judgePos);
        spawnBeat = spawnBeatTime;
        base.targetBeat = targetBeat;
        secPerBeat = secondsPerBeat;
        transform.position = spawnPos.position;
        belonged_pool = pool;
    }

    public void HoldInitialize(float endBeat, GameObject judgeObject)
    {
        tailShowed = false;
        this.endBeat = endBeat;
        judgeObj = judgeObject;
    }

    void Update()
    {
        Move();
        ShowHoldTail();
    }

    public void GetBack()
    {
        holdHead.transform.SetParent(this.gameObject.transform);
    }

    public void HeadClick()
    {
        //holdHead.transform.SetParent(judgeObj.transform);
        holdHead.transform.SetParent(judgeObj.transform);
        holdHead.transform.localPosition = Vector3.zero;
    }

    public void ShowHoldTail()
    {
        if(!tailShowed && endBeat <= LevelManager.Instance.songPositionInBeats + LevelManager.Instance.beatsShownInAdvance)
        {
            holdBody.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            holdTail.SetActive(true);
            holdTail.transform.position = this.spawnPos.position;
            tailShowed = true;
        }
    }

    public override void Move()
    {
        float songPositionInBeats = (float)(AudioSettings.dspTime - LevelManager.dspSongTime) / secPerBeat;
        float t = (songPositionInBeats - spawnBeat) / (targetBeat - spawnBeat);
        t = Mathf.Clamp01(t);

        if (!reachedJudge)
        {
            transform.position = Vector3.Lerp(spawnPos.position, judgePos, t);
            if (t >= 1.0f)
            {
                reachedJudge = true;
                jdugeSpeed = moveDistance / ((targetBeat - spawnBeat) * secPerBeat);
            }
        }
        else
        {
            Vector2 moveDirection = (removePos - (Vector2)judgePos).normalized;  // Calculate direction
            transform.position += (Vector3)(moveDirection * jdugeSpeed * Time.deltaTime);
            if(holdTail.transform.position.x < judgePos.x)
            {
                holdHead.transform.SetParent(this.gameObject.transform);
                holdHead.transform.localPosition = Vector3.zero;
                belonged_pool.ReturnToPool(this.gameObject);
            }
        }
    }
}
