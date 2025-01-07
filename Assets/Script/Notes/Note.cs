using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public abstract class Note : MonoBehaviour, INote
{
    protected Transform spawnPos;
    protected Vector2 judgePos;  //應該可以從levelManager 調取
    protected Vector2 removePos;  //應該可以從levelManager 調取
    protected float spawnBeat; 
    protected float secPerBeat;  //應該可以從levelManager 調取
    protected float targetBeat; // 此note的拍
    public AObjectPool belonged_pool;

    protected float moveDistance; //判定線跟生成點的距離
    public static float startTime = -1; //生成時間
    public static float jdugeSpeed = -1;
    protected bool reachedJudge = false;

    public abstract void Move();
    public abstract void Initialize(Transform spawn, Transform judgePos, Transform removePos, float spawnBeatTime, float targetBeat, float secondsPerBeat, AObjectPool beloned_pool);
}
