using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using NoteEditor.DTO;
using Config;
using Monochrome_Notes;


public class MainManager : MonoBehaviour {
    private float carrentGameTime = 0;
    [SerializeField][Range(1,20)]private int noteSpeed = 5;
    private readonly float DELAY_TIME = 4.5f;
    private float offset = 0;
    [SerializeField] [Range(-10, 10)] private int Fast_Slow = 0;
    private float timingAdjust = 0;
    private string musicName;
    private bool musicStart = false;
    private bool musicEnd = false;

    [SerializeField] private GameObject noteObj;
    [SerializeField] private GameObject holdNoteObj;
    [SerializeField] private GameObject holdNoteTapObj;
    [SerializeField] private GameObject holdEmptyObj;
    [SerializeField] private GameObject breakNotesObj;
    [SerializeField] private GameObject[] judgeLineObj;
    
    [SerializeField] private TextMesh scoreText;
    private int score = 0;
    [SerializeField] private Text comboText;
    private int combo;
    private int maxCombo;
    private int parfectNum;
    private int greatNum;
    private int missNum;
    private int noteCount = 0;
    
    [SerializeField] private Image gauge;
    private int maxScore = 0;

    [SerializeField] private TapEffect[] tapEffects;

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
    private static Dictionary<Line, List<Note>> LINE_NOTE_LIST = new Dictionary<Line, List<Note>>();
    
    /// <summary>
    /// ライン毎にノーツが残っているかを調べるindex
    /// </summary>
    private static  Dictionary<Line, int> CURRENT_LINE_NOTE_LIST = new Dictionary<Line, int>();

    /// <summary>
    /// それぞれのラインの座標
    /// </summary>
    private static Dictionary<Line, float> LINE_POSITION = new Dictionary<Line, float>();

    private readonly Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>()
    {
        {Judge.Pafect, 0.075f},
        {Judge.Graet, 0.1f},
        {Judge.Miss, 0.2f},
        {Judge.HoldStart,0.1f},
        {Judge.Hold,0.15f},
        {Judge.Break,0.2f},
    };

    //private readonly Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>()
    //{
    //    {Judge.Pafect, 0.04f},
    //    {Judge.Graet, 0.06f},
    //    {Judge.Miss, 0.08f},
    //    {Judge.HoldStart,0.06f},
    //    {Judge.Hold,0.06f},
    //};



    private static Dictionary<Judge, int> JUDGE_SCORE = new Dictionary<Judge, int>()
    {
        {Judge.Pafect,1000},
        {Judge.Graet,500},
        {Judge.Miss,0},
        {Judge.HoldStart,500},
        {Judge.Hold,50},
        {Judge.HoldEnd,500},
        {Judge.Break,1000}
    };

    private static Dictionary<Judge, Color> JUDGE_COLOR = new Dictionary<Judge, Color>()
    {
        {Judge.Pafect,Color.red},
        {Judge.Graet,Color.yellow},
        {Judge.Miss,Color.black},//shaderがParticles/Additive(Soft)の為,黒は表示されない
        {Judge.Hold,Color.red},
        {Judge.HoldStart,Color.red},
        {Judge.HoldEnd,Color.red},
        {Judge.Break,Color.magenta},
    };

    [SerializeField] private List<AudioClip> bgmList;
    [SerializeField] private List<AudioClip> seList;
    private AudioSource MusicSource;
    [SerializeField] private AudioSource[] SESource;

    //[SerializeField] private Text musicNameText;
    [SerializeField] private TextMesh musicNameTextMesh;

    //[SerializeField] private Text bpmText;
    //[SerializeField] private Text speedText;
    //[SerializeField] private Text humennteisuu;

    
    [SerializeField] private GameObject ResultCanvas;
    [SerializeField] private Text testResult;
    [SerializeField] private Text fullComboText;
    [SerializeField] private Text maxComboText;
    [SerializeField] private Text parfectText;
    [SerializeField] private Text greatText;
    [SerializeField] private Text missText;
    [SerializeField] private Text scoreResultText;
    [SerializeField] private Text parScore;
    [SerializeField] private Image parfectGauge;
    [SerializeField] private Image greatGauge;
    [SerializeField] private Image missGauge;


    [SerializeField] private GameObject PouseCanvas;


    [SerializeField] private Image[] Nexts = new Image[2];
    private SceneName nextScene = SceneName.MusicSelect;

    [SerializeField] private Animator comboAnimator;


