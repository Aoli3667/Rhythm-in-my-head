﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : Note
{

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

    //現在是到達判定線消失 之後可以把remove Pos降低 把判定線放在
    void Update()
    {
        Move();
    }

    /// <summary>
    /// 有點蠢的寫法 現在是每個音符都掛載此腳本 可能導致性能問題 可能還是生成在父物體上然後移動父物體(象卷軸)會好點? (待實驗)
    /// 另外解法是使用對象池
    /// 
    /// 移動的計算也比較多 可以優化
    /// </summary>
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
            transform.position = Vector2.MoveTowards(transform.position, removePos, jdugeSpeed * Time.deltaTime);
            //得改一下 
            if (Vector2.Distance(transform.position, removePos) < 0.1)
            {
                Destroy(gameObject);
            }
        }
    }
}
