using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using NoteEditor.DTO;
using Config;
using Monochrome_Notes;


public class MainManager : MonoBehaviour {
    [SerializeField] [Range(1, 20)] private int noteSpeed = 5;  
    private string musicName;
    private string musicLevel;
    private GameState state = GameState.Awake;
    private enum GameState{
        Awake,
        Start,
        NotesUpdate,
        PouseIn,
        PouseRetry,
        PouseResume,
        PouseMuscSelect,
        PouseWait,
        PouseOut,
        ResultIn,
        ResultWait,
        ResultRetry,
        ResultMusicSelect,
        ResultTitle,
    }
    private enum Tile{
        Left=0,
        Center = 1,
        Right = 2,
    }

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

    private float resultInterval = 0;
    private const float MAX_RESULT_INTERVAL = 1;

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
        {Judge.Hold,0.1f},
        {Judge.ExTap,0.1f},
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

    private enum ExTapColor {
        Red,
        Orange,
        Yellow,
        Green,
        Cyan,
        Purple,
    }
    private ExTapColor currentColor = ExTapColor.Red;
    private Dictionary<ExTapColor, Color> EX_COLOR = new Dictionary<ExTapColor, Color>()
    {
        {ExTapColor.Red, Color.red},
        {ExTapColor.Orange, new Color(1,0.5f,0)},
        {ExTapColor.Yellow, Color.yellow},
        {ExTapColor.Green, Color.green},
        {ExTapColor.Cyan, Color.cyan},
        {ExTapColor.Purple, new Color(1,0,1)},
    };

    private enum Key {
        Main,Sub
    }


    [SerializeField] private List<AudioClip> bgmList;
    [SerializeField] private List<AudioClip> seList;
    private AudioSource musicSource;
    private float currentMusicTime;
    private float ajustFsTime = 0.0f;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource fsSource;

    [SerializeField] private TextMesh musicNameTextMesh;

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


    [SerializeField] private GameObject pouseCanvas;
    [SerializeField] private GameObject endingText;


    [SerializeField] private Image[] Nexts = new Image[2];
    [SerializeField] private Image[] PouseButton;

    [SerializeField] private Animator comboAnimator;
    [SerializeField] private Animator feadInAnimator;

    private void Awake() {
        NOTES_LIST = new List<Note.NotePos>();
        LINE_NOTE_LIST = new Dictionary<Line, List<Note>>();
        CURRENT_NOTES = new Dictionary<Line, int>();
        LINE_POSITION = new Dictionary<Line, float>();

        JUDGE_RANGE = new Dictionary<Judge, float>()
        {
            {Judge.Pafect, 0.075f},
            {Judge.Graet, 0.1f},
            {Judge.Miss, 0.2f},
            {Judge.HoldStart,0.1f},
            {Judge.Hold,0.1f},
            {Judge.ExTap,0.1f},
        };

        JUDGE_SCORE = new Dictionary<Judge, int>()
        {
            {Judge.Pafect,1000},
            {Judge.Graet,500},
            {Judge.Miss,0},
            {Judge.HoldStart,500},
            {Judge.Hold,50},
            {Judge.HoldEnd,500},
            {Judge.ExTap,2000},
        };

                noteSpeed = (int)GameMaster.NoteSpeed * 3;
        musicName = GameMaster.MusicName;
        musicLevel = GameMaster.MusicLevel;
        safeNum = 0;

        //Jsonから譜面をもらってきてEditDate,Noteクラスとして復元
        string json = Resources.Load<TextAsset>(musicLevel + "/" + musicName).ToString();
        editData = JsonUtility.FromJson<MusicDTO.EditData>(json);
        musicNote = JsonUtility.FromJson<MusicDTO.Note>(json);

        musicSource = GetComponent<AudioSource>();
        //audioSource.clipにmusicNameと同じ名前のものを渡す
        musicSource.clip = bgmList.Find(bgm => bgm.name == musicName);

        //四つ打ちを決める処理
        switch (editData.BPM) {
            case 145:
                fsSource.clip = seList[1];
                break;
            case 160:
                fsSource.clip = seList[2];
                break;
            default:
                fsSource.clip = seList[1];
                break;
        }

        //設定した音量を反映する
        musicSource.volume = GameMaster.MusicVolume * 0.1f;
        fsSource.volume = GameMaster.MusicVolume * 0.1f;


        //判定ラインの座標を記憶する
        foreach (Line line in Enum.GetValues(typeof(Line))) {
            LINE_POSITION.Add(line, judgeLineObj[(int)line].transform.position.x);
        }

        //曲名の反映
        musicNameTextMesh.text = musicLevel.ToString() + " : " + musicName;
        resultMusicName.text = musicLevel.ToString() + " : " + musicName;

        //キャラクターの反映
        character = GameMaster.Character;
        resultCharacterImage.GetComponent<Image>().sprite = character.CharacterSprite;
        character.Skill();

        //設定した音量を反映する
        seSource.volume = GameMaster.SEVolume * 0.1f;

        GameMaster.DefoltUiSet(keyboradButton, joystickButton);
        if (GameMaster.HideGameMode) {
            GameMaster.UiOff(keyboradButton, joystickButton);
        }
        DataFormat();
        Initialize();
        NotesUpdate();
    }

