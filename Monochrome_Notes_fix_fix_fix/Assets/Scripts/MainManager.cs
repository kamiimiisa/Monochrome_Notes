using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NoteEditor.DTO;
using NoteEditor.Notes;
using Config;
using Monochrome_Notes;

public class MainManager : MonoBehaviour
{
    private float carrentGameTime = 0;

    private float deltaTime;
    private float noteSpeed = 10;

    private int score;
    private int parfectNum;
    private int greatNum;
    private int missNum;
    private int combo;



    private string musicName = "tutorial"; //テストで入れてるけどほんとは曲選択画面からもらう

    private bool musicStart = false;

    [SerializeField] private GameObject noteObj;
    [SerializeField] private GameObject judgeLineObj;

    [SerializeField] private Text scoreText;
    [SerializeField] private Text comboText;
    [SerializeField] private Text pafectText;
    [SerializeField] private Text greatText;
    [SerializeField] private Text missText;


    private GameMaster gameMaster;
    private MusicDTO.EditData editData;
    private MusicDTO.Note note;

    private static List<MyNote.Note> NOTES_LIST = new List<MyNote.Note>();
    private static readonly Dictionary<LineNum, float> LINE_POSITION = new Dictionary<LineNum, float>(){
        {LineNum.Line1, -3.75f},
        {LineNum.Line2, -1.25f},
        {LineNum.Line3, 1.25f},
        {LineNum.Line4, 3.75f}
    };

    private static readonly Dictionary<LineNum, List<MyNote>> NOTE_LINE_LIST = new Dictionary<LineNum, List<MyNote>>();

    private static readonly Dictionary<LineNum, int> CARRENT_NOTE_LINE_LIST = new Dictionary<LineNum, int>();

    private static readonly Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>(){
        {Judge.Pafect, 0.1f},
        {Judge.Graet, 0.2f},
        {Judge.Miss, 0.4f}
    };
    private static Dictionary<Judge, int> JUDGE_SCORE = new Dictionary<Judge, int>()
        {
            {Judge.Pafect,1000},
            {Judge.Graet,500},
            {Judge.Miss,0},
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
            NOTES_LIST.Add(new MyNote.Note(carrentTime, (LineNum)System.Enum.ToObject(typeof(LineNum), note.notes[i].block), (NoteType)System.Enum.ToObject(typeof(NoteType), note.notes[i].type)));
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

        foreach (var notes in NOTE_LINE_LIST.Values)
        {
            foreach (var note in notes)
            {
                var pos = note.transform.position;
                pos.y = judgeLineObj.transform.position.y + (note.notePos.notesTimeg - audioSource.time) * noteSpeed;
                note.transform.position = pos;
            }
        }

        foreach (LineNum _line in System.Enum.GetValues(typeof(LineNum)))
        {
            int index = CARRENT_NOTE_LINE_LIST[_line];
            if (index < 0)
            {
                continue;
            }

            var note = NOTE_LINE_LIST[_line][CARRENT_NOTE_LINE_LIST[_line]];
            float diff = audioSource.time - note.notePos.notesTimeg;
            if (diff > JUDGE_RANGE[Judge.Miss])
            {
                missNum++;
                combo = 0;
                comboText.text = combo.ToString();
                missText.text = missNum.ToString();

                if (CARRENT_NOTE_LINE_LIST[note.notePos.lineNum] + 1 < NOTE_LINE_LIST[_line].Count)
                {
                    CARRENT_NOTE_LINE_LIST[note.notePos.lineNum]++;
                }
                else {
                    CARRENT_NOTE_LINE_LIST[note.notePos.lineNum] = -1;
                }
            }
        } 

        if (Input.GetButton("Button1"))
        {
            JudgeNotes(LineNum.Line1);
        }
        if (Input.GetButton("Button2"))
        {
            JudgeNotes(LineNum.Line2);
        }
        if (Input.GetButton("Button3"))
        {
            JudgeNotes(LineNum.Line3);
        }
        if (Input.GetButton("Button4"))
        {
            JudgeNotes(LineNum.Line4);
        }

    }

    void Initialize()
    {
        score = 0;
        combo = 0;

        foreach (LineNum line in System.Enum.GetValues(typeof(LineNum)))
        {
            NOTE_LINE_LIST.Add(line, new List<MyNote>());
            CARRENT_NOTE_LINE_LIST.Add(line, -1);
        }

        foreach (var data in NOTES_LIST)
        {
            var obj = Instantiate(noteObj, transform);
            var note = obj.GetComponent<MyNote>();
            note.Intialize(data);
            var pos = obj.transform.position;
            pos.x = LINE_POSITION[note.notePos.lineNum];
            pos.y = judgeLineObj.transform.position.y + note.notePos.notesTimeg * noteSpeed;
            note.transform.position = pos;

            NOTE_LINE_LIST[note.notePos.lineNum].Add(note);
            if (CARRENT_NOTE_LINE_LIST[note.notePos.lineNum] < 0)
            {
                CARRENT_NOTE_LINE_LIST[note.notePos.lineNum] = 0;
            }
        }
    }

    void JudgeNotes(LineNum _line)
    {
        if (CARRENT_NOTE_LINE_LIST[_line] < 0) {
            return;        
        }

        var note = NOTE_LINE_LIST[_line][CARRENT_NOTE_LINE_LIST[_line]];
        float diff = Mathf.Abs(audioSource.time - note.notePos.notesTimeg);

        if ( diff > JUDGE_RANGE[Judge.Miss]) {
            return;
        }

        var judge = Judge.Miss;
        if (diff < JUDGE_RANGE[Judge.Pafect])
        {
            judge = Judge.Pafect;
            parfectNum++;
            pafectText.text = parfectNum.ToString();

        }
        else if (diff < JUDGE_RANGE[Judge.Graet])
        {
            judge = Judge.Graet;
            greatNum++;
            greatText.text = greatNum.ToString();
        }
        else if (diff < JUDGE_RANGE[Judge.Miss])
        {
            missNum++;
            missText.text = missNum.ToString();
        }

        score += JUDGE_SCORE[judge];
        scoreText.text = score.ToString("D7");

        if (judge != Judge.Miss)
        {
            combo++;
            comboText.text = combo.ToString();
        }
        else {
            combo = 0;
            comboText.text = combo.ToString();
        }

        if (CARRENT_NOTE_LINE_LIST[_line] + 1 < NOTE_LINE_LIST[_line].Count)
        {
            CARRENT_NOTE_LINE_LIST[_line]++;
        }
        else {
            CARRENT_NOTE_LINE_LIST[_line] = -1;
        }
    }
}