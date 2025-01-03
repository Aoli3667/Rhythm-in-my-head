using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//看看能不能直接寫一個軌道的script 比這裡存兩個東西好
//一個Note的list來存共有那些note 比從transform child山東西好 - 必要了 hold note直接從裡面讀取來show tail
//可以創一個BeatInfo的struct來存生成時間以及是否為hold建
public class LevelManager : MonoBehaviour
{
    public float songPosition; //歌曲進度(秒)
    public float songPositionInBeats; //歌曲進度(節拍)
    public float secPerBeat; //每拍幾秒
    public static float dspSongTime; //歌過了幾秒

    [SerializeField] Transform LightSpawnPos;
    [SerializeField] Transform ShadowSpawnPos;
    [SerializeField] Transform JudgePos;
    [SerializeField] Transform RemovePos;

    [Header("Player")]
    private int playerMode = 0; //0 = light, 1 = shadow
    [SerializeField] Transform playerLightPos;
    [SerializeField] Transform playerShadowPos;

    [Header("Track")]
    [SerializeField] private Track shadowTrack;

    [Header("Song Info")]
    public AudioSource musicSource;
    [SerializeField] private float bpm;

    [Header("Notes")]
    private List<object> lightNotes;
    private List<object> blackNotes;
    private List<Note> existLightNotes = new();
    private int nextLightIndex = 0;
    private int nextShadowIndex = 0;
    private int currentLightIndex = 0;
    private int currentShadowIndex = 0;
    [SerializeField] Transform shadowNotesParent;
    [SerializeField] Transform lightNotesParent;
    [SerializeField] public int beatsShownInAdvance; //提前多少拍先生成鍵
    [SerializeField] GameObject lightNotePrefab;
    [SerializeField] GameObject blackNotePrefab;

    [Header("Hold")]
    [SerializeField] GameObject lightHoldNotePrefab;
    [SerializeField] GameObject shadowHoldNotePrefab;

    public static LevelManager Instance;

