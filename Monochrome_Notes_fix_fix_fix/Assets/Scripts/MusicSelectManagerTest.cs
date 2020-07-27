using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Config;

public class MusicSelectManagerTest : MonoBehaviour {


    [SerializeField] private Animator musicAnimator;
    [SerializeField] private Animator SettingAnimator;
    [SerializeField] private Image[] jackets;
    [SerializeField] private Image[] jacketsBackGround;
    private static int musicNum = 0;
    [SerializeField] private MusicState[] musicState;
    [SerializeField] private Text selectText;
    [SerializeField] private Text[] musicName;
    [SerializeField] private Text[] levelNum;
    [SerializeField] private Text[] levelText;
    [SerializeField] private GameObject levelUI_Obj;
    [SerializeField] private GameObject characterUI_Obj;
    [SerializeField] private GameObject[] character_A_select;
    [SerializeField] private GameObject[] character_B_select;
    [SerializeField] private GameObject rootMusicSelect;
    [SerializeField] private GameObject rootCharacterSelect;
    [SerializeField] private GameObject conflmWindow;
    [SerializeField] private Sprite Character_A_Sprite;
    [SerializeField] private Sprite Character_B_Sprite;
    [SerializeField] private GameObject cursor;
    [SerializeField] private Slider notesSpeedSlider;
    [SerializeField] private Slider ajustSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;
    private AudioClip audioClip;
    private enum JacketIndex {
        LeftEnd,
        Left,
        Center,
        Right,
        RightEnd,
        OutSide,
        Next,
    }
    private enum Direction {
        Left,
        Right,
        Up,
        Down
    }
    private enum ModeSelect {
        Music,
        Character,
        Confrim,
        Settings,
    }
    private ModeSelect currentMode = ModeSelect.Music;
    private ModeSelect beforeMode = ModeSelect.Settings;
    private enum SettingSelect {
        NotesSpeed,
        TimingAjust,
        MusicVolume,
        SEVolume,
    }
    private static SettingSelect currentSetting = SettingSelect.NotesSpeed;
    private float settingInterval = 0;
    private Dictionary<Level, Color> LevelColor = new Dictionary<Level, Color>
    {
        {Level.EASY,Color.blue },
        {Level.NORMAL,Color.yellow },
        {Level.HARD,Color.red },
    };
    private static Level currentLevel = Level.NORMAL;
    private float animationInterval = 0f;
    private const float MAX_ANIMATION_INTERVAL = 0.3f;

    private Character_A characterA = new Character_A();
    private Character_B characterB = new Character_B();
    private Character currentCharacter;

    private MyInput Button2 = new MyInput();

    [SerializeField] private GameObject[] keyboradButton, joystickButton;

    // Use this for initialization
    void Start () {
        GameMaster.UiChangerConst(keyboradButton, joystickButton);
        for (int i = 0; i < musicState.Length; i++) {
            if (i + 1 > musicState.Length - 1) {
                musicState[i].NextMusic = musicState[0];
            } else {
                musicState[i].NextMusic = musicState[i + 1];
            }
            if (i - 1 < 0) {
                musicState[i].BeforeMusic = musicState[musicState.Length - 1];
            } else {
                musicState[i].BeforeMusic = musicState[i - 1];
            }
        }


        jackets[(int)JacketIndex.LeftEnd].sprite = musicState[musicNum].BeforeMusic.BeforeMusic.Jacket;
        jackets[(int)JacketIndex.Left].sprite = musicState[musicNum].BeforeMusic.Jacket;
        jackets[(int)JacketIndex.Center].sprite = musicState[musicNum].Jacket;
        jackets[(int)JacketIndex.Right].sprite = musicState[musicNum].NextMusic.Jacket;
        jackets[(int)JacketIndex.RightEnd].sprite = musicState[musicNum].NextMusic.NextMusic.Jacket;
        musicName[(int)currentMode].text = musicState[musicNum].name;
        levelNum[(int)currentMode].text = musicState[musicNum].GetLevelNum(currentLevel).ToString();
        levelText[(int)currentMode].text = currentLevel.ToString();
        selectText.text = "MusicSelect";
        

        //デフォルトで上のキャラを選んぶ処理
        currentCharacter = characterA;
        CharacterChange(Direction.Up);

        //キャラの絵を格納する
        characterA.CharacterSprite = Character_A_Sprite;
        characterB.CharacterSprite = Character_B_Sprite;

        //スライダーの設定をGameMasterから読み取る
        notesSpeedSlider.value = GameMaster.NoteSpeed;
        ajustSlider.value = GameMaster.Ajust;
        musicVolumeSlider.value = GameMaster.MusicVolume;
        seVolumeSlider.value = GameMaster.SEVolume;

        //シーン読み込み時のみ設定のカーソルを初期化する
        currentSetting = SettingSelect.NotesSpeed;

        //ゴリラ　UIの初期化
        levelUI_Obj.SetActive(true);
        characterUI_Obj.SetActive(false);

        //難易度に合わせた背景色の変更の初期化
        foreach (var _bg in jacketsBackGround) {
            _bg.color = LevelColor[currentLevel];
        }
    }

