using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NoteEditor.DTO;
using NoteEditor.Notes;
using Config;
using Monochrome_Notes;

public class MainManager : MonoBehaviour
{
    private float carrentGameTime = 0;

    private float deltaTime;
    private float noteSpeed = 1;

    private string musicName = "tutorial"; //テストで入れてるけどほんとは曲選択画面からもらう

    private bool musicStart = false;

    [SerializeField] private GameObject noteObj;
    [SerializeField] private GameObject judgeLineObj;

    private GameMaster gameMaster;
    private MusicDTO.EditData editData;
    private MusicDTO.Note note;

    private static List<MyNote.NotePos> NOTES_LIST = new List<MyNote.NotePos>();
    private static readonly Dictionary<LineNum, float> LINE_POSITION = new Dictionary<LineNum, float>(){
        {LineNum.Line1, -3.75f},
        {LineNum.Line2, -1.25f},
        {LineNum.Line3, 1.25f},
        {LineNum.Line4, 3.75f}
    };

    [SerializeField] private List<AudioClip> bgmList;
    [SerializeField] private List<AudioClip> seList;
    private AudioSource audioSource;
    private int musicNum = 0;

    float carrentTimeTest = 0f;

    // Use this for initialization
    void Start()
    {
        //gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        //musicName = gameMaster.GetMusicName();
        //deltaTime = gameMaster.GetTimeDeltaTime();
        //noteSpeed = gameMaster.GetNoteSpeed();

        deltaTime = Time.deltaTime;

        audioSource = GetComponent<AudioSource>();
        for (int i = 0; i < bgmList.Count; i++)
        {
            if (bgmList[i].name == musicName)
            {
                audioSource.clip = bgmList[i];
            }
        }

        string json = Resources.Load<TextAsset>(musicName).ToString();
        editData = JsonUtility.FromJson<MusicDTO.EditData>(json);
        note = JsonUtility.FromJson<MusicDTO.Note>(json);

        for (int i = 0; i < note.notes.Count; i++)
        {
            //carrentTime = ノーツの最短間隔[60f / (BPM * LPB)] * ノーツの位置
            float carrentTime = 60f / (editData.BPM * note.notes[0].LPB) * note.notes[i].num;
            NOTES_LIST.Add(new MyNote.NotePos(carrentTime, (LineNum)Enum.ToObject(typeof(LineNum), note.notes[i].block)));
            //Debug.Log(NOTES_LIST[i].notesTimeg.ToString() + NOTES_LIST[i].lineNum);
        }

        Initialize();
    }

    // Update is called once per frame
    void Update()
    {

        if (carrentGameTime < 5)
        {
            carrentGameTime += deltaTime;
        }
        else if (!musicStart)
        {
            audioSource.Play();
            musicStart = true;
        }

    }

    void Initialize()
    {
        foreach (var data in NOTES_LIST)
        {
            var obj = Instantiate(noteObj, transform);
            var note = obj.GetComponent<MyNote>();
            note.Intialize(data);
            var pos = obj.transform.position;
            pos.x = LINE_POSITION[note.notePos.lineNum];
            pos.y = judgeLineObj.transform.position.y + note.notePos.notesTimeg * noteSpeed;
            note.transform.position = pos;
        }

    }
}