    private void Awake() {
        NOTES_LIST = new List<Note.NotePos>();
        LINE_NOTE_LIST = new Dictionary<Line, List<Note>>();
        CURRENT_LINE_NOTE_LIST = new Dictionary<Line, int>();
        LINE_POSITION = new Dictionary<Line, float>();
    }

    // Use this for initialization
    void Start() {
        noteSpeed = (int)GameMaster.NoteSpeed * 3;
        musicName = GameMaster.MusicName;
        Fast_Slow = GameMaster.Ajust;
        carrentGameTime = 0;

        //Jsonから譜面をもらってきてEditDate,Noteクラスとして復元
        string json = Resources.Load<TextAsset>(musicName).ToString();
        editData = JsonUtility.FromJson<MusicDTO.EditData>(json);
        musicNote = JsonUtility.FromJson<MusicDTO.Note>(json);

        //audioSource.clipにmusicNameと同じ名前のものを渡す
        MusicSource = GetComponent<AudioSource>();
        MusicSource.clip = bgmList.Find(bgm => bgm.name == musicName);

        foreach (Line line in Enum.GetValues(typeof(Line))) {
            LINE_POSITION.Add(line,judgeLineObj[(int)line].transform.position.x);
        }

        musicNameTextMesh.text = musicName;
        //bpmText.text = "BPM : " + editData.BPM.ToString();
        //speedText.text = "Speed : "+ noteSpeed.ToString();

        foreach (var source in SESource) {
            source.clip = seList[0];
        }
        timingAdjust = (float)Fast_Slow / 100;
        Initialize();
    }