    // Use this for initialization
    void Start() {




    }

    // Update is called once per frame
    void Update() {


        switch (state) {
            case GameState.Awake:
                if (feadInAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
                    //よつうちの再生
                    fsSource.Play();
                    state = GameState.Start;
                }
                break;

            case GameState.Start:

                //四つ打ちが終わったら
                if (fsSource.time == 0.0f && !fsSource.isPlaying) {
                    ajustFsTime = fsSource.clip.length;
                    state = GameState.NotesUpdate;
                } else {
                    ajustFsTime = fsSource.time;
                }


                NotesUpdate();
                Judges();

                break;
            case GameState.NotesUpdate:
                //ノーツを移動させる関数
                NotesUpdate();
                Judges();
                MusicPlay();
                JudgeNotesMiss();
                ComboTextUpdate();

                

                

                //pouse画面の表示
                if (Input.GetButtonDown("Pouse")) {
                    state = GameState.PouseIn;
                }

                //全てのLineにNortsが残っていないかつMissを出していない場合
                if (ChackLastNorts() && missNum == 0) {

                    //All Colors !の場合
                    if (greatNum == 0) {
                        endingText.GetComponent<Text>().text = "<color=#ff0000>A</color><color=#ff8000>l</color><color=#ffff00>l</color>_<color=#009944>" +
                            "C</color><color=#0068B7>o</color><color=#1D2088>l</color><color=#ff0000>o</color><color=#ff8000>r</color>" +
                            "<color=#ffff00>s</color>_<color=#009944>!</color><color=#0068B7>!</color>";
                        endingText.GetComponent<Text>().color = new Color(0, 0, 0, 0);
                    }

                    endingText.SetActive(true);
                }


                //曲が終わったらリザルトに移る
                if (musicSource.time == 0.0f && !musicSource.isPlaying) {
                    state = GameState.ResultIn;
                }

                break;




            case GameState.PouseIn:
                musicSource.pitch = 0;
                pouseCanvas.SetActive(true);
                state = GameState.PouseResume;
                break;

            //ここからパネル選択処理
            case GameState.PouseRetry:
                GameMaster.SetColors(PouseButton, (int)Tile.Left);
                state = GameMaster.HorizontalSelect<GameState>(state,GameState.PouseMuscSelect, GameState.PouseResume);
                if (Input.GetButtonDown("Return")) {
                    GameMaster.SceneChanger(SceneName.Main);
                }
                break;
            case GameState.PouseResume:
                GameMaster.SetColors(PouseButton, (int)Tile.Center);
                state = GameMaster.HorizontalSelect<GameState>(state, GameState.PouseRetry, GameState.PouseMuscSelect);
                if (Input.GetButtonDown("Return")) {
                    state = GameState.PouseWait;
                }
                break;
            case GameState.PouseMuscSelect:
                GameMaster.SetColors(PouseButton, (int)Tile.Right);
                state = GameMaster.HorizontalSelect<GameState>(state, GameState.PouseResume, GameState.PouseRetry);
                if (Input.GetButtonDown("Return")) {
                    GameMaster.SceneChanger(SceneName.MusicSelect);
                }
                break;
            //ここまでパネル選択処理

            //ポーズから戻ったときに四つ打ちを再生する
            case GameState.PouseWait:
                pouseCanvas.SetActive(false);
                //よつうちの再生
                fsSource.Play();
                state = GameState.PouseOut;
                break;
            case GameState.PouseOut:
                //四つ打ちが終わったら
                if (fsSource.time == 0.0f && !fsSource.isPlaying) {
                    state = GameState.NotesUpdate;
                    musicSource.pitch = 1;
                }   
                break;


            //一回だけ処理してすぐにResultMusicSelectにうつる
            case GameState.ResultIn:
                //リザルトの反映
                string resultT = "C";
                float resultF = (float)(parfectNum + greatNum * 0.7f) / noteCount;
                if (resultF <= 0.7f) {
                    resultT = "B";
                } else if (resultF <= 0.85f) {
                    resultT = "A";
                } else if (resultF <= 0.90f) {
                    resultT = "S";
                } else if (resultF <= 0.95f) {
                    resultT = "SS";
                } else if (resultF < 1) {
                    resultT = "SSS";
                } else if (resultF == 1) {
                    resultT = "<color=#ff0000>A</color><color=#ff8000>l</color><color=#ffff00>l</color>_<color=#009944>C</color><color=#0068B7>o</color><color=#1D2088>l</color><color=#ff0000>o</color><color=#ff8000>r</color><color=#ffff00>s</color>_<color=#009944>!</color><color=#0068B7>!</color>";
                    testResult.color = new Color(0, 0, 0, 0);
                    fullComboText.text = "<color=#ff0000>A</color><color=#ff8000>l</color><color=#ffff00>l</color>_<color=#009944>C</color><color=#0068B7>o</color><color=#1D2088>l</color><color=#ff0000>o</color><color=#ff8000>r</color><color=#ffff00>s</color>_<color=#009944>!</color><color=#0068B7>!</color>";
                    fullComboText.color = new Color(0, 0, 0, 0);
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
                state = GameState.ResultWait;
                break;

            case GameState.ResultWait:
                parfectGauge.fillAmount = Mathf.MoveTowards(parfectGauge.fillAmount, (float)parfectNum / noteCount, 1 * GameMaster.DeltaTime);
                greatGauge.fillAmount = Mathf.MoveTowards(greatGauge.fillAmount, (float)greatNum / noteCount, 1 * GameMaster.DeltaTime);
                missGauge.fillAmount = Mathf.MoveTowards(missGauge.fillAmount, (float)missNum / noteCount, 1 * GameMaster.DeltaTime);

                resultInterval += GameMaster.DeltaTime;
                if (resultInterval >= MAX_RESULT_INTERVAL) {
                    state = GameState.ResultMusicSelect;
                }
                break;

            case GameState.ResultRetry:
                GameMaster.SetColors(Nexts, (int)Tile.Left);
                state = GameMaster.HorizontalSelect<GameState>(state, GameState.ResultTitle, GameState.ResultMusicSelect);
                if (Input.GetButtonDown("Return")) {
                    GameMaster.SceneChanger(SceneName.Main);
                }
                break;
            case GameState.ResultMusicSelect:
                GameMaster.SetColors(Nexts, (int)Tile.Center);
                state = GameMaster.HorizontalSelect<GameState>(state, GameState.ResultRetry,GameState.ResultTitle);
                if (Input.GetButtonDown("Return")) {
                    GameMaster.SceneChanger(SceneName.MusicSelect);
                }
                break;
            case GameState.ResultTitle:
                GameMaster.SetColors(Nexts, (int)Tile.Right);
                state = GameMaster.HorizontalSelect<GameState>(state, GameState.ResultMusicSelect, GameState.ResultRetry);
                if (Input.GetButtonDown("Return")) {
                    GameMaster.SceneChanger(SceneName.Title);
                }
                break;
            default:
                break;
        }
    }

    private void DataFormat() {
        float offset = (float)editData.offset / (float)musicSource.clip.frequency;
        float timingAdjust = (float)GameMaster.Ajust / 100 + 15 / 100;
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
                    for (int _num = _beforeNotes.num + 4; _num < _hold.num; _num += 4) {
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
            
            float spriteX = 2.3f;
            if (note.notePos.lineNum == Line.Line5 || note.notePos.lineNum == Line.Line6 || note.notePos.lineNum == Line.Line7) {
                spriteX = 4.8f;
            }
            if(note.notePos.lineNum == Line.Line8) {
                spriteX = 9.8f;
            }
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            var spritSize = spriteRenderer.size;
            spritSize.x = spriteX;
            spriteRenderer.size = spritSize;
            if(note.notePos.noteType == NoteType.ExTap) {
                spriteRenderer.color = EX_COLOR[currentColor];
                currentColor = CangeExTapColor(currentColor);
            }


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
    /// ノーツを更新する処理
    /// </summary>
    private void NotesUpdate() {
        foreach (var notes in LINE_NOTE_LIST.Values) {
            foreach (var note in notes) {
                var pos = note.transform.position;
                pos.z = judgeLineObj[(int)note.notePos.lineNum].transform.position.z + (note.notePos.notesTimeg - musicSource.time + fsSource.clip.length - ajustFsTime) * noteSpeed;
                note.transform.position = pos;
            }
        }
        
    }

    private void Judges() {
        if (GameMaster.HideGameMode) {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || 
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || 
                Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X) ) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line1);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line8);
            }

