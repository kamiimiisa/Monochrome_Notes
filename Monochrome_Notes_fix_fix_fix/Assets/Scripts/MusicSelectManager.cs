using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using UnityEngine.UI;
using System;

public class MusicSelectManager : MonoBehaviour {

    [SerializeField] private Animator charactorAnimator;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private Animator settingAnimator;

    private int selecter = 0;
    private float interval = 0;
    private MyInput Button2 = new MyInput();

    [SerializeField]
    private AudioClip[] musics;
    private AudioSource audioSource;

    [SerializeField] private Image[] musicUI;
    [SerializeField] private Image[] levelUI;
    [SerializeField] private Image[] SettingsUI;

    string charactor__Num = "Charactor_Num";
    string musicPanel = "Charactor→Music";
    string setting = "Music→Settings";

    private enum ModeSelect {
        Charactor,
        Music,
        Level,
        Settings,
    }

    private ModeSelect currentMode = ModeSelect.Charactor;

    private Slider notesSpeedSlider;
    private Slider ajustSlider;
    private Slider musicVolumeSlider;
    private Slider seVolumeSlider;

    private enum SettingSelect {
        NotesSpeed,
        TimingAjust,
        MusicVolume,
        SEVolume,
        MusicStart,
    }

    private SettingSelect currentSetting = SettingSelect.MusicStart;

    private enum LevelSelect {
        Esey,
        Normal,
        Hard,
    }

    private LevelSelect currentLevel = LevelSelect.Normal;

    private float[] sabiTIme = new float[]{44,16,80,0};
    private int[][] level = new int[][]
    {
        new int[] { 4,6,10 },
        new int[] { 1,2,3 },
        new int[] { 6,9,13 },
        new int[] { 1,2,3 },
    };
    