    void Update() {
        GameMaster.UiChanger(keyboradButton, joystickButton);

        switch (currentMode) {
            case ModeSelect.Music:
                //L1R1(おまけでL2R2)で曲の難易度を変更する
                if (Input.GetButtonDown("Button1")) {
                    LevelChange(Direction.Left);
                }
                if (Input.GetButtonDown("Button4")) {
                    LevelChange(Direction.Right);
                }

                //左右入力で曲を変更する
                animationInterval += Time.deltaTime;
                if (animationInterval > MAX_ANIMATION_INTERVAL && musicAnimator.GetCurrentAnimatorStateInfo(0).IsName("wait")) {
                    if (Input.GetAxisRaw("Button2_Horizontal") < 0) {
                        musicAnimator.SetTrigger("LeftTrigger");
                        MusicChange(Direction.Left);
                        animationInterval = 0;
                    }
                    if (Input.GetAxisRaw("Button2_Horizontal") > 0) {
                        musicAnimator.SetTrigger("RightTrigger");
                        MusicChange(Direction.Right);
                        animationInterval = 0;
                    }
                }

                if (Input.GetButtonDown("Pouse")) {
                    beforeMode = currentMode;
                    selectText.text = "Settings";
                    SettingAnimator.SetBool("CanSetting", true);
                    currentMode = ModeSelect.Settings;
                }

                if (Input.GetButtonDown("Return")) {
                    currentMode = ModeSelect.Character;
                    selectText.text = "CharacterSelect";
                    jackets[(int)JacketIndex.Next].sprite = musicState[musicNum].Jacket;

                    musicName[(int)currentMode].text = musicState[musicNum].name;
                    levelNum[(int)currentMode].text = musicState[musicNum].GetLevelNum(currentLevel).ToString();
                    levelText[(int)currentMode].text = currentLevel.ToString();
                    rootCharacterSelect.SetActive(true);
                    rootMusicSelect.SetActive(false);
                    levelUI_Obj.SetActive(false);
                    characterUI_Obj.SetActive(true);
                }

                if (Input.GetButtonDown("Cancel")) {
                    GameMaster.SceneChanger(SceneName.Title);
                }
                break;
            case ModeSelect.Character:
                if (Input.GetAxisRaw("Button2_Vartical") > 0) {
                    CharacterChange(Direction.Up);
                }
                if (Input.GetAxisRaw("Button2_Vartical") < 0) {
                    CharacterChange(Direction.Down);
                }

                if (Input.GetButtonDown("Return")) {
                    currentMode = ModeSelect.Confrim;
                    conflmWindow.SetActive(true);
                    levelUI_Obj.SetActive(false);
                    characterUI_Obj.SetActive(false);
                }
                if (Input.GetButtonDown("Cancel")) {
                    currentMode = ModeSelect.Music;
                    selectText.text = "MusicSelect";
                    rootCharacterSelect.SetActive(false);
                    rootMusicSelect.SetActive(true);
                    levelUI_Obj.SetActive(true);
                    characterUI_Obj.SetActive(false);
                }

                if (Input.GetButtonDown("Pouse")) {
                    beforeMode = currentMode;
                    selectText.text = "Settings";
                    SettingAnimator.SetBool("CanSetting", true);
                    currentMode = ModeSelect.Settings;
                }

                break;

            case ModeSelect.Confrim:
                if (Input.GetButtonDown("Return")) {
                    GameMaster.MusicName = musicState[musicNum].name;
                    GameMaster.MusicLevel = currentLevel.ToString();
                    GameMaster.Character = currentCharacter;
                    GameMaster.NoteSpeed = notesSpeedSlider.value;
                    GameMaster.Ajust = (int)ajustSlider.value;
                    GameMaster.MusicVolume = (int)musicVolumeSlider.value;
                    GameMaster.SEVolume = (int)seVolumeSlider.value;
                    GameMaster.SceneChanger(SceneName.Main);
                }

                if (Input.GetButtonDown("Cancel")) {
                    currentMode = ModeSelect.Character;
                    conflmWindow.SetActive(false);
                    levelUI_Obj.SetActive(false);
                    characterUI_Obj.SetActive(true);
                }
                break;
            case ModeSelect.Settings:

                switch (currentSetting) {
                    case SettingSelect.NotesSpeed:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.SEVolume, SettingSelect.TimingAjust);
                        cursor.transform.position = notesSpeedSlider.gameObject.transform.position;
                        break;
                    case SettingSelect.TimingAjust:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.NotesSpeed, SettingSelect.MusicVolume);
                        cursor.transform.position = ajustSlider.gameObject.transform.position;
                        break;
                    case SettingSelect.MusicVolume:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.TimingAjust, SettingSelect.SEVolume);
                        cursor.transform.position = musicVolumeSlider.gameObject.transform.position;
                        break;
                    case SettingSelect.SEVolume:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.MusicVolume, SettingSelect.NotesSpeed);
                        cursor.transform.position = seVolumeSlider.gameObject.transform.position;
                        break;
                }

                //ボタン短押し
                if (Button2.GetButtonDown("Button2_Horizontal")) {
                    SliderChanger();
                }
                //ボタン長押し
                if (Input.GetAxisRaw("Button2_Horizontal") != 0) {

                    settingInterval += Time.deltaTime;
                    if (settingInterval > 0.15f) {
                        SliderChanger();
                        settingInterval = 0;
                    }
                } else {
                    settingInterval = 0;
                }

                if (Input.GetButtonDown("Return")) {
                    switch (beforeMode) {
                        case ModeSelect.Music:
                            selectText.text = "MusicSelect";
                            break;
                        case ModeSelect.Character:
                            selectText.text = "CharacterSelect";
                            break;
                    }
                    SettingAnimator.SetBool("CanSetting", false);
                    currentMode = beforeMode;
                }

                if (Input.GetButtonDown("Cancel")) {
                    switch (beforeMode) {
                        case ModeSelect.Music:
                            selectText.text = "MusicSelect";
                            break;
                        case ModeSelect.Character:
                            selectText.text = "CharacterSelect";
                            break;
                    }
                    SettingAnimator.SetBool("CanSetting", false);
                    currentMode = beforeMode;
                }
                break;
        }


    }


    private void MusicChange(Direction _direction) {
        switch (_direction) {
            case Direction.Left:
                musicNum--;
                if (musicNum > musicState.Length-1) {
                    musicNum = 0;
                }
                if (musicNum < 0) {
                    musicNum = musicState.Length-1;
                }
                jackets[(int)JacketIndex.OutSide].sprite = musicState[musicNum].BeforeMusic.BeforeMusic.BeforeMusic.Jacket;
                break;
            case Direction.Right:
                musicNum++;
                if (musicNum > musicState.Length-1) {
                    musicNum = 0;
                }
                if (musicNum < 0) {
                    musicNum = musicState.Length-1;
                }
                jackets[(int)JacketIndex.OutSide].sprite = musicState[musicNum].NextMusic.NextMusic.NextMusic.Jacket;
                break;
            default:
                break;
        }
        jackets[(int)JacketIndex.LeftEnd].sprite = musicState[musicNum].BeforeMusic.BeforeMusic.Jacket;
        jackets[(int)JacketIndex.Left].sprite = musicState[musicNum].BeforeMusic.Jacket;
        jackets[(int)JacketIndex.Center].sprite = musicState[musicNum].Jacket;
        jackets[(int)JacketIndex.Right].sprite = musicState[musicNum].NextMusic.Jacket;
        jackets[(int)JacketIndex.RightEnd].sprite = musicState[musicNum].NextMusic.NextMusic.Jacket;
        musicName[(int)currentMode].text = musicState[musicNum].name;
        levelNum[(int)currentMode].text = musicState[musicNum].GetLevelNum(currentLevel).ToString();
    }

    private void LevelChange(Direction _directhon) {
        switch (currentLevel) {
            case Level.EASY:
                switch (_directhon) {
                    case Direction.Left:
                        currentLevel = Level.EASY;
                        break;
                    case Direction.Right:
                        currentLevel = Level.NORMAL;
                        break;
                }
                break;
            case Level.NORMAL:
                switch (_directhon) {
                    case Direction.Left:
                        currentLevel = Level.EASY;
                        break;
                    case Direction.Right:
                        currentLevel = Level.HARD;
                        break;
                }
                break;
            case Level.HARD:
                switch (_directhon) {
                    case Direction.Left:
                        currentLevel = Level.NORMAL;
                        break;
                    case Direction.Right:
                        currentLevel = Level.HARD;
                        break;
                }
                break;
            default:
                break;
        }
        
        levelNum[(int)currentMode].text = musicState[musicNum].GetLevelNum(currentLevel).ToString();
        levelText[(int)currentMode].text = currentLevel.ToString();

        foreach (var _bg in jacketsBackGround) {
            _bg.color = LevelColor[currentLevel];
        }
    }

    private void CharacterChange(Direction _direction) {
        switch (_direction) {
            case Direction.Up:
                foreach (var _obj in character_A_select) {
                    _obj.SetActive(true);
                }
                foreach (var _obj in character_B_select) {
                    _obj.SetActive(false);
                }

                currentCharacter = characterA;
                break;
            case Direction.Down:
                foreach (var _obj in character_A_select) {
                    _obj.SetActive(false);
                }
                foreach (var _obj in character_B_select) {
                    _obj.SetActive(true);
                }


                currentCharacter = characterB;
                break;
            default:
                break;
        }
    }

    private void SliderChanger() {
        switch (currentSetting) {
            case SettingSelect.NotesSpeed:
                notesSpeedSlider.value += 0.5f * (int)Input.GetAxisRaw("Button2_Horizontal");
                break;
            case SettingSelect.TimingAjust:
                ajustSlider.value += (int)Input.GetAxisRaw("Button2_Horizontal");
                break;
            case SettingSelect.MusicVolume:
                musicVolumeSlider.value += (int)Input.GetAxisRaw("Button2_Horizontal");
                break;
            case SettingSelect.SEVolume:
                seVolumeSlider.value += (int)Input.GetAxisRaw("Button2_Horizontal");
                break;
            default:
                break;
        }
    }
}
