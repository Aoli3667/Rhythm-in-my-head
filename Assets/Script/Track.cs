using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    [SerializeField] Transform SpawnPos;

    private List<NoteInfo> m_Notes = new();
    private List<Note> existed_notes = new();
    private int currentNoteIndex;
    private int nextNoteIndex;
    private bool holdingNote;

    private int _beatsShownInAdvance;
    private Transform _judgePos;
    private Transform _removePos;
    private Transform _playerHoldPos;

    [SerializeField] GameObject tapPrefab;
    [SerializeField] GameObject holdPrefab;

    Transform parent;


    public void Initialize(int beatsShowInAdvance, Transform judgePos, Transform removePos, Transform playerHoldPos)
    {
        _beatsShownInAdvance = beatsShowInAdvance;
        _judgePos = judgePos;
        _removePos = removePos;
        _playerHoldPos = playerHoldPos;

        parent = this.transform;
        m_Notes = new List<NoteInfo>
        {
            new NoteInfo(3f),
            new NoteInfo(4f),
            new NoteInfo(5f, 8f),
            new NoteInfo(9f),
            new NoteInfo(10f, 12f)
        };
    }

    public void TrackUpdate()
    {
        SpawnNote();
        UpdateCurrentNote();
    }

    //maybe need to update later than player input
    private void UpdateCurrentNote()
    {
        //exceed judge time
        if (currentNoteIndex < m_Notes.Count)
        {
            //hold note
            if(m_Notes[currentNoteIndex].holdEndBeat != -1)
            {
                if (LevelManager.Instance.songPosition - ConvertBeatToSec(m_Notes[currentNoteIndex].holdEndBeat) >= 0)
                {
                    existed_notes.RemoveAt(0);
                    clickOnHead = false;
                    currentNoteIndex++;
                }
            }
            //tap note
            else if (LevelManager.Instance.songPosition - ConvertBeatToSec(m_Notes[currentNoteIndex].targetBeat) >= 0.2f)
            {
                existed_notes.RemoveAt(0);
                currentNoteIndex++;
            }
        }
    }

    public bool clickOnHead;
    public void Judge()
    {
        float hitTime;
        if (currentNoteIndex < m_Notes.Count && parent.childCount > 0)
        {
            hitTime = Mathf.Abs(LevelManager.Instance.songPosition - ConvertBeatToSec(m_Notes[currentNoteIndex].targetBeat));
            if (hitTime < 0.2f)
            {
                //tap note judge
                if(m_Notes[currentNoteIndex].holdEndBeat == -1)
                {
                    Destroy(existed_notes[0].gameObject);
                    existed_notes.RemoveAt(0);
                    currentNoteIndex++;
                }
                //hold note judge
                else
                {
                    if(existed_notes[0] is HoldNote hold)
                    {
                        hold.HeadClick();
                        clickOnHead = true;
                        Debug.Log("head");
                    }
                }

                if (hitTime <= 0.03f)
                {
                    Debug.Log("Perfect: " + hitTime);
                }
                else
                {
                    Debug.Log("Not Perfect: " + hitTime);
                }
            }
        }
    }

    public void ReleaseHold()
    {
        existed_notes[0].GetComponent<HoldNote>().GetBack();
        clickOnHead = false;
    }

    private float ConvertBeatToSec(float beat)
    {
        return beat * LevelManager.Instance.secPerBeat;
    }

    private void SpawnNote()
    {
        Note note;
        if (holdingNote)
        {
            if (m_Notes[nextNoteIndex].holdEndBeat <= LevelManager.Instance.songPositionInBeats + _beatsShownInAdvance)
            {
                GameObject holdNote = null;
                foreach (Note test in existed_notes)
                {
                    if (test is HoldNote)
                    {
                        holdNote = test.gameObject;
                        break;
                    }
                }
                if (holdNote)
                {
                    holdNote.GetComponent<HoldNote>().ShowHoldTail();
                    holdingNote = false;
                    nextNoteIndex++;
                }
            }
        }
        else if (nextNoteIndex < m_Notes.Count && m_Notes[nextNoteIndex].targetBeat <= LevelManager.Instance.songPositionInBeats + _beatsShownInAdvance)
        {
            //tap beat
            if (m_Notes[nextNoteIndex].holdEndBeat == -1)
            {
                note = Instantiate(tapPrefab, parent).GetComponent<TapNote>();

                //initialize the fields of the music note
                float targetBeat = m_Notes[nextNoteIndex].targetBeat;
                note.Initialize(parent, _judgePos, _removePos, LevelManager.Instance.songPositionInBeats, targetBeat, LevelManager.Instance.secPerBeat);
                existed_notes.Add(note);
                nextNoteIndex++;
            }
            //hold beat
            else if (m_Notes[nextNoteIndex].holdEndBeat != -1)
            {
                note = Instantiate(holdPrefab, parent).GetComponent<HoldNote>();
                float targetBeat = m_Notes[nextNoteIndex].targetBeat;
                note.Initialize(parent, _judgePos, _removePos, LevelManager.Instance.songPositionInBeats, targetBeat, LevelManager.Instance.secPerBeat);
                note.GetComponent<HoldNote>().HoldInitialize(-1, _playerHoldPos.gameObject);
                existed_notes.Add(note);
                holdingNote = true;
            }
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
