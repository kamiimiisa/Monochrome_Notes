using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Config;
using System.IO;

public class GameMaster : SingletonMonoBehaviour<GameMaster> {

    private static string musicName = "tutorial";
    public static string MusicName {
        get { return musicName; }
        set { musicName = value; }
    }

    private static string musicLevel = "Hard";
    public static string MusicLevel {
        get { return musicLevel; }
        set { musicLevel = value; }
    }

    private static int score;
    public static int Score {
        get { return score; }
        set { score = value; }
    }

    private static int maxCombo;
    public static int MaxCombo {
        get { return maxCombo; }
        set { maxCombo = value; }
    }

    private static int parfectNum;
    public static int ParfectNum {
        get { return parfectNum; }
        set { parfectNum = value; }
    }

    private static int greatNum;
    public static int GreatNum {
        get { return greatNum; }
        set { greatNum = value; }
    }

    private static int missNum;
    public static int MissNum {
        get { return missNum; }
        set { missNum = value; }
    }

    private static float noteSpeed = 7f;
    public static float NoteSpeed {
        get { return noteSpeed; }
        set { noteSpeed = value; }
    }

    private static int ajust = 0;
    public static int Ajust {
        get { return ajust; }
        set { ajust = value; }
    }

    private static int musicVolume = 5;
    public static int MusicVolume {
        get { return musicVolume; }
        set { musicVolume = value; }
    }

    private static int seVolume = 3;
    public static int SEVolume {
        get { return seVolume; }
        set { seVolume = value; }
    }

    private static float deltaTime;
    public static float DeltaTime {
        get { return deltaTime; }
    }

    private static Character character;
    public static Character Character {
        get { return character; }
        set { character = value; }
    }

    private static bool hideGameMode = false;
    public static bool HideGameMode {
        get { return hideGameMode; }
        set { hideGameMode = value; }
    }

    private int currentJoysticKey = 0;
    private static ControlMode controlMode = ControlMode.KeyBoard;
    public static ControlMode ControlMode {
        get { return controlMode; }
    }


    // Use this for initialization
    void Start() {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update() {
        deltaTime = Time.deltaTime;
        ControlModeChanger();
    }

    public void Awake() {
        if (this != Instance) {
            Destroy(gameObject);
            return;
        } else {

        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// enum SceneNameに記載されたシーンにのみ移る
    /// </summary>
    /// <param name="_sceneName"></param>
    public static void SceneChanger(SceneName _sceneName) {
        switch (_sceneName) {
            case SceneName.Title:
            case SceneName.Load:
                SceneManager.LoadScene(_sceneName.ToString());
                break;

            case SceneName.Exit:
                Application.Quit();
                break;

            default:
                LoadManager.NextScene = _sceneName;
                SceneChanger(SceneName.Load);
                break;
        }
    }



    public static void SetColors(Image[] _images, int key) {
        foreach (var _item in _images) {
            _item.color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
        }

        _images[key].color = new Color(0.38f, 0.38f, 0.38f, 1);
    }


    public static MyInput myButton = new MyInput();
    public static float interval = 0;
    public static E VarticalSelect<E>(E e, E _before, E _next) {
        if (myButton.GetButtonDown("Button2_Vartical")) {
            if (Input.GetAxisRaw("Button2_Vartical") > 0) {
                e = _before;
            }
            if (Input.GetAxisRaw("Button2_Vartical") < 0) {
                e = _next;
            }
        }

        if (Input.GetAxisRaw("Button2_Vartical") != 0) {
            interval += DeltaTime;
            if (interval > 0.3f) {
                if (Input.GetAxisRaw("Button2_Vartical") > 0) {
                    e = _before;
                }
                if (Input.GetAxisRaw("Button2_Vartical") < 0) {
                    e = _next;
                }
                interval = 0;
            }
        } else {
            if (Input.GetAxisRaw("Button2_Horizontal") == 0) {
                interval = 0;
            }
        }
        return e;
    }

    public static E HorizontalSelect<E>(E e, E _before, E _next) {
        if (myButton.GetButtonDown("Button2_Horizontal")) {
            if (Input.GetAxisRaw("Button2_Horizontal") < 0) {
                e = _before;
            }
            if (Input.GetAxisRaw("Button2_Horizontal") > 0) {
                e = _next;
            }
        }

        if (Input.GetAxisRaw("Button2_Horizontal") != 0) {
            interval += DeltaTime;
            if (interval > 0.3f) {
                if (Input.GetAxisRaw("Button2_Horizontal") < 0) {
                    e = _before;
                }
                if (Input.GetAxisRaw("Button2_Horizontal") > 0) {
                    e = _next;
                }
                interval = 0;
            }
        } else {
            if (Input.GetAxisRaw("Button2_Vartical") == 0) {
                interval = 0;
            }
        }
        return e;
    }

    static ControlMode beforeMode = ControlMode;
    public static void DefoltUiSet(GameObject[] _keyboard, GameObject[] _joystick) {
        switch (ControlMode) {
            case ControlMode.KeyBoard:
                foreach (var item in _keyboard) {
                    item.SetActive(true);
                }
                foreach (var item in _joystick) {
                    item.SetActive(false);
                }
                break;
            case ControlMode.JoyStick:
                foreach (var item in _joystick) {
                    item.SetActive(true);
                }
                foreach (var item in _keyboard) {
                    item.SetActive(false);
                }
                break;
        }
        beforeMode = ControlMode;
    }

    public static void UiChanger(GameObject[] _keyboard, GameObject[] _joystick) {
        if (beforeMode != ControlMode) {
            switch (ControlMode) {
                case ControlMode.KeyBoard:
                    foreach (var item in _keyboard) {
                        item.SetActive(true);
                    }
                    foreach (var item in _joystick) {
                        item.SetActive(false);
                    }
                    break;
                case ControlMode.JoyStick:
                    foreach (var item in _joystick) {
                        item.SetActive(true);
                    }
                    foreach (var item in _keyboard) {
                        item.SetActive(false);
                    }
                    break;
            }
            beforeMode = ControlMode;
        }
    }

    private void ControlModeChanger() {
        if (Input.anyKeyDown) {
            currentJoysticKey = 0;
            for (int i = currentJoysticKey; i < 20; i++) {
                if (Input.GetKeyDown("joystick 1 button " + i)) {
                    controlMode = ControlMode.JoyStick;
                    currentJoysticKey = i;
                    break;
                }
            }
            if (!Input.GetKeyDown("joystick 1 button " + currentJoysticKey)) {
                controlMode = ControlMode.KeyBoard;
            }

        }
    }

    public static void UiOff(GameObject[] _keyboard, GameObject[] _joystick) {
        foreach (var item in _joystick) {
            item.SetActive(false);
        }

        foreach (var item in _keyboard) {
            item.SetActive(false);
        }
    }
}
