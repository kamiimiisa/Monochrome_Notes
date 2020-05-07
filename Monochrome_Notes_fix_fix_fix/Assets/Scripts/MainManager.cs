using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NoteEditor.DTO;
using Config;
using Monochrome_Notes;

public class MainManager : MonoBehaviour {
    private float carrentGameTime = 0;
    private float deltaTime;
    [SerializeField][Range(1,20)]private int noteSpeed = 5;
    private readonly float DELAY_TIME = 5f;
    private float offset = 0;
    [SerializeField] private string musicName = "tutorial"; //テストで入れてるけどほんとは曲選択画面→GMからもらう
    private bool musicStart = false;

    private bool[] holdFlg = { false, false, false, false };
    private float[] holdTime = { 0, 0, 0, 0 };
    private float[] musicTime = { 0, 0, 0, 0 };
    private Note[] endHoldNote = new Note[4];

    [SerializeField] private GameObject noteObj;
    [SerializeField] private GameObject holdNoteObj;
    [SerializeField] private GameObject[] judgeLineObj;
    
    [SerializeField] private Text scoreText;
    private int score = 0;
    [SerializeField] private Text comboText;
    private int combo;
    private int maxCombo;
    private int parfectNum;
    private int greatNum;
    private int missNum;
    //[SerializeField] private Text maxComboText;
    //[SerializeField] private Text pafectText;
    //[SerializeField] private Text greatText;
    //[SerializeField] private Text missText;

    [SerializeField] private Image gauge;
    private int maxScore = 0;

    private MusicDTO.EditData editData;
    private MusicDTO.Note musicNote;

    private MyInput Button2 = new MyInput();
    private MyInput Button2_2 = new MyInput();


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

    private static Dictionary<Line, float> LINE_POSITION = new Dictionary<Line, float>();

    private static readonly Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>()
    {
        {Judge.Pafect, 0.05f},
        {Judge.Graet, 0.1f},
        {Judge.Miss, 0.2f}
    };
    private static readonly Dictionary<Judge, int> JUDGE_SCORE = new Dictionary<Judge, int>()
    {
        {Judge.Pafect,1000},
        {Judge.Graet,500},
        {Judge.Miss,0},
     };

    [SerializeField] private List<AudioClip> bgmList;
    [SerializeField] private List<AudioClip> seList;
    private AudioSource MusicSource;
    [SerializeField] private AudioSource SESource;


    // Use this for initialization
    void Start() {
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
        musicNote = JsonUtility.FromJson<MusicDTO.Note>(json);

        //audioSource.clipにmusicNameと同じ名前のものを渡す
        MusicSource = GetComponent<AudioSource>();
        MusicSource.clip = bgmList.Find(bgm => bgm.name == musicName);

        foreach (Line line in System.Enum.GetValues(typeof(Line))) {
            LINE_POSITION.Add(line,judgeLineObj[(int)line].transform.position.x);
        }

        SESource.clip = seList[0];
        Initialize();
    }

    // Update is called once per frame
    void Update() {
        //曲が流れ始めるまでに一拍置く処理
        if (carrentGameTime < DELAY_TIME) {
            carrentGameTime += deltaTime;
        } else if (!musicStart) {
            MusicSource.Play();
            musicStart = true;
        }

        //ノーツの位置を更新する処理
        foreach (var notes in LINE_NOTE_LIST.Values) {
            foreach (var note in notes) {
                var pos = note.transform.position;
                pos.y = judgeLineObj[(int)note.notePos.lineNum].transform.position.y + (note.notePos.notesTimeg - MusicSource.time - carrentGameTime + DELAY_TIME) * noteSpeed;
                note.transform.position = pos;
            }
        }

        JudgeNotesMiss();
        if (MusicSource.time == MusicSource.clip.length) {
            Debug.Log("終わりやよ～");
        }

        if (carrentGameTime > DELAY_TIME - JUDGE_RANGE[Judge.Miss]) {
            if (Input.GetButtonDown("Button1")) {
                SESource.Play();
                JudgeNotes(Line.Line1);
            }
            if (Input.GetButtonDown("Button2") || Button2.GetButtonDown("Button2_2") || Button2_2.GetButtonDown("Button2_3")) {
                SESource.Play();
                JudgeNotes(Line.Line2);
            }
            if (Input.GetButtonDown("Button3")) {
                SESource.Play();
                JudgeNotes(Line.Line3);
            }
            if (Input.GetButtonDown("Button4")) {
                SESource.Play();
                JudgeNotes(Line.Line4);
            }

            if (Input.GetButton("Button1")) {
                JudgeHoldNotes(Line.Line1);
            }
            if (Input.GetButton("Button2") || Button2.GetButton("Button2_2") || Button2_2.GetButton("Button2_3")) {
                JudgeHoldNotes(Line.Line2);
            }
            if (Input.GetButton("Button3")) {
                JudgeHoldNotes(Line.Line3);
            }
            if (Input.GetButton("Button4")) {
                JudgeHoldNotes(Line.Line4);
            }
        }

        //クリアゲージを更新する処理
        gauge.fillAmount = (float)score / maxScore;
    }

