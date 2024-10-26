using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private float songPosition; //歌曲進度(秒)
    private float songPositionInBeats; //歌曲進度(節拍)
    private float secPerBeat; //每拍幾秒
    public static float dspSongTime; //歌過了幾秒

    [SerializeField] Transform LightSpawnPos;
    [SerializeField] Transform ShadowSpawnPos;
    [SerializeField] Transform JudgePos;
    [SerializeField] Transform RemovePos;

    [Header("Song Info")]
    public AudioSource musicSource;
    [SerializeField] private float bpm;

    [Header("Notes")]
    private float[] lightNotes;
    private float[] blackNotes;
    private int nextLightIndex = 0;
    private int nextShadowIndex = 0;
    public static int currentLightIndex = 0;
    public static int currentShadowIndex = 0;
    [SerializeField] Transform shadowNotesParent;
    [SerializeField] Transform lightNotesParent;
    [SerializeField] private int beatsShownInAdvance; //提前多少拍先生成鍵
    [SerializeField] GameObject lightNotePrefab;
    [SerializeField] GameObject blackNotePrefab;

    void Start()
    {
        musicSource = GetComponent<AudioSource>();

        secPerBeat = 60f / bpm;

        dspSongTime = (float)AudioSettings.dspTime;

        musicSource.Play();

        lightNotes = new float[] { 5, 7, 9 };
        blackNotes = new float[] { 6, 8, 10 };
    }

    // Update is called once per frame
    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime); //歌開始後過了幾秒
        songPositionInBeats = songPosition / secPerBeat; //歌開始後過了幾拍
        SpawnNote();
        PlayerChangePos();
        NoteJudge();
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (JudgePos.position.x == 1)
            {
                if (Mathf.Abs(songPositionInBeats - lightNotes[currentLightIndex]) <= 0.3f)
                {
                    Destroy(lightNotesParent.GetChild(0).gameObject);
                    currentLightIndex++;
                    Debug.Log("Perfect");
                }
            }
            else
            {
                if (Mathf.Abs(songPositionInBeats - blackNotes[currentShadowIndex]) <= 0.3f)
                {
                    Destroy(shadowNotesParent.GetChild(0).gameObject);
                    currentShadowIndex++;
                    Debug.Log("Perfect");
                }
            }
        }
    }

    /// <summary>
    /// 切換玩家位置 沒啥好說
    /// </summary>
    private void PlayerChangePos()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (JudgePos.position.x == 1)
            {
                JudgePos.position = new Vector2(-1, JudgePos.position.y);
                JudgePos.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            }
            else
            {
                JudgePos.position = new Vector2(1, JudgePos.position.y);
                JudgePos.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    /// <summary>
    /// 生成音符
    /// 個別判定光和暗的音符 如果目前音樂節拍已到達下一個音符的生成時間(生成時間是該音符的判定節拍+自訂的在幾拍之前生成)
    /// </summary>
    private void SpawnNote()
    {
        //For example notes [1,2,3]
        if (nextLightIndex < lightNotes.Length && lightNotes[nextLightIndex] <= songPositionInBeats + beatsShownInAdvance)
        {
            GameObject note = Instantiate(lightNotePrefab, lightNotesParent);

            //initialize the fields of the music note
            float targetBeat = lightNotes[nextLightIndex];
            note.GetComponent<Note>().Initialize(LightSpawnPos, JudgePos, RemovePos, songPositionInBeats, targetBeat, secPerBeat,0);

            nextLightIndex++;
        }
        if (nextShadowIndex < blackNotes.Length && blackNotes[nextShadowIndex] <= songPositionInBeats + beatsShownInAdvance)
        {
            GameObject note = Instantiate(blackNotePrefab, shadowNotesParent);

            //initialize the fields of the music note
            float targetBeat = blackNotes[nextShadowIndex];
            note.GetComponent<Note>().Initialize(ShadowSpawnPos, JudgePos, RemovePos, songPositionInBeats, targetBeat, secPerBeat,1);

            nextShadowIndex++;
        }
    }
}
