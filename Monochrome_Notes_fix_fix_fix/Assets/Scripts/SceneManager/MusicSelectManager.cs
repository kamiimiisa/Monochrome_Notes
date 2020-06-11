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
    private MyInput Button2 = new MyInput();
    private MyInput Button2_2 = new MyInput();

    [SerializeField]
    private AudioClip[] musics;

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

    
    // Use this for initialization
    void Start() {
        notesSpeedSlider = SettingsUI[0].GetComponentInChildren<Slider>();
        ajustSlider =SettingsUI[1].GetComponentInChildren<Slider>();
        musicVolumeSlider = SettingsUI[2].GetComponentInChildren<Slider>();
        seVolumeSlider = SettingsUI[3].GetComponentInChildren<Slider>();

        notesSpeedSlider.value = GameMaster.NoteSpeed;
        ajustSlider.value = GameMaster.Ajust;
        musicVolumeSlider.value = GameMaster.MusicVolume;
        seVolumeSlider.value = GameMaster.SEVolume; ;
    }

    // Update is called once per frame
    void Update() {
        switch (currentMode) {
            //キャラクター選択
            case ModeSelect.Charactor:
                //キャラクターを選択する処理
                if (Button2.GetButtonDown("Button2_2")) {
                    if (Input.GetAxisRaw("Button2_2") < 0) {
                        charactorAnimator.SetInteger(charactor__Num, 0);
                    }
                    if (Input.GetAxisRaw("Button2_2") > 0) {
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
                }

                //曲選択
                //selecterは曲の配列のindex
                if (Button2.GetButtonDown("Button2_3")) {
                    selecter += -(int)Input.GetAxisRaw("Button2_3");
                }

                //配列の範囲を超えないように制限
                if (selecter < 0) {
                    selecter = 0;
                }
                if (selecter > musicUI.Length - 1) {
                    selecter = musicUI.Length - 1;
                }

                //全ての曲の背景を一括で暗くする
                foreach (var ui in musicUI) {
                    ui.color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
                }
                //選んだ曲の背景を明るくする
                musicUI[selecter].color = new Color(0.38f, 0.38f, 0.38f, 1f);



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
                        NextLevel(LevelSelect.Esey, LevelSelect.Normal);
                        LevelUIColler(LevelSelect.Esey);
                        break;
                    case LevelSelect.Normal:
                        NextLevel(LevelSelect.Esey, LevelSelect.Hard);
                        LevelUIColler(LevelSelect.Normal);
                        break;
                    case LevelSelect.Hard:
                        NextLevel(LevelSelect.Normal, LevelSelect.Hard);
                        LevelUIColler(LevelSelect.Hard);
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
                        NextSetting(SettingSelect.NotesSpeed, SettingSelect.TimingAjust);
                        SettingUIColler(SettingSelect.NotesSpeed);
                        break;
                    case SettingSelect.TimingAjust:
                        NextSetting(SettingSelect.NotesSpeed, SettingSelect.MusicVolume);
                        SettingUIColler(SettingSelect.TimingAjust);
                        break;
                    case SettingSelect.MusicVolume:
                        NextSetting(SettingSelect.TimingAjust, SettingSelect.SEVolume);
                        SettingUIColler(SettingSelect.MusicVolume);
                        break;
                    case SettingSelect.SEVolume:
                        NextSetting(SettingSelect.MusicVolume, SettingSelect.MusicStart);
                        SettingUIColler(SettingSelect.SEVolume);
                        break;
                    case SettingSelect.MusicStart:
                        NextSetting(SettingSelect.SEVolume, SettingSelect.MusicStart);
                        SettingUIColler(SettingSelect.MusicStart);
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

                if (Button2_2.GetButtonDown("Button2_2")) {
                    switch (currentSetting) {
                        case SettingSelect.NotesSpeed:
                            notesSpeedSlider.value += 0.5f * (int)Input.GetAxisRaw("Button2_2");
                            break;
                        case SettingSelect.TimingAjust:
                            ajustSlider.value += (int)Input.GetAxisRaw("Button2_2");
                            break;
                        case SettingSelect.MusicVolume:
                            musicVolumeSlider.value += (int)Input.GetAxisRaw("Button2_2");
                            break;
                        case SettingSelect.SEVolume:
                            seVolumeSlider.value += (int)Input.GetAxisRaw("Button2_2");
                            break;
                        default:
                            break;
                    }
                }
                break;

            //論理エラーが起きた時の処理
            default:
                Debug.Log("遷移失敗");
                GameMaster.SceneChanger(SceneName.Title);
                break;
        }
    }

    private void NextLevel(LevelSelect _before, LevelSelect _next) {
        if (Button2_2.GetButtonDown("Button2_2")) {
            if (Input.GetAxisRaw("Button2_2") < 0) {
                currentLevel = _before;
            }
            if (Input.GetAxisRaw("Button2_2") > 0) {
                currentLevel = _next;
            }
        }

    }
    private void LevelUIColler(LevelSelect _select) {
        //全ての設定項目の背景を一括で暗くする
        foreach (var ui in levelUI) {
            ui.color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
        }

        //選んだ設定項目の背景を明るくする
        levelUI[(int)_select].color = new Color(0.38f, 0.38f, 0.38f, 1f);
    }


    private void SettingUIColler(SettingSelect _select) {
        //全ての設定項目の背景を一括で暗くする
        foreach (var ui in SettingsUI) {
            ui.color = new Color(0.38f, 0.38f, 0.38f, 0.15f);
        }
        
        //選んだ設定項目の背景を明るくする
        SettingsUI[(int)_select].color = new Color(0.38f, 0.38f, 0.38f, 1f);
    }

    private void NextSetting(SettingSelect _before,SettingSelect _next) {
        if (Button2.GetButtonDown("Button2_3")) {
            if (Input.GetAxisRaw("Button2_3") > 0 ) {
                currentSetting = _before;
            }
            if (Input.GetAxisRaw("Button2_3") < 0) {
                currentSetting = _next;
            }
        }

    }
}
