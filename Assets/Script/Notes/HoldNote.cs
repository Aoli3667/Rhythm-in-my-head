using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNote : Note
{
    [SerializeField] private GameObject holdHead;
    [SerializeField] private GameObject holdTail;
    [SerializeField] private SpriteRenderer holdBody;
    private GameObject judgeObj;
    private float endBeat;

    private bool initialized;

    public override void Initialize(Transform spawn, Transform judgePos, Transform removePos, float spawnBeatTime, float targetBeat, float secondsPerBeat)
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
    }

    public void HoldInitialize(float endBeat, GameObject judgeObject)
    {
        this.endBeat = endBeat;
        judgeObj = judgeObject;
    }

    void Update()
    {
        Move();
    }

    public void HeadClick()
    {
        holdHead.transform.SetParent(judgeObj.transform);
        holdHead.transform.localPosition = Vector3.zero;
    }

    public void ShowHoldTail()
    {
        holdBody.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        holdTail.SetActive(true);
        holdTail.transform.position = this.spawnPos.position;
        Debug.Log("tail");
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
                Destroy(this.gameObject);
            }
        }
    }
}