    void Start()
    {
        musicSource = GetComponent<AudioSource>();

        secPerBeat = 60f / bpm;

        dspSongTime = (float)AudioSettings.dspTime;

        musicSource.Play();

        shadowTrack.Initialize(beatsShownInAdvance, JudgePos, RemovePos, playerShadowPos);
        //lightNotes = new float[] { 5, 7, 9 };
        //blackNotes = new float[] { 6, 8, 10 };
        blackNotes = new List<object> { };
        lightNotes = new List<object> { 3f, 4f,  "h5", "h7", 8f, 9f };
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Ensure only one instance exists
        }
    }

    // Update is called once per frame
    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime); //歌開始後過了幾秒
        songPositionInBeats = songPosition / secPerBeat; //歌開始後過了幾拍
        //SpawnNote();
        shadowTrack.TrackUpdate();
        //PlayerChangePos();
        //NoteJudge();
        UpdateCurrentNote(blackNotes, ref currentShadowIndex);
        UpdateCurrentNote(lightNotes, ref currentLightIndex);
    }

    /// <summary>
    /// 按鍵判定
    /// 當按下空格時 首先先判定玩家在哪一列軌道來知道要判定的是光還是暗音符
    /// 然後在判定玩家按下時音樂的節拍是否與當前軌道的音符是否在判定區間
    /// 判定到時 摧毀音符
    /// 
    /// 目前需要修改: 
    /// 因為是由節拍來判定區間和寫譜 所以會導致BPM越高(大概?)
    /// Miss和增加index的方式也很有可能有問題
    /// </summary>
    private void NoteJudge()
    {
        float hitTime = 0;
        if (Input.GetKeyDown(KeyCode.D))
        {
            JudgePos.position = playerShadowPos.position;
            JudgePos.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            if (currentLightIndex < lightNotes.Count && lightNotesParent.childCount > 0)
            {
                hitTime = Mathf.Abs(songPosition - ConvertBeatToSec(lightNotes[currentLightIndex]));
                if (hitTime < 0.2f)
                {
                    Destroy(existLightNotes[0].gameObject);
                    existLightNotes.RemoveAt(0);
                    Debug.Log(currentLightIndex);
                    currentLightIndex++;
                    if (hitTime <= 0.03f)
                    {
                        Debug.Log("Perfect");
                    }
                    else
                    {
                        Debug.Log("Not Perfect");
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            JudgePos.position = playerLightPos.position;
            JudgePos.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            if (currentShadowIndex < blackNotes.Count && shadowNotesParent.childCount > 0)
            {
                hitTime = Mathf.Abs(songPosition - ConvertBeatToSec(blackNotes[currentShadowIndex]));
                if (hitTime < 0.3f)
                {
                    Destroy(shadowNotesParent.GetChild(0).gameObject);
                    currentShadowIndex++;
                    if (hitTime <= 0.03f)
                    {
                        Debug.Log("Perfect");
                    }
                    else
                    {
                        Debug.Log("Not Perfect");
                    }
                }
            }
        }
    }

    //***potential bug: 如果正好消失跟判定同時發生? 感覺會出事
    private void UpdateCurrentNote(List<object> notes, ref int index)
    {
        if (index < notes.Count)
        {
            if (songPosition - ConvertBeatToSec(notes[index]) > 0.1f)
            {
                index++;
            }
        }
    }

    private float ConvertBeatToSec(object beat)
    {
        if (beat is float tapBeatVal)
        {
            return tapBeatVal * secPerBeat;
        }
        else if (beat is string specialBeatVal && specialBeatVal.StartsWith("h"))
        {
            string beatString = specialBeatVal.Substring(1);

            if (float.TryParse(beatString, out float parsedBeat))
            {
                return parsedBeat * secPerBeat;
            }
            else
            {
                Debug.LogWarning($"Failed to parse beat: {specialBeatVal}");
            }
        }
        Debug.LogWarning("Invalid beat format: " + beat);
        return -1;
    }

    private float ConvertToBeat(object beat)
    {
        if (beat is float tapBeatVal)
        {
            return tapBeatVal;
        }
        else if (beat is string specialBeatVal && specialBeatVal.StartsWith("h"))
        {
            string beatString = specialBeatVal.Substring(1);

            if (float.TryParse(beatString, out float parsedBeat))
            {
                return parsedBeat;
            }
            else
            {
                Debug.LogWarning($"Failed to parse beat: {specialBeatVal}");
            }
        }
        Debug.LogWarning("Invalid beat format: " + beat);
        return -1;
    }


    /// <summary>
    /// 切換玩家位置 沒啥好說
    /// </summary>
    private void PlayerChangePos()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (playerMode == 1)
            {
                playerMode = 0;
                JudgePos.position = playerLightPos.position;
                JudgePos.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            }
            else if (playerMode == 0)
            {
                playerMode = 1;
                JudgePos.position = playerShadowPos.position;
                JudgePos.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    private bool lightHolding = false;
    /// <summary>
    /// 生成音符
    /// 個別判定光和暗的音符 如果目前音樂節拍已到達下一個音符的生成時間(生成時間是該音符的判定節拍+自訂的在幾拍之前生成)
    /// 
    /// 開局設置一個倒數和一個"幽靈"note來測試並得到音符的移動速度
    /// </summary>
    private void SpawnNote()
    {
        Note note;
        //For example notes [1,2,3]
        if (nextLightIndex < lightNotes.Count && ConvertToBeat(lightNotes[nextLightIndex]) <= songPositionInBeats + beatsShownInAdvance)
        {
            if (lightNotes[nextLightIndex] is float)
            {
                note = Instantiate(lightNotePrefab, lightNotesParent).GetComponent<TapNote>();

                //initialize the fields of the music note
                float targetBeat = ConvertToBeat(lightNotes[nextLightIndex]);
                note.Initialize(LightSpawnPos, JudgePos, RemovePos, songPositionInBeats, targetBeat, secPerBeat);
                existLightNotes.Add(note);
                nextLightIndex++;
            }
            else if ((lightNotes[nextLightIndex] is string))
            {
                if (!lightHolding)
                {
                    note = Instantiate(lightHoldNotePrefab, lightNotesParent).GetComponent<HoldNote>();
                    float targetBeat = ConvertToBeat(lightNotes[nextLightIndex]);
                    lightHolding = true;
                    note.GetComponent<HoldNote>().Initialize(LightSpawnPos, JudgePos, RemovePos, songPositionInBeats, targetBeat, secPerBeat);
                    note.GetComponent<HoldNote>().HoldInitialize(-1, playerLightPos.gameObject);
                    existLightNotes.Add(note);
                }
                else
                {
                    GameObject holdNote = null;
                    foreach (Note test in existLightNotes)
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
                        lightHolding = false;
                    }
                }
                nextLightIndex++;
            }
        }
        if (nextLightIndex < blackNotes.Count && ConvertToBeat(blackNotes[nextShadowIndex]) <= songPositionInBeats + beatsShownInAdvance)
        {
            note = Instantiate(blackNotePrefab, shadowNotesParent).GetComponent<TapNote>();

            //initialize the fields of the music note
            float targetBeat = ConvertToBeat(blackNotes[nextShadowIndex]);
            note.GetComponent<TapNote>().Initialize(ShadowSpawnPos, JudgePos, RemovePos, songPositionInBeats, targetBeat, secPerBeat);

            nextShadowIndex++;
        }
    }
}
