using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private Transform spawnPos;
    private Vector2 judgePos;
    private Vector2 removePos;
    private float spawnBeat;
    private float removeBeat;
    private float secPerBeat;
    private float beatToReachRemove; // 此note的拍
    private int NoteType;

    private float moveDistance; //判定線跟生成點的距離
    public static float startTime = -1; //生成時間
    private float jdugeTime; //到判定線的時間
    public static float jdugeSpeed = -1;
    private bool reachedJudge = false;

    public void Initialize(Transform spawn, Transform judgePos, Transform removePos, float spawnBeatTime, float targetBeat, float secondsPerBeat, int noteType)
    {
        if (startTime == -1)
        {
            startTime = Time.time;
        }
        spawnPos = spawn;
        this.judgePos = new Vector2(spawnPos.transform.position.x, judgePos.position.y);
        this.removePos = new Vector2 (spawnPos.transform.position.x, removePos.position.y);
        moveDistance = Vector2.Distance((Vector2)this.spawnPos.position, this.judgePos);
        spawnBeat = spawnBeatTime;
        beatToReachRemove = targetBeat;
        secPerBeat = secondsPerBeat;
        NoteType = noteType;
        transform.position = spawnPos.position;
    }

    //現在是到達判定線消失 之後可以把remove Pos降低 把判定線放在
    void Update()
    {
        Move();
    }

    /// <summary>
    /// 有點蠢的寫法 現在是每個音符都掛載此腳本 可能導致性能問題 可能還是生成在父物體上然後移動父物體(象卷軸)會好點? (待實驗)
    /// 解法是使用對象池
    /// 
    /// 移動的計算也比較多 可以優化
    /// </summary>
    private void Move()
    {
        if (!reachedJudge)
        {
            // 歌曲現在的拍
            float songPositionInBeats = (float)(AudioSettings.dspTime - LevelManager.dspSongTime) / secPerBeat;

            //interpolation factor
            float t = (songPositionInBeats - spawnBeat) / (beatToReachRemove - spawnBeat);
            t = Mathf.Clamp01(t);

            // 移動
            transform.position = Vector3.Lerp(spawnPos.position, judgePos, t);
            if (t >= 1.0f)
            {
                reachedJudge = true;
            }
        }
        else
        {
            if (jdugeSpeed == -1)
            {
                jdugeTime = Time.time;
                jdugeSpeed = moveDistance / (jdugeTime - startTime);
            }
            transform.position = Vector2.MoveTowards(transform.position, removePos, jdugeSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, removePos) < 0.1)
            {
                Destroy(gameObject);
                if (NoteType == 0)
                {
                    LevelManager.currentLightIndex++;
                }
                else if (NoteType == 1)
                {
                    LevelManager.currentShadowIndex++;
                }
            }
        }
    }
}
