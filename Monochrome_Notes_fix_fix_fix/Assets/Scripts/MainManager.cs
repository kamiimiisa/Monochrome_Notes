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
    [SerializeField] [Range(1, 20)] private int noteSpeed = 5;
    private readonly float DELAY_TIME = 4.5f;
    private string musicName;
    private string musicLevel;
    private bool musicStart = false;
    private bool musicEnd = false;
    private bool canPlay = true;

    [SerializeField] private GameObject noteObj;
    [SerializeField] private GameObject holdNoteObj;
    [SerializeField] private GameObject holdNoteTapObj;
    [SerializeField] private GameObject holdEmptyObj;
    [SerializeField] private GameObject breakNotesObj;
    [SerializeField] private GameObject[] judgeLineObj;
    [SerializeField] private GameObject[] joystickButton;
    [SerializeField] private GameObject[] keyboradButton;

    [SerializeField] private TextMesh scoreText;
    private float score = 0;
    [SerializeField] private Text comboText;
    [SerializeField] private Text comboBonusText;
    private int combo;
    private int maxCombo;
    private int parfectNum;
    private int greatNum;
    private int missNum;
    private static int safeNum;
    public static int SafeNum {
        set { safeNum = value; }
    }
    private int noteCount = 0;
    private Character character;

    [SerializeField] private Image gauge;

    [SerializeField] private TapEffect[] tapEffects;

    private MusicDTO.EditData editData;
    private MusicDTO.Note musicNote;

    private MyInput Button2_Horizontal = new MyInput();
    private MyInput Button2_Vartical = new MyInput();


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
    private static Dictionary<Line, int> CURRENT_NOTES = new Dictionary<Line, int>();

    /// <summary>
    /// それぞれのラインの座標
    /// </summary>
    private static Dictionary<Line, float> LINE_POSITION = new Dictionary<Line, float>();

    public static Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>()
    {
        {Judge.Pafect, 0.075f},
        {Judge.Graet, 0.1f},
        {Judge.Miss, 0.2f},
        {Judge.HoldStart,0.1f},
        {Judge.Hold,0.05f},
        {Judge.ExTap,0.2f},
    };

    public static Dictionary<Judge, int> JUDGE_SCORE = new Dictionary<Judge, int>()
    {
        {Judge.Pafect,1000},
        {Judge.Graet,500},
        {Judge.Miss,0},
        {Judge.HoldStart,500},
        {Judge.Hold,50},
        {Judge.HoldEnd,500},
        {Judge.ExTap,2000},
    };

    private static Dictionary<Judge, Color> JUDGE_COLOR = new Dictionary<Judge, Color>()
    {
        {Judge.Pafect,Color.red},
        {Judge.Graet,Color.yellow},
        {Judge.Miss,Color.black},//shaderがParticles/Additive(Soft)の為,黒は表示されない
        {Judge.Hold,Color.red},
        {Judge.HoldStart,Color.red},
        {Judge.HoldEnd,Color.red},
        {Judge.ExTap,Color.red},
    };

    [SerializeField] private List<AudioClip> bgmList;
    [SerializeField] private List<AudioClip> seList;
    private AudioSource MusicSource;
    [SerializeField] private AudioSource SESource;

    [SerializeField] private TextMesh musicNameTextMesh;
    //[SerializeField] private Text musicNameText;

    [SerializeField] private GameObject ResultCanvas;
    [SerializeField] private Text resultMusicName;
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
    [SerializeField] private GameObject resultCharacterImage;


    [SerializeField] private GameObject PouseCanvas;


    [SerializeField] private Image[] Nexts = new Image[2];
    [SerializeField] private Image[] PouseButton;
    private SceneName nextScene = SceneName.Main;
    private ResultStatus currentResultStatus = ResultStatus.MusicSelect;
    private PouseStatus currentPouseStatus = PouseStatus.Resume;

    [SerializeField] private Animator comboAnimator;

    private void Awake() {
        NOTES_LIST = new List<Note.NotePos>();
        LINE_NOTE_LIST = new Dictionary<Line, List<Note>>();
        CURRENT_NOTES = new Dictionary<Line, int>();
        LINE_POSITION = new Dictionary<Line, float>();
    }

    // Use this for initialization
    void Start() {
        GameMaster.UiChangerConst(keyboradButton, joystickButton);

        noteSpeed = (int)GameMaster.NoteSpeed * 3;
        musicName = GameMaster.MusicName;
        musicLevel = GameMaster.MusicLevel;
        carrentGameTime = 0;
        safeNum = 0;

        //Jsonから譜面をもらってきてEditDate,Noteクラスとして復元
        string json = Resources.Load<TextAsset>(musicLevel + "/" + musicName).ToString();
        editData = JsonUtility.FromJson<MusicDTO.EditData>(json);
        musicNote = JsonUtility.FromJson<MusicDTO.Note>(json);

        //audioSource.clipにmusicNameと同じ名前のものを渡す
        MusicSource = GetComponent<AudioSource>();
        MusicSource.clip = bgmList.Find(bgm => bgm.name == musicName);

        //判定ラインの座標を記憶する
        foreach (Line line in Enum.GetValues(typeof(Line))) {
            LINE_POSITION.Add(line,judgeLineObj[(int)line].transform.position.x);
        }

        //曲名の反映
        musicNameTextMesh.text = musicName;
        resultMusicName.text = musicName;

        //キャラクターの反映
        character = GameMaster.Character;
        resultCharacterImage.GetComponent<Image>().sprite = character.CharacterSprite;
        character.Skill();
        
        SESource.clip = seList[0];
        DataFormat();
        Initialize();
    }

    // Update is called once per frame
    void Update() {
        GameMaster.UiChanger(keyboradButton, joystickButton);

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

        if (MusicSource.time >= MusicSource.clip.length && !musicEnd) {
            string resultT = "D";
            float resultF = (float)(parfectNum + greatNum) / noteCount;
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
            
            switch (currentResultStatus) {
                case ResultStatus.Retry:
                    GameMaster.SetColors(Nexts, (int)currentResultStatus);
                    nextScene = SceneName.Main;
                    currentResultStatus = GameMaster.HorizontalSelect<ResultStatus>(currentResultStatus, ResultStatus.Title, ResultStatus.MusicSelect);
                    break;
                case ResultStatus.MusicSelect:
                    GameMaster.SetColors(Nexts, (int)currentResultStatus);
                    nextScene = SceneName.MusicSelect;
                    currentResultStatus = GameMaster.HorizontalSelect<ResultStatus>(currentResultStatus, ResultStatus.Retry, ResultStatus.Title);
                    break;
                case ResultStatus.Title:
                    GameMaster.SetColors(Nexts, (int)currentResultStatus);
                    nextScene = SceneName.Title;
                    currentResultStatus = GameMaster.HorizontalSelect<ResultStatus>(currentResultStatus, ResultStatus.MusicSelect, ResultStatus.Retry);
                    break;
                default:
                    break;
            }
            if (Input.GetButtonDown("Return")) {
                GameMaster.SceneChanger(nextScene);
            }
        } else {
            if (canPlay) {
                if (Input.GetButtonDown("Button1")) {
                    SESource.Play();
                    JudgeNotes(Line.Line1);
                    JudgeNotes(Line.Line5);
                    JudgeNotes(Line.Line8);
                }
                if (Input.GetButtonDown("Button2") || Button2_Horizontal.GetButtonDown("Button2_Horizontal") || Button2_Vartical.GetButtonDown("Button2_Vartical")) {
                    SESource.Play();
                    JudgeNotes(Line.Line2);
                    JudgeNotes(Line.Line5);
                    JudgeNotes(Line.Line6);
                    JudgeNotes(Line.Line8);
                }
                if (Input.GetButtonDown("Button3")) {
                    SESource.Play();
                    JudgeNotes(Line.Line3);
                    JudgeNotes(Line.Line6);
                    JudgeNotes(Line.Line7);
                    JudgeNotes(Line.Line8);
                }
                if (Input.GetButtonDown("Button4")) {
                    SESource.Play();
                    JudgeNotes(Line.Line4);
                    JudgeNotes(Line.Line7);
                    JudgeNotes(Line.Line8);
                }
                if (Input.GetButtonDown("Button5")) {
                    SESource.Play();
                    JudgeNotes(Line.Line5);
                    JudgeNotes(Line.Line6);
                    JudgeNotes(Line.Line7);
                    JudgeNotes(Line.Line8);
                }

                if (Input.GetButton("Button1")) {
                    JudgeHoldNotes(Line.Line1);
                    JudgeHoldNotes(Line.Line5);
                    JudgeHoldNotes(Line.Line8);
                }
                if (Input.GetButton("Button2") || Button2_Horizontal.GetButton("Button2_Horizontal") || Button2_Vartical.GetButton("Button2_Vartical")) {
                    JudgeHoldNotes(Line.Line2);
                    JudgeHoldNotes(Line.Line5);
                    JudgeHoldNotes(Line.Line6);
                    JudgeHoldNotes(Line.Line8);
                }
                if (Input.GetButton("Button3")) {
                    JudgeHoldNotes(Line.Line3);
                    JudgeHoldNotes(Line.Line6);
                    JudgeHoldNotes(Line.Line7);
                    JudgeHoldNotes(Line.Line8);
                }
                if (Input.GetButton("Button4")) {
                    JudgeHoldNotes(Line.Line4);
                    JudgeHoldNotes(Line.Line7);
                    JudgeHoldNotes(Line.Line8);
                }
            }
        }

        Pouse();

        //クリアゲージを更新する処理
        gauge.fillAmount = (float)(parfectNum + greatNum) / noteCount;
    }

    private void DataFormat() {
        float offset = (float)editData.offset / (float)MusicSource.clip.frequency;
        float timingAdjust = (float)GameMaster.Ajust / 100;
        //Jsonから受け取ったデータを使いやすいように変換する処理
        foreach (var _note in musicNote.notes) {
            //ノーツのある時間 = ノーツの最短間隔[60f / (BPM * LPB)] * エディタ上のノーツの位置 + 曲が再生されるまでのラグ + タイミング調整
            Func<float, float> _notesTiming = (float x) => 60f / (editData.BPM * _note.LPB) * x + offset + timingAdjust;
            Note.NotePos _notePos;
            if (_note.type != 2) {
                if (_note.block >= 4) {
                    _notePos = new Note.NotePos(_notesTiming(_note.num),_note.block, NoteType.ExTap);
                } else {
                    _notePos = new Note.NotePos(_notesTiming(_note.num), _note.block, _note.type);
                }
            } else {
                List<Note> _noteList = new List<Note>();
                var _beforeNotes = _note;
                foreach (var _hold in _note.notes) {
                    for (int _num = _beforeNotes.num + 2; _num < _hold.num; _num += 2) {
                        Note _holdNotes = new Note();
                        _holdNotes.Initialize(new Note.NotePos(_notesTiming(_num), _hold.block, NoteType.Hold));
                        _beforeNotes = _hold;
                        _noteList.Add(_holdNotes);
                    }

                    Note _holdEnd = new Note();
                    _holdEnd.Initialize(new Note.NotePos(_notesTiming(_hold.num), _hold.block, NoteType.HoldEnd));
                    _noteList.Add(_holdEnd);
                }
                _notePos = new Note.NotePos(_notesTiming(_note.num), _note.block, _note.type, _noteList);
            }
            NOTES_LIST.Add(_notePos);
        }
    }

    /// <summary>
    /// ノーツを生成する関数
    /// </summary>
    void Initialize() {
        score = 0;
        combo = 0;

        foreach (Line line in System.Enum.GetValues(typeof(Line))) {
            LINE_NOTE_LIST.Add(line, new List<Note>());
            CURRENT_NOTES.Add(line, -1);
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
                case NoteType.ExTap:
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
            
            float spriteX = 2.5f;
            if (note.notePos.lineNum == Line.Line5 || note.notePos.lineNum == Line.Line6 || note.notePos.lineNum == Line.Line7) {
                spriteX = 5f;
            }
            if(note.notePos.lineNum == Line.Line8) {
                spriteX = 10f;
            }
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            var spritSize = spriteRenderer.size;
            spritSize.x = spriteX;
            spriteRenderer.size = spritSize;


            LINE_NOTE_LIST[note.notePos.lineNum].Add(note);
            if (CURRENT_NOTES[note.notePos.lineNum] < 0) {
                CURRENT_NOTES[note.notePos.lineNum] = 0;
            }
            
            //ホールドノーツの生成
            if (note.notePos.noteType == NoteType.HoldStart) {
                noteCount++;
                var _holdEnd = note.gameObject;
                foreach (var _data in data.noteList) {
                    GameObject _obj;
                    var _note = new Note();
                    var _notePos = new Note.NotePos();
                    if (_data.notePos.noteType == NoteType.Hold) {
                        _obj = Instantiate(holdEmptyObj, transform);
                        _note = _obj.GetComponent<Note>();
                        _notePos = new Note.NotePos(_data.notePos.notesTimeg, _data.notePos.lineNum, NoteType.Hold);
                    } else {
                        _obj = Instantiate(holdNoteTapObj, transform);
                        _note = _obj.GetComponent<Note>();
                        _notePos = new Note.NotePos(_data.notePos.notesTimeg, _data.notePos.lineNum, NoteType.HoldEnd);
                        var _spriteRenderer = _obj.GetComponent<SpriteRenderer>();
                        var _spritSize = spriteRenderer.size;
                        _spritSize.x = spriteX;
                        _spriteRenderer.size = spritSize;

                    }

                    _note.Initialize(_notePos);
                    var _pos = _obj.transform.position;
                    _pos.x = LINE_POSITION[_note.notePos.lineNum];
                    _pos.z = judgeLineObj[(int)_data.notePos.lineNum].transform.position.z + _note.notePos.notesTimeg * noteSpeed;
                    _obj.transform.parent = obj.transform;
                    _note.transform.position = _pos;
                    if (_data.notePos.noteType == NoteType.HoldEnd) {
                        _note.transform.eulerAngles = new Vector3(90, 0, 0);
                        _holdEnd = _obj;
                    }
                    LINE_NOTE_LIST[_note.notePos.lineNum].Add(_note);
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
            return;
        }

        //LINEにもうノーツが無いときの空打ち
        if (CURRENT_NOTES[_line] < 0) {
            return;
        }

        //ノーツまで距離(時間)があるときの空打ち
        var note = LINE_NOTE_LIST[_line][CURRENT_NOTES[_line]];
        float diff = Mathf.Abs(MusicSource.time - note.notePos.notesTimeg);
        if (diff > JUDGE_RANGE[Judge.Miss]) {
            return;
        }        

        var judge = Judge.Miss;
        var judgeEff = Note.Eff.Miss;

        switch (note.notePos.noteType) {
            case NoteType.Touch:
                if (diff < JUDGE_RANGE[Judge.Pafect]) {
                    judge = Judge.Pafect;
                    judgeEff = Note.Eff.Pafect;
                    ++parfectNum;

                } else if (diff < JUDGE_RANGE[Judge.Graet]) {
                    judge = Judge.Graet;
                    judgeEff = Note.Eff.Great;
                    ++greatNum;
                } 
                break;
            case NoteType.HoldStart:
                if (diff < JUDGE_RANGE[Judge.HoldStart]) {
                    judge = Judge.HoldStart;
                    judgeEff = Note.Eff.Pafect;
                    ++parfectNum;
                }
                break;
            case NoteType.Hold:
            case NoteType.HoldEnd:
                return;
            case NoteType.ExTap:
                if (diff < JUDGE_RANGE[Judge.ExTap]) {
                    judge = Judge.ExTap;
                    judgeEff = Note.Eff.Pafect;
                    ++parfectNum;
                }
                break;
            default:
                break;
        }

        if (judge == Judge.Miss) {
            ++missNum;
            if (safeNum > 0) {
                judgeEff = Note.Eff.Safe;
            }
        }
        
        score += JUDGE_SCORE[judge] * ComboBonus(combo);
        scoreText.text = "Score : " + ((int)score).ToString("D7");
        tapEffects[(int)_line].SetEffectData(JUDGE_COLOR[judge], 250);
        note.NotesEff(judgeEff, judgeLineObj[(int)_line].transform.position);
        
        if (judge != Judge.Miss) {
            ++combo;
        } else {
            if(safeNum > 0) {
                safeNum--;
            } else {
                combo = 0;
                comboBonusText.gameObject.SetActive(false);
            }
        }

        if (maxCombo < combo) {
            maxCombo = combo;
        }
        if (CURRENT_NOTES[_line] + 1 < LINE_NOTE_LIST[_line].Count) {
            CURRENT_NOTES[_line]++;
        } else {
            CURRENT_NOTES[_line] = -1;
        }
    }


    private void JudgeHoldNotes(Line _line) {
        if (carrentGameTime < DELAY_TIME - JUDGE_RANGE[Judge.Miss]) {
            return;
        }

        if (CURRENT_NOTES[_line] < 0) {
            return;
        }

        var _note = LINE_NOTE_LIST[_line][CURRENT_NOTES[_line]];
        float diff = Mathf.Abs(MusicSource.time - _note.notePos.notesTimeg);
        if (diff > JUDGE_RANGE[Judge.Hold]) {
            return;
        }

        //TouchとHoldStartとBreak以外なら処理しない
        if (_note.notePos.noteType == NoteType.Touch || _note.notePos.noteType == NoteType.HoldStart || _note.notePos.noteType == NoteType.ExTap) {
            return;
        }

        Judge judge = Judge.Hold;
        if (_note.notePos.noteType == NoteType.Hold || _note.notePos.noteType == NoteType.HoldEnd) {
            if (diff < JUDGE_RANGE[Judge.Hold]) {
                if (_note.notePos.noteType == NoteType.HoldEnd) {
                    SESource.Play();
                    judge = Judge.HoldEnd;
                }
                ++parfectNum;
            }
        }
        score += JUDGE_SCORE[judge] * ComboBonus(combo);
        scoreText.text = "Score : " + ((int)score).ToString("D7");
        ++combo;

        tapEffects[(int)_line].SetEffectData(JUDGE_COLOR[judge],125);
        _note.NotesEff(Note.Eff.Pafect,judgeLineObj[(int)_line].transform.position);

        if (maxCombo < combo) {
            maxCombo = combo;
        }
        if (CURRENT_NOTES[_line] + 1 < LINE_NOTE_LIST[_line].Count) {
            CURRENT_NOTES[_line]++;
        } else {
            CURRENT_NOTES[_line] = -1;
        }
    }
    


    /// <summary>
    /// 押せなかったnotesをmissにする
    /// </summary>
    private void JudgeNotesMiss() {
        foreach (Line _line in System.Enum.GetValues(typeof(Line))) {
            if (CURRENT_NOTES[_line] < 0) {
                continue;
            }

            var note = LINE_NOTE_LIST[_line][CURRENT_NOTES[_line]];
            float diff = MusicSource.time - note.notePos.notesTimeg;
            float missTime = JUDGE_RANGE[Judge.Miss];
            if (note.notePos.noteType == NoteType.Hold) {
                missTime = JUDGE_RANGE[Judge.Hold];
            }
            if (diff > missTime) {
                var _eff = Note.Eff.Miss;
                if(safeNum > 0) {
                    _eff = Note.Eff.Safe;
                    safeNum--;
                }else{
                    combo = 0;
                    comboBonusText.gameObject.SetActive(false);
                }
                ++missNum;
                comboText.text = combo.ToString();
                if (maxCombo < combo) {
                    maxCombo = combo;
                }
                note.NotesEff(_eff, judgeLineObj[(int)_line].transform.position);
                if (CURRENT_NOTES[note.notePos.lineNum] + 1 < LINE_NOTE_LIST[_line].Count) {
                    CURRENT_NOTES[note.notePos.lineNum]++;
                } else {
                    CURRENT_NOTES[note.notePos.lineNum] = -1;
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
        if (MusicSource.pitch == 1 && MusicSource.time < MusicSource.clip.length && !musicEnd) {
            if (Input.GetButtonDown("Pouse")) {
                MusicSource.pitch = 0;
                PouseCanvas.SetActive(true);
                canPlay = false;
            }
        } else {

            switch (currentPouseStatus) {
                case PouseStatus.Retry:
                    GameMaster.SetColors(PouseButton, (int)currentPouseStatus);
                    currentPouseStatus = GameMaster.HorizontalSelect<PouseStatus>(currentPouseStatus, PouseStatus.MusicSelect, PouseStatus.Resume);
                    if (Input.GetButtonDown("Return")) {
                        GameMaster.SceneChanger(SceneName.Main);
                    }
                    break;
                case PouseStatus.Resume:
                    GameMaster.SetColors(PouseButton, (int)currentPouseStatus);
                    currentPouseStatus = GameMaster.HorizontalSelect<PouseStatus>(currentPouseStatus, PouseStatus.Retry, PouseStatus.MusicSelect);
                    if (Input.GetButtonDown("Return")) {
                        PouseCanvas.SetActive(false);
                        MusicSource.pitch = 1;
                        canPlay = true;
                    }
                    break;
                case PouseStatus.MusicSelect:
                    GameMaster.SetColors(PouseButton, (int)currentPouseStatus);
                    currentPouseStatus = GameMaster.HorizontalSelect<PouseStatus>(currentPouseStatus,PouseStatus.Resume,PouseStatus.Retry);
                    if (Input.GetButtonDown("Return")) {
                        GameMaster.SceneChanger(SceneName.MusicSelect);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private float ComboBonus(int _combo) {
        int x = _combo / 100;
        if(x != 0) {
            comboBonusText.gameObject.SetActive(true);
            comboBonusText.text = "× 1." + x.ToString(); 
        }
        return 1 + (x * 0.1f);
    }

    public Vector3 GetJudgeLinePositon(int _index) {
        return judgeLineObj[_index].transform.position;
    }
}