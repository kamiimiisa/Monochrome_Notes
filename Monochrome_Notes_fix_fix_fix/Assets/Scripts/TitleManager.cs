using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Config;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour {

    
    [SerializeField] GameObject[] keyboard;
    [SerializeField] GameObject[] joystick;

    private bool canSettings = false;
    [SerializeField] private Animator SettingAnimator;
    private MyInput Button2 = new MyInput();
    private float settingInterval = 0;
    [SerializeField] private GameObject cursor;
    [SerializeField] private Slider notesSpeedSlider;
    [SerializeField] private Slider ajustSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;
    [SerializeField] private AudioSource musicSource;
    private enum SettingSelect {
        NotesSpeed,
        TimingAjust,
        MusicVolume,
        SEVolume,
    }
    private static SettingSelect currentSetting = SettingSelect.NotesSpeed;
    // Use this for initialization
    void Start() {
        GameMaster.UiChangerConst(keyboard, joystick);

        //スライダーの設定をGameMasterから読み取る
        notesSpeedSlider.value = GameMaster.NoteSpeed;
        ajustSlider.value = GameMaster.Ajust;
        musicVolumeSlider.value = GameMaster.MusicVolume;
        seVolumeSlider.value = GameMaster.SEVolume;
        musicSource.volume = GameMaster.MusicVolume * 0.1f;
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetButtonDown("Return")) {
            if (canSettings) {
                canSettings = false;
                SettingAnimator.SetBool("CanSetting", false);
            } else {
                GameMaster.NoteSpeed = notesSpeedSlider.value;
                GameMaster.Ajust = (int)ajustSlider.value;
                GameMaster.MusicVolume = (int)musicVolumeSlider.value;
                GameMaster.SEVolume = (int)seVolumeSlider.value;
                GameMaster.SceneChanger(SceneName.MusicSelect);
            }
        }
        if (Input.GetButtonDown("Cancel")) {
            if (canSettings) {
                canSettings = false;
                SettingAnimator.SetBool("CanSetting", false);
            } else {
                GameMaster.SceneChanger(SceneName.Exit);
            }
        }
        if (Input.GetButtonDown("Pouse")) {
            SettingAnimator.SetBool("CanSetting", true);
            canSettings = true;
        }

        GameMaster.UiChanger(keyboard, joystick);

        if (canSettings) {
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
                    musicSource.volume = musicVolumeSlider.value * 0.1f;
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

