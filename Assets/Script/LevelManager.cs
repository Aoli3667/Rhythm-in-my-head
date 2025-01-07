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

    [SerializeField] Transform JudgePos;
    [SerializeField] Transform RemovePos;

    [Header("Player")]
    [SerializeField] Transform playerLightPos;
    [SerializeField] Transform playerShadowPos;

    [Header("Track")]
    [SerializeField] private Track shadowTrack;
    [SerializeField] private Track lightTrack;

    [Header("Song Info")]
    public AudioSource musicSource;
    [SerializeField] private float bpm;

    [Header("Notes")]
    [SerializeField] public int beatsShownInAdvance; //提前多少拍先生成鍵


    public static LevelManager Instance;

    void Start()
    {
        musicSource = GetComponent<AudioSource>();

        secPerBeat = 60f / bpm;

        dspSongTime = (float)AudioSettings.dspTime;

        musicSource.Play();

        shadowTrack.Initialize(beatsShownInAdvance, JudgePos, RemovePos, playerShadowPos);
        lightTrack.Initialize(beatsShownInAdvance, JudgePos, RemovePos, playerLightPos);
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

    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime); //歌開始後過了幾秒
        songPositionInBeats = songPosition / secPerBeat; //歌開始後過了幾拍
        shadowTrack.TrackUpdate();
        lightTrack.TrackUpdate();
    }
}