    // Update is called once per frame
    void Update() {
        //曲が流れ始めるまでに一拍置く処理
        if (carrentGameTime < DELAY_TIME) {
            carrentGameTime += GameMaster.DeltaTime;
        } else if (!musicStart) {
            MusicSource.Play();
            musicStart = true;
        }

        //ノーツの位置を更新する処理
        foreach (var notes in LINE_NOTE_LIST.Values) {
            foreach (var note in notes) {
                var pos = note.transform.position;
                pos.z = judgeLineObj[(int)note.notePos.lineNum].transform.position.z + (note.notePos.notesTimeg - MusicSource.time - carrentGameTime + DELAY_TIME) * noteSpeed;
                note.transform.position = pos;
            }
        }


        JudgeNotesMiss();
        ComboTextUpdate();

        if (MusicSource.time == MusicSource.clip.length && !musicEnd) {
            string resultT = "D";
            float resultF = (float)score / maxScore;
            if (resultF <= 0.7f) {
                resultT = "C";
            } else if (resultF <= 0.85f) {
                resultT = "B";
            } else if (resultF <= 0.95f) {
                resultT = "A";
            } else if (resultF <= 0.98f) {
                resultT = "S";
            } else if (resultF < 1) {
                resultT = "SS";
            } else if (resultF == 1) {
                resultT = "SSS";
                fullComboText.text = "ALL Parfect !!";
            }
            testResult.text = resultT;
            scoreResultText.text = score.ToString();
            parfectText.text = parfectNum.ToString();
            greatText.text = greatNum.ToString();
            missText.text = missNum.ToString();
            maxComboText.text = maxCombo.ToString();
            parScore.text = (resultF * 100).ToString() + "%";
            ResultCanvas.SetActive(true);
            if (maxCombo == noteCount) {
                fullComboText.gameObject.SetActive(true);
            }
            musicEnd = true;
        }

        if (musicEnd) {

            parfectGauge.fillAmount = Mathf.MoveTowards(parfectGauge.fillAmount, (float)parfectNum / noteCount, 1 * GameMaster.DeltaTime);
            greatGauge.fillAmount = Mathf.MoveTowards(greatGauge.fillAmount, (float)greatNum / noteCount, 1 * GameMaster.DeltaTime);
            missGauge.fillAmount = Mathf.MoveTowards(missGauge.fillAmount, (float)missNum / noteCount, 1 * GameMaster.DeltaTime);
            
            switch (nextScene) {
                case SceneName.Main:
                    Nexts[0].color = new Color(0.38f, 0.38f, 0.38f, 1);
                    Nexts[1].color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                    Nexts[2].color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                    if (Button2.GetButtonDown("Button2_2")) {
                        if (Input.GetAxisRaw("Button2_2") < 1) {
                            nextScene = SceneName.Main;
                        } else {
                            nextScene = SceneName.MusicSelect;
                        }
                    }
                    break;
                case SceneName.MusicSelect:
                    Nexts[0].color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                    Nexts[1].color = new Color(0.38f, 0.38f, 0.38f, 1f);
                    Nexts[2].color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                    if (Button2.GetButtonDown("Button2_2")) {
                        if (Input.GetAxisRaw("Button2_2") < 1) {
                            nextScene = SceneName.Main;
                        } else {
                            nextScene = SceneName.Title;
                        }
                    }
                    break;
                case SceneName.Title:
                    Nexts[0].color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                    Nexts[1].color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                    Nexts[2].color = new Color(0.38f, 0.38f, 0.38f, 1f);
                    if (Button2.GetButtonDown("Button2_2")) {
                        if (Input.GetAxisRaw("Button2_2") < 1) {
                            nextScene = SceneName.MusicSelect;
                        } else {
                            nextScene = SceneName.Title;
                        }
                    }
                    break;
                default:
                    break;
            }
            if (Input.GetButtonDown("Return")) {
                GameMaster.SceneChanger(nextScene);
            }
        } else {
            if (Input.GetButtonDown("Button1")) {
                JudgeNotes(Line.Line1);
            }
            if (Input.GetButtonDown("Button2") || Button2.GetButtonDown("Button2_2") || Button2_2.GetButtonDown("Button2_3")) {
                JudgeNotes(Line.Line2);
            }
            if (Input.GetButtonDown("Button3")) {
                JudgeNotes(Line.Line3);
            }
            if (Input.GetButtonDown("Button4")) {
                JudgeNotes(Line.Line4);
            }
            if (Input.GetButtonDown("Button5")) {
                JudgeNotes(Line.Line5);
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

        Pouse();

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
        foreach (var _notes in musicNote.notes) {
            //ノーツのある時間 = ノーツの最短間隔[60f / (BPM * LPB)] * エディタ上のノーツの位置 + 曲が再生されるまでのラグ + タイミング調整
            Func<float, float> _notesTiming = (float x) => 60f / (editData.BPM * _notes.LPB) * x + offset + timingAdjust;
            Note.NotePos _notePos;
            if (_notes.type != 2) {
                _notePos = new Note.NotePos(_notesTiming(_notes.num), _notes.block, _notes.type);
            } else {
                List<Note> _noteList = new List<Note>();
                var _beforeNotes = _notes;
                foreach (var _hold in _notes.notes) {
                    for (int _num = _beforeNotes.num + 2; _num < _hold.num; _num+=2) {
                        Note _holdNotes = new Note();
                        _holdNotes.Initialize(new Note.NotePos(_notesTiming(_num), _hold.block, NoteType.Hold));
                        _beforeNotes = _hold;
                        _noteList.Add(_holdNotes);
                    }

                    Note _holdEnd = new Note();
                    _holdEnd.Initialize(new Note.NotePos(_notesTiming(_hold.num), _hold.block, NoteType.HoldEnd));
                    _noteList.Add(_holdEnd);
                }
                _notePos = new Note.NotePos(_notesTiming(_notes.num), _notes.block, _notes.type, _noteList);
            }
            NOTES_LIST.Add(_notePos);
        }

        foreach (Line line in System.Enum.GetValues(typeof(Line))) {
            LINE_NOTE_LIST.Add(line, new List<Note>());
            CURRENT_LINE_NOTE_LIST.Add(line, -1);
        }

        foreach (var data in NOTES_LIST) {
            GameObject obj;
            switch (data.noteType) {
                case NoteType.Touch:
                    obj = Instantiate(noteObj, transform);
                    break;
                case NoteType.HoldStart:
                    obj = Instantiate(holdNoteTapObj, transform);
                    break;
                case NoteType.Break:
                    obj = Instantiate(breakNotesObj, transform);
                    break;
                default:
                    obj = Instantiate(noteObj, transform);
                    break;
            }
            var note = obj.GetComponent<Note>();
            note.Initialize(data);
            var pos = obj.transform.position;
            pos.x = LINE_POSITION[note.notePos.lineNum];
            pos.z = judgeLineObj[(int)data.lineNum].transform.position.z + note.notePos.notesTimeg * noteSpeed;
            note.transform.position = pos;
            note.transform.transform.eulerAngles = new Vector3(90, 0, 0);

            float spriteX = 2f;
            if (note.notePos.lineNum == Line.Line1 || note.notePos.lineNum == Line.Line4) {
                spriteX = 0.8f;
            }
            if (note.notePos.lineNum == Line.Line5) {
                spriteX = 10.5f;
            }
            var spritRenderer = obj.GetComponent<SpriteRenderer>();
            var spritSize = spritRenderer.size;
            spritSize.x = spriteX;
            spritRenderer.size = spritSize;

            LINE_NOTE_LIST[note.notePos.lineNum].Add(note);
            if (CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] < 0) {
                CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] = 0;
            }
            
            //ホールドノーツの生成
            if (note.notePos.noteType == NoteType.HoldStart) {
                maxScore += JUDGE_SCORE[Judge.HoldStart];
                noteCount++;
                var _holdEnd = note.gameObject;
                foreach (var _data in data.noteList) {
                    GameObject _obj;
                    var _note = new Note();
                    var _notePos = new Note.NotePos();
                    Judge _judge;

                    if (_data.notePos.noteType == NoteType.Hold) {
                        _obj = Instantiate(holdEmptyObj, transform);
                        _note = _obj.GetComponent<Note>();
                        _notePos = new Note.NotePos(_data.notePos.notesTimeg, _data.notePos.lineNum, NoteType.Hold);
                        _judge = Judge.Hold;
                    } else {
                        _obj = Instantiate(holdNoteTapObj, transform);
                        _note = _obj.GetComponent<Note>();
                        _notePos = new Note.NotePos(_data.notePos.notesTimeg, _data.notePos.lineNum, NoteType.HoldEnd);
                        _judge = Judge.HoldEnd;
                    }

                    _note.Initialize(_notePos);
                    var _pos = _obj.transform.position;
                    _pos.x = LINE_POSITION[_note.notePos.lineNum];
                    _pos.z = judgeLineObj[(int)_data.notePos.lineNum].transform.position.z + _note.notePos.notesTimeg * noteSpeed;
                    _obj.transform.parent = obj.transform;
                    _note.transform.position = _pos;

                    if (_data.notePos.noteType == NoteType.HoldEnd) {
                        if (_note.notePos.lineNum == Line.Line1 || _note.notePos.lineNum == Line.Line4) {
                            spriteX = 0.8f;
                        }
                        if (_note.notePos.lineNum == Line.Line5) {
                            spriteX = 10.5f;
                        }
                        var _spritRenderer = _obj.GetComponent<SpriteRenderer>();
                        var _spritSize = _spritRenderer.size;
                        _spritSize.x = spriteX;
                        _spritRenderer.size = _spritSize;

                        _holdEnd = _obj;
                    }
                    LINE_NOTE_LIST[_note.notePos.lineNum].Add(_note);
                    maxScore += JUDGE_SCORE[_judge];
                    noteCount++;
                }
                if (_holdEnd.GetComponent<Note>().notePos.noteType == NoteType.HoldEnd) {
                    var holdObj = Instantiate(holdNoteObj, transform);
                    var _holdNoteSpritRenderer = holdObj.GetComponent<SpriteRenderer>();
                    var _holdNotesSpritSize = _holdNoteSpritRenderer.size;
                    _holdNotesSpritSize.x = spriteX;
                    _holdNotesSpritSize.y = obj.transform.position.z - _holdEnd.transform.position.z - 0.2f;
                    _holdNoteSpritRenderer.size = _holdNotesSpritSize;
                    holdObj.transform.position = (obj.transform.position + _holdEnd.transform.position) / 2;
                    holdObj.transform.eulerAngles = new Vector3(90, 0, 0);
                    holdObj.transform.parent = obj.transform;
                }

            } else {
                maxScore += JUDGE_SCORE[Judge.Pafect];
                noteCount++;
            }
        }
    }