            if (Input.GetKeyDown(KeyCode.E) || 
                Input.GetKeyDown(KeyCode.D) || 
                Input.GetKeyDown(KeyCode.C)) {
                JudgeNotes(Line.Line1,Key.Sub);
                JudgeNotes(Line.Line2, Key.Sub);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line8);
            }

            if (Input.GetKeyDown(KeyCode.R) ||
                Input.GetKeyDown(KeyCode.F) ||
                Input.GetKeyDown(KeyCode.V)) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line2);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line8);
            }

            if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Y) ||
                Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.H) ||
                Input.GetKeyDown(KeyCode.B) ) {
                JudgeNotes(Line.Line2, Key.Sub);
                JudgeNotes(Line.Line3, Key.Sub);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }


            if (Input.GetKeyDown(KeyCode.U) || 
                Input.GetKeyDown(KeyCode.J) || 
                Input.GetKeyDown(KeyCode.N) ) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line3);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }

            if (Input.GetKeyDown(KeyCode.I) ||
                Input.GetKeyDown(KeyCode.K) ||
                Input.GetKeyDown(KeyCode.M)) {
                JudgeNotes(Line.Line3, Key.Sub);
                JudgeNotes(Line.Line4, Key.Sub);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }



            if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.P) ||
                Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.Semicolon) || Input.GetKeyDown(KeyCode.Colon) || 
                Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Space)) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line4);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.B)) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }

            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.E) ||
                Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.C)) {
                JudgeHoldNotes(Line.Line1);
                JudgeHoldNotes(Line.Line5);
                JudgeHoldNotes(Line.Line8);
            }
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.T) ||
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.G) ||
                Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.B)) {
                JudgeHoldNotes(Line.Line2);
                JudgeHoldNotes(Line.Line5);
                JudgeHoldNotes(Line.Line6);
                JudgeHoldNotes(Line.Line8);
            }
            if (Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.U) ||Input.GetKey(KeyCode.I) || 
                Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.J) ||Input.GetKey(KeyCode.K) || 
                Input.GetKey(KeyCode.N) || Input.GetKey(KeyCode.M)) {
                JudgeHoldNotes(Line.Line3);
                JudgeHoldNotes(Line.Line6);
                JudgeHoldNotes(Line.Line7);
                JudgeHoldNotes(Line.Line8);
            }
            if (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.O) || Input.GetKey(KeyCode.P) ||
                Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.Semicolon) || Input.GetKey(KeyCode.Colon) ||
                Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Comma) || Input.GetKey(KeyCode.Period) || Input.GetKey(KeyCode.Slash)) {
                JudgeHoldNotes(Line.Line4);
                JudgeHoldNotes(Line.Line7);
                JudgeHoldNotes(Line.Line8);
            }
        } else {
            if (Input.GetButtonDown("Button1") || Input.GetButtonDown("Button1_sub")) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line1);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line8);
            }
            if (Input.GetButtonDown("Button2") || Button2_Horizontal.GetButtonDown("Button2_Horizontal") || Button2_Vartical.GetButtonDown("Button2_Vartical")) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line2);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line8);
            }
            if (Input.GetButtonDown("Button3") || Input.GetButtonDown("Button3_sub") || Input.GetButtonDown("Button3_sub2")) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line3);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }
            if (Input.GetButtonDown("Button4") || Input.GetButtonDown("Button4_sub")) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line4);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }
            if (Input.GetButtonDown("Button5")) {
                seSource.PlayOneShot(seList[0]);
                JudgeNotes(Line.Line5);
                JudgeNotes(Line.Line6);
                JudgeNotes(Line.Line7);
                JudgeNotes(Line.Line8);
            }

            if (Input.GetButton("Button1") || Input.GetButton("Button1_sub")) {
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
            if (Input.GetButton("Button3") || Input.GetButton("Button3_sub") || Input.GetButton("Button3_sub2")) {
                JudgeHoldNotes(Line.Line3);
                JudgeHoldNotes(Line.Line6);
                JudgeHoldNotes(Line.Line7);
                JudgeHoldNotes(Line.Line8);
            }
            if (Input.GetButton("Button4") || Input.GetButton("Button4_sub")) {
                JudgeHoldNotes(Line.Line4);
                JudgeHoldNotes(Line.Line7);
                JudgeHoldNotes(Line.Line8);
            }
        }
    }
    /// <summary>
    /// タッチノーツの判定をとる関数
    /// </summary>
    /// <param name="_line"></param>
    void JudgeNotes(Line _line) {
        //LINEにもうノーツが無いときの空打ち
        if (CURRENT_NOTES[_line] < 0) {
            return;
        }
                
        //ノーツまで距離(時間)があるときの空打ち
        var note = LINE_NOTE_LIST[_line][CURRENT_NOTES[_line]];
        float diff = Mathf.Abs(fsSource.clip.length - ajustFsTime + musicSource.time - note.notePos.notesTimeg);
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
                if (diff <= JUDGE_RANGE[Judge.HoldStart]) {
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
            return;
        }

        score += JUDGE_SCORE[judge] * ComboBonus(combo);
        scoreText.text = "Score : " + ((int)score).ToString("D7");
        tapEffects[(int)_line].SetEffectData(JUDGE_COLOR[judge], 250);
        note.NotesEff(judgeEff, judgeLineObj[(int)_line].transform.position);

        if (judge != Judge.Miss) {
            ++combo;
        } else {
            if (safeNum > 0) {
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

    /// <summary>
    /// タッチノーツの判定をとる関数
    /// </summary>
    /// <param name="_line"></param>
    /// <param name="_key">誤動作防止に使う SubならGreat以下が出ない</param>
    void JudgeNotes(Line _line,Key _key) {
        //LINEにもうノーツが無いときの空打ち
        if (CURRENT_NOTES[_line] < 0) {
            return;
        }

        //ノーツまで距離(時間)があるときの空打ち
        var note = LINE_NOTE_LIST[_line][CURRENT_NOTES[_line]];
        float diff = Mathf.Abs(fsSource.clip.length - ajustFsTime + musicSource.time - note.notePos.notesTimeg);
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
                if (diff <= JUDGE_RANGE[Judge.HoldStart]) {
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
            return;
        }

        if(_key == Key.Sub && judge == Judge.Graet) {
            return;
        }

        score += JUDGE_SCORE[judge] * ComboBonus(combo);
        scoreText.text = "Score : " + ((int)score).ToString("D7");
        tapEffects[(int)_line].SetEffectData(JUDGE_COLOR[judge], 250);
        note.NotesEff(judgeEff, judgeLineObj[(int)_line].transform.position);

        if (judge != Judge.Miss) {
            ++combo;
        } else {
            if (safeNum > 0) {
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

        if (CURRENT_NOTES[_line] < 0) {
            return;
        }

        var _note = LINE_NOTE_LIST[_line][CURRENT_NOTES[_line]];
        float diff = Mathf.Abs(fsSource.clip.length - ajustFsTime + musicSource.time - _note.notePos.notesTimeg);
        if (diff > JUDGE_RANGE[Judge.Hold]) {
            return;
        }

        //TouchとHoldStartとBreak以外なら処理しない
        if (_note.notePos.noteType == NoteType.Touch ||_note.notePos.noteType == NoteType.ExTap || _note.notePos.noteType == NoteType.HoldStart) {
            return;
        }

        Judge judge = Judge.Hold;
        if (_note.notePos.noteType == NoteType.Hold || _note.notePos.noteType == NoteType.HoldEnd) {
            if (diff < JUDGE_RANGE[Judge.Hold]) {
                if (_note.notePos.noteType == NoteType.HoldEnd ) {
                    seSource.PlayOneShot(seList[0]);
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
            float diff = musicSource.time - note.notePos.notesTimeg;
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

        if (missNum == 0) {
            comboText.color = Color.cyan;
            if (greatNum == 0) {
                comboText.color = Color.yellow;
            }
        } else {
            comboText.color = Color.white;
        }
    }

    bool musicFlg = false;
    private void MusicPlay() {
        if (!musicFlg) {
            musicSource.Play();
            musicFlg = true;
        }
    }

    private void MusicStop() {
        musicFlg = false;
    }

    private float ComboBonus(int _combo) {
        int x = _combo / 100;
        if(x != 0) {
            comboBonusText.gameObject.SetActive(true);
            comboBonusText.text = "× 1." + x.ToString(); 
        }
        return 1 + (x * 0.1f);
    }
    
    private ExTapColor CangeExTapColor(ExTapColor _ex) {
        switch (_ex) {
            case ExTapColor.Red:
                _ex = ExTapColor.Orange;
                break;
            case ExTapColor.Orange:
                _ex = ExTapColor.Yellow;
                break;
            case ExTapColor.Yellow:
                _ex = ExTapColor.Green;
                break;
            case ExTapColor.Green:
                _ex = ExTapColor.Cyan;
                break;
            case ExTapColor.Cyan:
                _ex = ExTapColor.Purple;
                break;
            case ExTapColor.Purple:
                _ex = ExTapColor.Red;
                break;
        }

        return _ex;
    }

    private bool ChackLastNorts() {
        bool b = false;

        foreach (Line line in System.Enum.GetValues(typeof(Line))) {
            if(CURRENT_NOTES[line] != -1) {
                b = false;
                break;
            }

            b = true;
        }

        return b;
    }
}