using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NoteEditor.DTO;
using Config;
using Monochrome_Notes;

public class MainManager : MonoBehaviour
{
    private float carrentGameTime = 0;
    private float deltaTime;
    private float noteSpeed = 15;
    private string musicName = "Under-the-Moonlight"; //テストで入れてるけどほんとは曲選択画面からもらう
    private bool musicStart = false;

    [SerializeField] private GameObject noteObj;
    [SerializeField] private GameObject judgeLineObj;

    #region Score関係の宣言
    [SerializeField] private Text scoreText;
    private int score;
    [SerializeField] private Text comboText;
    private int combo;
    [SerializeField] private Text pafectText;
    private int parfectNum;
    [SerializeField] private Text greatText;
    private int greatNum;
    [SerializeField] private Text missText;
    private int missNum;
    #endregion

    private GameMaster gameMaster;
    private MusicDTO.EditData editData;
    /// <summary>
    /// Jsonから復元されたMusicDTO.Noteクラス
    /// </summary>
    private MusicDTO.Note note;

    /// <summary>
    /// 全てのノーツの情報を持つリスト
    /// </summary>
    /// <remarks>時間の早いものから順に入っている</remarks>
    private static List<Note.NotePos> NOTES_LIST = new List<Note.NotePos>();

    /// <summary>
    /// ライン毎にノーツの情報を持つDictionary
    /// </summary>
    private static readonly Dictionary<Line, List<Note>> LINE_NOTE_LIST = new Dictionary<Line, List<Note>>();

    /// <summary>
    /// ライン毎にノーツが残っているかを調べるindex
    /// </summary>
    private static readonly Dictionary<Line, int> CURRENT_LINE_NOTE_LIST = new Dictionary<Line, int>();

    private static readonly Dictionary<Line, float> LINE_POSITION = new Dictionary<Line, float>(){
        {Line.Line1, -3.75f},
        {Line.Line2, -1.25f},
        {Line.Line3, 1.25f},
        {Line.Line4, 3.75f}
    };
    private static readonly Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>()
    {
        {Judge.Pafect, 0.1f},
        {Judge.Graet, 0.2f},
        {Judge.Miss, 0.4f}
    };
    private static readonly Dictionary<Judge, int> JUDGE_SCORE = new Dictionary<Judge, int>()
    {
        {Judge.Pafect,1000},
        {Judge.Graet,500},
        {Judge.Miss,0},
     };

    [SerializeField] private readonly List<AudioClip> bgmList;
    [SerializeField] private readonly List<AudioClip> seList;
    private AudioSource audioSource;