    /// <summary>
    /// タッチノーツの判定をとる関数
    /// </summary>
    /// <param name="_line"></param>
    void JudgeNotes(Line _line) {
        //空打ち処理
        //曲が始まるまでの空打ち
        if (carrentGameTime < DELAY_TIME - JUDGE_RANGE[Judge.Miss]) {
            tapEffects[(int)_line].SetEffectData(Color.white, 300f);
            SESource[0].Play();
            return;
        }

        //LINEにもうノーツが無いときの空打ち
        if (CURRENT_LINE_NOTE_LIST[_line] < 0) {
            tapEffects[(int)_line].SetEffectData(Color.white, 300f);
            SESource[0].Play();
            return;
        }

        //ノーツまで距離(時間)があるときの空打ち
        var note = LINE_NOTE_LIST[_line][CURRENT_LINE_NOTE_LIST[_line]];
        float diff = Mathf.Abs(MusicSource.time - note.notePos.notesTimeg);
        if (diff > JUDGE_RANGE[Judge.Miss]) {
            tapEffects[(int)_line].SetEffectData(Color.white, 300f);
            SESource[0].Play();
            return;
        }

        var judge = Judge.Miss;

        switch (note.notePos.noteType) {
            case NoteType.Touch:
                if (diff < JUDGE_RANGE[Judge.Pafect]) {
                    judge = Judge.Pafect;
                    ++parfectNum;

                } else if (diff < JUDGE_RANGE[Judge.Graet]) {
                    judge = Judge.Graet;
                    ++greatNum;
                } else if (diff < JUDGE_RANGE[Judge.Miss]) {
                    ++missNum;
                }
                break;
            case NoteType.HoldStart:
                if (diff < JUDGE_RANGE[Judge.HoldStart]) {
                    judge = Judge.HoldStart;
                    ++parfectNum;
                } else if (diff < JUDGE_RANGE[Judge.Miss]) {
                    ++missNum;
                }
                break;
            case NoteType.Hold:
            case NoteType.HoldEnd:
                return;
            case NoteType.Break:
                if (diff < JUDGE_RANGE[Judge.Break]) {
                    judge = Judge.Break;
                    ++parfectNum;
                }
                break;
            default:
                break;
        }

        SESource[0].Play();
        score += JUDGE_SCORE[judge];
        scoreText.text = "Score : " + score.ToString("D7");

        tapEffects[(int)_line].SetEffectData(JUDGE_COLOR[judge], 300f);

        if (judge != Judge.Miss) {
            ++combo;
            
        } else {
            combo = 0;
        }
        if (maxCombo < combo) {
            maxCombo = combo;
        }
        if (CURRENT_LINE_NOTE_LIST[_line] + 1 < LINE_NOTE_LIST[_line].Count) {
            CURRENT_LINE_NOTE_LIST[_line]++;
        } else {
            CURRENT_LINE_NOTE_LIST[_line] = -1;
        }
    }


