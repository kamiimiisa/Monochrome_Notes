using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Config;

public class MusicSelectManager : MonoBehaviour {


    [SerializeField] private Animator musicAnimator;
    [SerializeField] private Animator SettingAnimator;
    [SerializeField] private Image[] jackets;
    [SerializeField] private Image[] jacketsBackGround;
    private static int musicNum = 0;
    [SerializeField] private MusicState[] musicState;
    [SerializeField] private Text selectText;
    [SerializeField] private Text[] musicName;
    [SerializeField] private GameObject levelUI_Obj;
    [SerializeField] private Color[] levelColor;
    [SerializeField] private Text levelText;

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
    private Dictionary<Level, Color> LEVEL_COROL = new Dictionary<Level, Color>();
    private static Level currentLevel = Level.NORMAL;
    private float animationInterval = 0f;
    private const float MAX_ANIMATION_INTERVAL = 0.3f;

    private Character_A characterA = new Character_A();
    private Character_B characterB = new Character_B();
    private Character currentCharacter;

    private MyInput Button2 = new MyInput();

    [SerializeField] private GameObject[] keyboradButton = null, joystickButton = null;

    private AudioSource demoAudioSource;

    // Use this for initialization
    void Start () {
        GameMaster.DefoltUiSet(keyboradButton, joystickButton);
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
        selectText.text = "MusicSelect";
        levelText.text = currentLevel.ToString();

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
        LEVEL_COROL.Add(Level.EASY, levelColor[0]);
        LEVEL_COROL.Add(Level.NORMAL, levelColor[1]);
        LEVEL_COROL.Add(Level.HARD, levelColor[2]);

        //難易度に合わせた背景色の変更の初期化
        foreach (var _bg in jacketsBackGround) {
            _bg.color = LEVEL_COROL[currentLevel];
        }

        demoAudioSource = GetComponent<AudioSource>();
        ChangeDemoMusic(musicState[musicNum].DemoMusic);
    }

    void Update() {
        GameMaster.UiChanger(keyboradButton, joystickButton);
        DemoPlay();

        switch (currentMode) {
            case ModeSelect.Music:
                //↑↓で曲の難易度を変更する
                if (Button2.GetButtonDown("Button2_Vartical")) {
                    if (Input.GetAxisRaw("Button2_Vartical") < 0) {
                        LevelChange(Direction.Down);
                    }

                    if (Input.GetAxisRaw("Button2_Vartical") > 0) {
                        LevelChange(Direction.Up);
                    }
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
                jackets[(int)JacketIndex.OutSide].sprite = musicState[musicNum].NextMusic.NextMusic.NextMusic.Jacket;
                break;
            case Direction.Right:
                musicNum++;
                if (musicNum > musicState.Length-1) {
                    musicNum = 0;
                }
                if (musicNum < 0) {
                    musicNum = musicState.Length-1;
                }
                jackets[(int)JacketIndex.OutSide].sprite = musicState[musicNum].BeforeMusic.BeforeMusic.BeforeMusic.Jacket;
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

        ChangeDemoMusic(musicState[musicNum].DemoMusic);
    }

    private void LevelChange(Direction _directhon) {
        switch (currentLevel) {
            case Level.EASY:
                switch (_directhon) {
                    case Direction.Down:
                        currentLevel = Level.EASY;
                        break;
                    case Direction.Up:
                        currentLevel = Level.NORMAL;
                        break;
                }
                break;
            case Level.NORMAL:
                switch (_directhon) {
                    case Direction.Down:
                        currentLevel = Level.EASY;
                        break;
                    case Direction.Up:
                        currentLevel = Level.HARD;
                        break;
                }
                break;
            case Level.HARD:
                switch (_directhon) {
                    case Direction.Down:
                        currentLevel = Level.NORMAL;
                        break;
                    case Direction.Up:
                        currentLevel = Level.HARD;
                        break;
                }
                break;
            default:
                break;
        }
        

        foreach (var _bg in jacketsBackGround) {
            _bg.color = LEVEL_COROL[currentLevel];
        }

        levelText.text = currentLevel.ToString();
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
                demoAudioSource.volume = musicVolumeSlider.value * 0.1f;
                break;
            case SettingSelect.SEVolume:
                seVolumeSlider.value += (int)Input.GetAxisRaw("Button2_Horizontal");
                break;
            default:
                break;
        }
    }
    
    private void DemoPlay() {
        if (!demoAudioSource.isPlaying) {
            demoAudioSource.Play();
        }
    }

    private void ChangeDemoMusic(AudioClip _demoClip) {
        demoAudioSource.clip = _demoClip;
    }
}