    // Use this for initialization
    void Start() {
        notesSpeedSlider = SettingsUI[0].GetComponentInChildren<Slider>();
        ajustSlider =SettingsUI[1].GetComponentInChildren<Slider>();
        musicVolumeSlider = SettingsUI[2].GetComponentInChildren<Slider>();
        seVolumeSlider = SettingsUI[3].GetComponentInChildren<Slider>();

        notesSpeedSlider.value = GameMaster.NoteSpeed;
        ajustSlider.value = GameMaster.Ajust;
        musicVolumeSlider.value = GameMaster.MusicVolume;
        seVolumeSlider.value = GameMaster.SEVolume;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {

        switch (currentMode) {
            //キャラクター選択
            case ModeSelect.Charactor:
                //キャラクターを選択する処理
                if (Button2.GetButtonDown("Button2_Horizontal")) {
                    if (Input.GetAxisRaw("Button2_Horizontal") < 0) {
                        charactorAnimator.SetInteger(charactor__Num, 0);
                    }
                    if (Input.GetAxisRaw("Button2_Horizontal") > 0) {
                        charactorAnimator.SetInteger(charactor__Num, 1);
                    }
                }

                //キャラクターを決定する処理
                if (Input.GetButtonDown("Return")) {
                    //キャラクターと曲選択両方のboolをTrueにする
                    charactorAnimator.SetBool(musicPanel, true);
                    panelAnimator.SetBool(musicPanel, true);
                    //曲選択へ進む
                    currentMode = ModeSelect.Music;
                    audioSource.clip = musics[selecter];
                    audioSource.time = sabiTIme[selecter];
                    audioSource.Play();
                }

                //タイトルに戻る
                if (Input.GetButtonDown("Cancel")) {
                    GameMaster.SceneChanger(SceneName.Title);
                }
                break;

            //曲選択
            case ModeSelect.Music:
                //キャラクター選択に戻る
                if (Input.GetButtonDown("Cancel")) {
                    //キャラクターと曲選択両方のboolをfalseにする
                    charactorAnimator.SetBool(musicPanel, false);
                    panelAnimator.SetBool(musicPanel, false);
                    currentMode = ModeSelect.Charactor;
                    audioSource.time = sabiTIme[selecter];
                }

                //曲選択
                //selecterは曲の配列のindex
                if (Button2.GetButtonDown("Button2_Vartical")) {
                    selecter += -(int)Input.GetAxisRaw("Button2_Vartical");
                    //配列の範囲を超えないように制限
                    if (selecter < 0) {
                        selecter = musicUI.Length - 1;
                    }
                    if (selecter > musicUI.Length - 1) {
                        selecter = 0;
                    }
                    audioSource.clip = musics[selecter];
                    audioSource.time = sabiTIme[selecter];
                    audioSource.Play();
                }

                

                //全ての曲の背景を一括で暗くする
                foreach (var ui in musicUI) {
                    ui.color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                }
                //選んだ曲の背景を明るくする
                musicUI[selecter].color = new Color(0.38f, 0.38f, 0.38f, 1f);
                for (int i=0;i<levelUI.Length;i++) {
                    levelUI[i].GetComponentInChildren<Text>().text = level[selecter][i].ToString();
                }



                //遊ぶ曲を確定する
                if (Input.GetButtonDown("Return")) {
                    //難易度選択へ進む
                    currentMode = ModeSelect.Level;
                }
                break;

            //難易度選択
            case ModeSelect.Level:
                if (Input.GetButtonDown("Cancel")) {
                    currentMode = ModeSelect.Music;
                    charactorAnimator.SetBool(setting, false);
                    panelAnimator.SetBool(setting, false);
                }

                switch (currentLevel) {
                    case LevelSelect.Esey:
                        currentLevel = GameMaster.HorizontalSelect<LevelSelect>(currentLevel,LevelSelect.Hard,LevelSelect.Normal);
                        GameMaster.SetColors(levelUI, (int)currentLevel);
                        break;
                    case LevelSelect.Normal:
                        currentLevel = GameMaster.HorizontalSelect<LevelSelect>(currentLevel, LevelSelect.Esey, LevelSelect.Hard);
                        GameMaster.SetColors(levelUI, (int)currentLevel);
                        break;
                    case LevelSelect.Hard:
                        currentLevel = GameMaster.HorizontalSelect<LevelSelect>(currentLevel, LevelSelect.Normal, LevelSelect.Esey);
                        GameMaster.SetColors(levelUI, (int)currentLevel);
                        break;
                    default:
                        break;
                }



                if (Input.GetButtonDown("Return")) {
                    //キャラクターと曲選択両方のboolをTrueにする
                    charactorAnimator.SetBool(setting, true);
                    panelAnimator.SetBool(setting, true);
                    settingAnimator.SetBool(setting, true);
                    currentMode = ModeSelect.Settings;
                }
                break;

            //各種設定
            case ModeSelect.Settings:
                //曲選択に戻る
                if (Input.GetButtonDown("Cancel")) {
                    currentMode = ModeSelect.Music;
                    charactorAnimator.SetBool(setting,false);
                    panelAnimator.SetBool(setting, false);
                    settingAnimator.SetBool(setting, false);
                }
                

                //設定項目の選択
                switch (currentSetting) {
                    case SettingSelect.NotesSpeed:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting,SettingSelect.MusicStart,SettingSelect.TimingAjust);
                        GameMaster.SetColors(SettingsUI, (int)currentSetting);
                        break;
                    case SettingSelect.TimingAjust:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.NotesSpeed, SettingSelect.MusicVolume);
                        GameMaster.SetColors(SettingsUI, (int)currentSetting);
                        break;
                    case SettingSelect.MusicVolume:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.TimingAjust, SettingSelect.SEVolume);
                        GameMaster.SetColors(SettingsUI, (int)currentSetting);
                        break;
                    case SettingSelect.SEVolume:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.MusicVolume, SettingSelect.MusicStart);
                        GameMaster.SetColors(SettingsUI, (int)currentSetting);
                        break;
                    case SettingSelect.MusicStart:
                        currentSetting = GameMaster.VarticalSelect<SettingSelect>(currentSetting, SettingSelect.SEVolume, SettingSelect.NotesSpeed);
                        GameMaster.SetColors(SettingsUI, (int)currentSetting);
                        //メインシーンへ進む
                        if (Input.GetButtonDown("Return")) {
                            //選んだ設定をGameMasterに反映する
                            GameMaster.MusicName = musics[selecter].name;
                            GameMaster.MusicLevel = currentLevel.ToString();
                            GameMaster.NoteSpeed = notesSpeedSlider.value;
                            GameMaster.Ajust = (int)ajustSlider.value;
                            GameMaster.MusicVolume = (int)musicVolumeSlider.value;
                            GameMaster.SEVolume = (int)seVolumeSlider.value;
                            GameMaster.SceneChanger(SceneName.Main);
                        }
                        break;
                    default:
                        break;
                }

                //ボタン短押し
                if (Button2.GetButtonDown("Button2_Horizontal")) {
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

                //ボタン長押し
                if (Input.GetAxisRaw("Button2_Horizontal") != 0) {
                    
                    interval += GameMaster.DeltaTime;
                    if (interval > 0.15f) {
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
                        interval = 0;
                    }
                } else {
                    interval = 0;
                }
                break;

            //論理エラーが起きた時の処理
            default:
                Debug.Log("遷移失敗");
                GameMaster.SceneChanger(SceneName.Title);
                break;
        }
    }
}