    private void JudgeHoldNotes(Line _line) {
        if (carrentGameTime < DELAY_TIME - JUDGE_RANGE[Judge.Miss]) {
            return;
        }

        if (CURRENT_LINE_NOTE_LIST[_line] < 0) {
            return;
        }

        var _note = LINE_NOTE_LIST[_line][CURRENT_LINE_NOTE_LIST[_line]];
        float diff = Mathf.Abs(MusicSource.time - _note.notePos.notesTimeg);
        if (diff > JUDGE_RANGE[Judge.Pafect]) {
            return;
        }

        //TouchとHoldStart以外なら処理しない
        if (_note.notePos.noteType == NoteType.Touch || _note.notePos.noteType == NoteType.HoldStart) {
            return;
        }

        var judge = Judge.Hold;
        if (diff < JUDGE_RANGE[Judge.Hold]) {
            if(_note.notePos.noteType == NoteType.HoldEnd) {
                SESource[0].Play();
                judge = Judge.HoldEnd;
            }
            ++parfectNum;
        }

        score += JUDGE_SCORE[judge];
        scoreText.text = "Score : " + score.ToString("D7");
        ++combo;

        tapEffects[(int)_line].SetEffectData(JUDGE_COLOR[judge], 150f);

        if (maxCombo < combo) {
            maxCombo = combo;
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
                if (maxCombo < combo) {
                    maxCombo = combo;
                }

                if (CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] + 1 < LINE_NOTE_LIST[_line].Count) {
                    CURRENT_LINE_NOTE_LIST[note.notePos.lineNum]++;
                } else {
                    CURRENT_LINE_NOTE_LIST[note.notePos.lineNum] = -1;
                }
            }
        }
    }

    bool b = true;
    float time = 0f;
    private void ComboTextUpdate() {
        if (combo < 10) {
            comboText.gameObject.SetActive(false);
        } else if(b){
            comboText.gameObject.SetActive(true);
            comboText.text = combo.ToString();
        }

        if(combo % 100 == 0) {
            comboAnimator.SetBool("Test",true);
            b = false;
        }

        if (!b) {
            time += GameMaster.DeltaTime;
        }

        if(time > 0.5f) {
            b = true;
            time = 0f;
            comboAnimator.SetBool("Test", false);
        }
    }

    //中断処理
    private void Pouse() {
        if (MusicSource.pitch == 1 && MusicSource.time < MusicSource.clip.length) {
            if (Input.GetButtonDown("Pouse")) {
                MusicSource.pitch = 0;
                PouseCanvas.SetActive(true);
            }
        } else {
            
        }


    }
}