    // Use this for initialization
    void Start()
    {
        #region 今は使わないけど後で使うやつ
        //gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        //musicName = gameMaster.GetMusicName();
        //deltaTime = gameMaster.GetTimeDeltaTime();
        //noteSpeed = gameMaster.GetNoteSpeed();
        #endregion

        deltaTime = Time.deltaTime;

        //Jsonから譜面をもらってきてEditDate,Noteクラスとして復元
        string json = Resources.Load<TextAsset>(musicName).ToString();
        editData = JsonUtility.FromJson<MusicDTO.EditData>(json);
        note = JsonUtility.FromJson<MusicDTO.Note>(json);
        
        MusicSelect();
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        //曲が流れ始めるまでに一拍置く処理
        if (carrentGameTime < 5)
        {
            carrentGameTime += deltaTime;
        }
        else if (!musicStart)
        {
            audioSource.Play();
            musicStart = true;
        }
        //ノーツの位置を更新する処理
        foreach (var notes in LINE_NOTE_LIST.Values)
        {
            foreach (var note in notes)
            {
                var pos = note.transform.position;
                pos.y = judgeLineObj.transform.position.y + (note.notePos.notesTimeg - audioSource.time) * noteSpeed;
                note.transform.position = pos;
            }
        }

        //押せなかったnotesをmissにする処理
        foreach (Line _line in System.Enum.GetValues(typeof(Line)))
        {
            int index = CURRENT_LINE_NOTE_LIST[_line];
            if (index < 0)
            {
                //これ以降のループ処理をせずに次の周回に移る
                continue;
            }

            var note = LINE_NOTE_LIST[_line][CURRENT_LINE_NOTE_LIST[_line]];
            float diff = audioSource.time - note.notePos.notesTimeg;
            if (diff > JUDGE_RANGE[Judge.Miss])
            {
                missNum++;
                combo = 0;
                comboText.text = combo.ToString();
                missText.text = missNum.ToString();

                if (CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] + 1 < LINE_NOTE_LIST[_line].Count)
                {
                    CURRENT_LINE_NOTE_LIST[note.notePos.lineNum]++;
                }
                else
                {
                    CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] = -1;
                }
            }
        }

        if (Input.GetButtonDown("Button1"))
        {
            audioSource.PlayOneShot(seList[0]);
            JudgeNotes(Line.Line1);
        }
        if (Input.GetButtonDown("Button2"))
        {
            audioSource.PlayOneShot(seList[0]);
            JudgeNotes(Line.Line2);
        }
        if (Input.GetButtonDown("Button3"))
        {
            audioSource.PlayOneShot(seList[0]);
            JudgeNotes(Line.Line3);
        }
        if (Input.GetButtonDown("Button4"))
        {
            audioSource.PlayOneShot(seList[0]);
            JudgeNotes(Line.Line4);
        }

    }


    /// <summary>
    /// 曲選択から貰った曲の名前をbgmListから探し、audioSourceに格納する
    /// </summary>
    void MusicSelect()
    {
        audioSource = GetComponent<AudioSource>();
        foreach (AudioClip bgm in bgmList)
        {
            if (bgm.name == musicName)
            {
                audioSource.clip = bgm;
            }
        }
    }

    /// <summary>
    /// ノーツを生成する関数
    /// </summary>
    void Initialize()
    {
        score = 0;
        combo = 0;


        //Jsonから受け取ったデータを使いやすいように変換する処理
        foreach (var note in note.notes)
        {
        
        }

        for (int i = 0; i < note.notes.Count; i++)
        {
            //ノーツのある時間 = ノーツの最短間隔[60f / (BPM * LPB)] * エディタ上のノーツの位置
            float time = 60f / (editData.BPM * note.notes[0].LPB) * note.notes[i].num;
            Note.NotePos notePos;
            if (note.notes[i].type == 2) {
                notePos = new Note.NotePos(time, (Line)System.Enum.ToObject(typeof(Line), note.notes[i].block), (NoteType)System.Enum.ToObject(typeof(NoteType), note.notes[i].type));
            } else {
                for (int j = 0; j < note.notes[i].notes.Count; j++)
                {
                    
                }
                notePos = new Note.NotePos(time, (Line)System.Enum.ToObject(typeof(Line), note.notes[i].block), (NoteType)System.Enum.ToObject(typeof(NoteType), note.notes[i].type));
            }
            NOTES_LIST.Add(notePos);
            #region チームメンバーに向けた説明文
            /*  
             *  NOTES_LIST.Add
             *  NOTES_LISTはNote.NotePos型のLIst。Add関数はListの末尾に()の中身を追加する
             *  
             *  (Line)System.Enum.ToObject(typeof(Line), note.notes[i].block)
             *  本来int型であるnote.notes[i].blockをenumのLineに変換する処理
             *  
             *  (NoteType)System.Enum.ToObject(typeof(NoteType), note.notes[i].type))
             *  こちらも同じくnote.notes[i].typeをenumのNoteTypeに変換している
             *  
             *  なので
             *  NOTE_LISTの中にNote.NotePos(どの時間に,どのラインに,どの種類のノーツか)を与えている処理ということになる
             */
            #endregion
        }

        
        foreach (Line line in System.Enum.GetValues(typeof(Line)))
        {
            LINE_NOTE_LIST.Add(line, new List<Note>());
            CURRENT_LINE_NOTE_LIST.Add(line, -1);
            #region チームメンバーに向けた説明文
            /*
             * foreach (Line line in System.Enum.GetValues(typeof(Line)))
             * このforeach文はenum Lineの要素数だけループする。つまり今回だとLine1~Line4
             * 
             * LINE_NOTE_LIST.Add(line, new List<Note>());
             * まずLINE_NOTE_LISTに空のList<Note>を与えLine1~4までを確保している
             * これによってレーン毎のノーツを入れる下準備ができた
             * 
             * CURRENT_LINE_NOTE_LIST.Add(line, -1);
             * 次にCURRENT_LINE_NOTE_LISTに-1を入れているが、これは今のノーツがどこかを調べるためのindexになる
             * -1はノーツがないことを表している
             * この時点ではまだ何のノーツも存在しないので当然-1が入っている
             */
            #endregion
        }

        foreach (var data in NOTES_LIST)
        {
            var obj = Instantiate(noteObj, transform);
            var note = obj.GetComponent<Note>();
            note.Intialize(data);
            var pos = obj.transform.position;
            pos.x = LINE_POSITION[note.notePos.lineNum];
            pos.y = judgeLineObj.transform.position.y + note.notePos.notesTimeg * noteSpeed;
            note.transform.position = pos;
            #region チームメンバーに向けた説明文
            /*
             * var obj = Instantiate(noteObj, transform);
             * Instantiateだけでは生成したオブジェクトにアクセスできないため、Objに代入している
             * 
             * var note = obj.GetComponent<Note>();
             * GetComponent()関数はオブジェクトの持つコンポーネントの情報を取得することができる
             * 今回はノーツのもつNoteの情報を取得している。
             * これによりnoteの持つNoteクラスのpublicな関数や変数を使用することができる
             * 
             * note.Intialize(data); 
             * なのこれでノーツのもつnote.Initializeを実行可能になる
             * 
             * var pos = obj.transform.position;
             * ここからは見た通りノーツの座標を決めている処理になる
             * 
             * pos.x = LINE_POSITION[note.notePos.lineNum];
             * pos.xにはそのノーツがどのLineなのかを見て、対応する数値を代入する
             * 
             * pos.y = judgeLineObj.transform.position.y + note.notePos.notesTimeg * noteSpeed;
             * pos.yには判定ラインからnoteSpeedの速度で動いたときに何秒離れた位置になるかを代入
             * 
             * note.transform.position = pos;
             * xとyの座標が確定したのでノーツのtransform.positionに代入
             * これでノーツが譜面通り配置される
             */
            #endregion

            LINE_NOTE_LIST[note.notePos.lineNum].Add(note);
            if (CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] < 0)
            {
                CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] = 0;
            }
            #region チームメンバーに向けた説明文
            /*
             *  LINE_NOTE_LIST[note.notePos.lineNum].Add(note);
             *  ライン毎にノーツを持つLINE_NOTE_LISTにノーツを追加している
             *  
             *  if (CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] < 0) {
             *  CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] = 0;
             *  }
             *  CURRENT_LINE_NOTE_LISTには最初ノーツがないことを表す-1が入っている
             *  ノーツを生成した為、そのノーツのあるラインに0を与える
             */
            #endregion

            if (note.notePos.noteType == NoteType.Hold)
            {
                var _obj = Instantiate(noteObj, transform);
                var _note = _obj.GetComponent<Note>();
                //_note.Intialize();
            }
        }
    }

    void JudgeNotes(Line _line)
    {
        if (CURRENT_LINE_NOTE_LIST[_line] < 0)
        {
            return;
        }

        var note = LINE_NOTE_LIST[_line][CURRENT_LINE_NOTE_LIST[_line]];
        float diff = Mathf.Abs(audioSource.time - note.notePos.notesTimeg);

        if (diff > JUDGE_RANGE[Judge.Miss])
        {
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
        else
        {
            combo = 0;
            comboText.text = combo.ToString();
        }

        if (CURRENT_LINE_NOTE_LIST[_line] + 1 < LINE_NOTE_LIST[_line].Count)
        {
            CURRENT_LINE_NOTE_LIST[_line]++;
        }
        else
        {
            CURRENT_LINE_NOTE_LIST[_line] = -1;
        }
    }
}