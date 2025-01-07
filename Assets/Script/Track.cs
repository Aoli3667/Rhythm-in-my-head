using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Track : MonoBehaviour
{
    private List<NoteInfo> m_Notes = new();
    private List<Note> existed_notes = new();
    private AObjectPool tap_Pool;
    private AObjectPool hold_Pool;
    private int currentNoteIndex;
    private int nextNoteIndex;
    public bool clickOnHead;

    [SerializeField] private int _beatsShownInAdvance;
    private Transform _judgePos;
    private Transform _removePos;
    private Transform _playerHoldPos;

    [SerializeField] GameObject tapPrefab;
    [SerializeField] GameObject holdPrefab;

    Transform notes_parent;

    TMP_Text judge_display;
    Coroutine judge_coroutine;


    public void Initialize(int beatsShowInAdvance, Transform judgePos, Transform removePos, Transform playerHoldPos)
    {
        _beatsShownInAdvance = beatsShowInAdvance;
        _judgePos = judgePos;
        _removePos = removePos;
        _playerHoldPos = playerHoldPos;

        judge_display = gameObject.transform.Find("Judge").Find("Text").GetComponent<TMP_Text>();

        notes_parent = this.transform.Find("Notes");
        m_Notes = new List<NoteInfo>
        {
            new NoteInfo(3f),
            new NoteInfo(4f),
            new NoteInfo(5f, 8f),
            new NoteInfo(9f),
            new NoteInfo(12f, 13f)
        };

        InitPool();
    }

    /// <summary>
    /// 初始化不同種類案件的對象池
    /// </summary>
    private void InitPool()
    {
        tap_Pool = new AObjectPool(tapPrefab, notes_parent);
        hold_Pool = new AObjectPool(holdPrefab, notes_parent);
    }

    public void TrackUpdate()
    {
        SpawnNote();
        UpdateCurrentNote();
    }

    bool judge_shown;

    //maybe need to update later than player input
    //***potential bug: 如果正好消失跟判定同時發生? 感覺會出事
    /// <summary>
    /// 更新目前要判定的按鍵索引
    /// </summary>
    private void UpdateCurrentNote()
    {
        //exceed judge time
        if (currentNoteIndex < m_Notes.Count)
        {
            if (LevelManager.Instance.songPosition - ConvertBeatToSec(m_Notes[currentNoteIndex].targetBeat) >= 0.2f)
            {
                if (m_Notes[currentNoteIndex].holdEndBeat == -1)
                {
                    existed_notes.RemoveAt(0);
                    currentNoteIndex++;
                    StartJudgeCoroutine("Miss...");
                }
                else if(!judge_shown && !clickOnHead)
                {
                    StartJudgeCoroutine("Miss...");
                }
                judge_shown = true;
            }
            //hold note
            if (m_Notes[currentNoteIndex].holdEndBeat != -1)// && clickOnHead)
            {
                if (LevelManager.Instance.songPosition - ConvertBeatToSec(m_Notes[currentNoteIndex].holdEndBeat) >= 0)
                {
                    if (!clickOnHead)
                    {
                        StartJudgeCoroutine("Miss...");
                    }
                    else
                    {
                        StartJudgeCoroutine("Perfect!");
                    }
                    existed_notes.RemoveAt(0);
                    clickOnHead = false;
                    currentNoteIndex++;
                    judge_shown = false;
                }
            }
        }
    }

    /// <summary>
    /// 按鍵判定
    /// </summary>
    public void Judge()
    {
        float hitTime;
        if (currentNoteIndex < m_Notes.Count && notes_parent.childCount > 0)
        {
            hitTime = Mathf.Abs(LevelManager.Instance.songPosition - ConvertBeatToSec(m_Notes[currentNoteIndex].targetBeat));
            if (hitTime < 0.2f)
            {
                //tap note judge
                if (m_Notes[currentNoteIndex].holdEndBeat == -1)
                {
                    //tap_Pool.ReturnToPool(existed_notes[0].gameObject);
                    existed_notes[0].belonged_pool.ReturnToPool(existed_notes[0].gameObject);
                    existed_notes.RemoveAt(0);
                    currentNoteIndex++;
                }
                //hold note judge
                else
                {
                    if (existed_notes[0] is HoldNote hold)
                    {
                        hold.HeadClick();
                        clickOnHead = true;
                        //Debug.Log("head");
                    }
                }

                if (hitTime <= 0.03f)
                {
                    StartJudgeCoroutine("Perfect!");
                    //Debug.Log("Perfect: " + hitTime);
                }
                else if (hitTime <= 0.05f)
                {
                    StartJudgeCoroutine("Great!");
                    //Debug.Log("Great: " + hitTime);
                }
            }
        }
    }

    private void StartJudgeCoroutine(string display)
    {
        Debug.Log(display);
        if (judge_coroutine != null)
        {
            StopCoroutine(judge_coroutine);
        }
        judge_coroutine = StartCoroutine(JudgeDisplay(display));
    }

    private IEnumerator JudgeDisplay(string display)
    {
        judge_display.text = display;
        yield return new WaitForSeconds(1);
        judge_display.text = "";
        judge_coroutine = null;
    }

    /// <summary>
    /// 長按鬆手
    /// </summary>
    public void ReleaseHold()
    {
        //release early but in perfect range
        if (ConvertBeatToSec(m_Notes[currentNoteIndex].holdEndBeat) - LevelManager.Instance.songPosition <= 0.2f)
        {
            existed_notes.RemoveAt(0);
            currentNoteIndex++;
            StartJudgeCoroutine("Perfect!");
        }
        //release too early
        else
        {
            existed_notes[0].GetComponent<HoldNote>().GetBack();
        }
        clickOnHead = false;
    }

    private float ConvertBeatToSec(float beat)
    {
        return beat * LevelManager.Instance.secPerBeat;
    }

    /// <summary>
    /// 按鍵生成
    /// </summary>
    private void SpawnNote()
    {
        if (nextNoteIndex < m_Notes.Count && m_Notes[nextNoteIndex].targetBeat <= LevelManager.Instance.songPositionInBeats + _beatsShownInAdvance)
        {
            NoteSpawn();
        }
    }

    /// <summary>
    /// 按鍵生成
    /// </summary>
    private void NoteSpawn()
    {
        GameObject note;
        //tap beat
        if (m_Notes[nextNoteIndex].holdEndBeat == -1)
        {
            note = tap_Pool.Get();
            TapNote tapNote = note.GetComponent<TapNote>();
            //initialize the fields of the music note
            float targetBeat = m_Notes[nextNoteIndex].targetBeat;
            tapNote.Initialize(notes_parent, _judgePos, _removePos, LevelManager.Instance.songPositionInBeats, targetBeat, LevelManager.Instance.secPerBeat, tap_Pool);
            existed_notes.Add(tapNote);
            nextNoteIndex++;
        }
        //hold beat
        else if (m_Notes[nextNoteIndex].holdEndBeat != -1)
        {
            note = hold_Pool.Get();
            HoldNote holdNote = note.GetComponent<HoldNote>();
            float targetBeat = m_Notes[nextNoteIndex].targetBeat;
            holdNote.Initialize(notes_parent, _judgePos, _removePos, LevelManager.Instance.songPositionInBeats, targetBeat, LevelManager.Instance.secPerBeat, hold_Pool);
            holdNote.HoldInitialize(m_Notes[nextNoteIndex].holdEndBeat, _playerHoldPos.gameObject);
            existed_notes.Add(holdNote);
            //holdingNote = true;
            nextNoteIndex++;
        }
    }

    private struct NoteInfo
    {
        public float targetBeat;
        public float holdEndBeat;

        public NoteInfo(float targetBeat, float holdEndBeat = -1)
        {
            this.targetBeat = targetBeat;
            this.holdEndBeat = holdEndBeat;
        }
    }
}