    /// <summary>
    /// ノーツを生成する関数
    /// </summary>
    void Initialize() {
        score = 0;
        combo = 0;
        offset = (float)editData.offset / (float)MusicSource.clip.frequency;

        //Jsonから受け取ったデータを使いやすいように変換する処理
        for (int i = 0; i < musicNote.notes.Count; i++) {
            //ノーツのある時間 = ノーツの最短間隔[60f / (BPM * LPB)] * エディタ上のノーツの位置
            float time = 60f / (editData.BPM * musicNote.notes[0].LPB) * musicNote.notes[i].num + offset;
            Note.NotePos notePos;
            if (musicNote.notes[i].type != 2) {
                notePos = new Note.NotePos(time, musicNote.notes[i].block, musicNote.notes[i].type);
            } else {
                List<Note> _noteList = new List<Note>();
                for (int j = 0; j < musicNote.notes[i].notes.Count; j++) {
                    float _time = 60f / (editData.BPM * musicNote.notes[0].LPB) * musicNote.notes[i].notes[j].num + offset;
                    Note _noteTmp = new Note();
                    _noteTmp.Initialize(new Note.NotePos(_time, musicNote.notes[i].notes[j].block, musicNote.notes[i].notes[j].type));
                    _noteList.Add(_noteTmp);
                }
                notePos = new Note.NotePos(time, musicNote.notes[i].block, musicNote.notes[i].type, _noteList);
            }
            NOTES_LIST.Add(notePos);
            #region 
            /*  
             * 
             */
            #endregion
        }


        foreach (Line line in System.Enum.GetValues(typeof(Line))) {
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

        foreach (var data in NOTES_LIST) {
            var obj = Instantiate(noteObj, transform);
            var note = obj.GetComponent<Note>();
            note.Initialize(data);
            var pos = obj.transform.position;
            pos.x = LINE_POSITION[note.notePos.lineNum];
            pos.y = judgeLineObj[(int)data.lineNum].transform.position.y + note.notePos.notesTimeg * noteSpeed;
            note.transform.position = pos;
            #region チームメンバーに向けた説明文
            /*
             * var obj = Instantiate(noteObj, transform);
             * Instantiateだけでは生成したオブジェクトにアクセスできないため、Objに入れている
             * 
             * var note = obj.GetComponent<Note>();
             * GetComponent()関数はオブジェクトの持つコンポーネントの情報を取得することができる
             * 今回はnoteObjのもつNoteの情報を取得している。
             * これによりnoteの持つNoteクラスのpublicな関数を使用することができる
             * 
             * note.Initialize(data); 
             * なのこれでnoteのもつNote.Initializeを実行可能になる
             * 
             * var pos = obj.transform.position;
             * ここからは見た通りノーツの座標を決めている処理になる
             * 
             * pos.x = LINE_POSITION[note.notePos.lineNum] + laneObj.transform.position.x;
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

            float spriteX = 2f;
            if (note.notePos.lineNum == Line.Line1 || note.notePos.lineNum == Line.Line4) {
                spriteX = 0.8f;
            }
            var spritRenderer = obj.GetComponent<SpriteRenderer>();
            var spritSize = spritRenderer.size;
            spritSize.x = spriteX;
            spritRenderer.size = spritSize;

            maxScore += JUDGE_SCORE[Judge.Pafect];
            LINE_NOTE_LIST[note.notePos.lineNum].Add(note);
            if (CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] < 0) {
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

            //ホールドノーツの生成
            if (note.notePos.noteType == NoteType.Hold) {
                foreach (var _data in data.noteList) {
                    var _obj = Instantiate(noteObj, transform);
                    var _note = _obj.GetComponent<Note>();
                    _note.Initialize(_data.notePos);
                    var _pos = _obj.transform.position;
                    _pos.x = LINE_POSITION[_note.notePos.lineNum];
                    _pos.y = judgeLineObj[(int)_data.notePos.lineNum].transform.position.y + _note.notePos.notesTimeg * noteSpeed;
                    _obj.transform.parent = obj.transform;
                    _note.transform.position = _pos;

                    if (_note.notePos.lineNum == Line.Line1 || _note.notePos.lineNum == Line.Line4) {
                        spriteX = 0.8f;
                    }
                    var _spritRenderer = _obj.GetComponent<SpriteRenderer>();
                    var _spritSize = _spritRenderer.size;
                    _spritSize.x = spriteX;
                    _spritRenderer.size = _spritSize;



                    var holdObj = Instantiate(holdNoteObj, transform);
                    var _holdNoteSpritRenderer = holdObj.GetComponent<SpriteRenderer>();
                    var _holdNotesSpritSize = _holdNoteSpritRenderer.size;
                    _holdNotesSpritSize.x = spriteX;
                    _holdNotesSpritSize.y = obj.transform.position.y - _obj.transform.position.y - 0.2f;
                    _holdNoteSpritRenderer.size = _holdNotesSpritSize;
                    holdObj.transform.position = (obj.transform.position + _obj.transform.position) / 2;
                    holdObj.transform.parent = obj.transform;

                    maxScore += JUDGE_SCORE[Judge.Pafect];
                    LINE_NOTE_LIST[_note.notePos.lineNum].Add(_note);
                }
            }
        }
    }

    /// <summary>
    /// タッチノーツの判定をとる関数
    /// </summary>
    /// <param name="_line"></param>
    void JudgeNotes(Line _line) {
        if (CURRENT_LINE_NOTE_LIST[_line] < 0) {
            return;
        }

        var note = LINE_NOTE_LIST[_line][CURRENT_LINE_NOTE_LIST[_line]];
        if (note.notePos.noteType == NoteType.Hold) {
            return;
        }
        
        float diff = Mathf.Abs(MusicSource.time - note.notePos.notesTimeg);
        if (diff > JUDGE_RANGE[Judge.Miss]) {
            return;
        }

        var judge = Judge.Miss;
        if (diff < JUDGE_RANGE[Judge.Pafect]) {
            judge = Judge.Pafect;
            ++parfectNum;
            //pafectText.text = parfectNum.ToString();

        } else if (diff < JUDGE_RANGE[Judge.Graet]) {
            judge = Judge.Graet;
            ++greatNum;
            //greatText.text = greatNum.ToString();
        } else if (diff < JUDGE_RANGE[Judge.Miss]) {
            ++missNum;
            //missText.text = missNum.ToString();
        }

        score += JUDGE_SCORE[judge];
        scoreText.text = score.ToString("D7");

        if (judge != Judge.Miss) {
            ++combo;
            comboText.text = combo.ToString();
            
        } else {
            combo = 0;
            comboText.text = combo.ToString();
        }
        if (maxCombo < combo) {
            maxCombo = combo;
            //maxComboText.text = maxCombo.ToString();
        }
        if (CURRENT_LINE_NOTE_LIST[_line] + 1 < LINE_NOTE_LIST[_line].Count) {
            CURRENT_LINE_NOTE_LIST[_line]++;
        } else {
            CURRENT_LINE_NOTE_LIST[_line] = -1;
        }
    }


    private void JudgeHoldNotes(Line _line) {
        if (CURRENT_LINE_NOTE_LIST[_line] < 0) {
            return;
        }

        var _note = LINE_NOTE_LIST[_line][CURRENT_LINE_NOTE_LIST[_line]];
        if (_note.notePos.noteType == NoteType.Touch) {
            return;
        }

        float diff = Mathf.Abs(MusicSource.time - _note.notePos.notesTimeg);
        if (diff > JUDGE_RANGE[Judge.Pafect]) {
            return;
        }

        var judge = Judge.Miss;
        if (diff < JUDGE_RANGE[Judge.Pafect]) {
            judge = Judge.Pafect;
            ++parfectNum;
            //pafectText.text = parfectNum.ToString();
        }

        score += JUDGE_SCORE[judge];
        scoreText.text = score.ToString("D7");

        if (judge != Judge.Miss) {
            ++combo;
            comboText.text = combo.ToString();

        } else {
            combo = 0;
            comboText.text = combo.ToString();
        }
        if (maxCombo < combo) {
            maxCombo = combo;
            //maxComboText.text = maxCombo.ToString();
        }
        if (CURRENT_LINE_NOTE_LIST[_line] + 1 < LINE_NOTE_LIST[_line].Count) {
            CURRENT_LINE_NOTE_LIST[_line]++;
        } else {
            CURRENT_LINE_NOTE_LIST[_line] = -1;
        }
    }
    


    /// <summary>
    /// 押せなかったnotesをmissにする
    /// </summary>
    private void JudgeNotesMiss() {
        foreach (Line _line in System.Enum.GetValues(typeof(Line))) {
            if (CURRENT_LINE_NOTE_LIST[_line] < 0) {
                continue;
            }

            var note = LINE_NOTE_LIST[_line][CURRENT_LINE_NOTE_LIST[_line]];
            float diff = MusicSource.time - note.notePos.notesTimeg;
            if (diff > JUDGE_RANGE[Judge.Miss]) {
                ++missNum;
                combo = 0;
                comboText.text = combo.ToString();
                //missText.text = missNum.ToString();
                if (maxCombo < combo) {
                    maxCombo = combo;
                    //maxComboText.text = maxCombo.ToString();

                }

                if (CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] + 1 < LINE_NOTE_LIST[_line].Count) {
                    CURRENT_LINE_NOTE_LIST[note.notePos.lineNum]++;
                } else {
                    CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] = -1;
                }
            }
        }
    }


}