using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class MusicSelectManager : MonoBehaviour {

    [SerializeField] private Animator charactorAnimator;
    [SerializeField] private Animator panelAnimator;

    private int selecter = 0;
    private MyInput Button2 = new MyInput();

    [SerializeField]
    private AudioClip[] musics;

    [SerializeField]
    private GameObject[] musicUI;

    [SerializeField]
    private GameObject icon;

    private enum Select {
        Charactor,
        Music,
        Settings,
    }

    Select s = Select.Charactor;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        switch (s) {
            case Select.Charactor:
                if (Button2.GetButtonDown("Button2_2")) {
                    if (Input.GetAxisRaw("Button2_2") < 0) {
                        charactorAnimator.SetInteger("Charactor_Num", 0);
                    }
                    if (Input.GetAxisRaw("Button2_2") > 0) {
                        charactorAnimator.SetInteger("Charactor_Num", 1);
                    }
                }

                if (Input.GetButtonDown("Return")) {
                    charactorAnimator.SetBool("Charactor→Music", true);
                    panelAnimator.SetBool("Charactor→Music", true);
                    s = Select.Music;
                }

                if (Input.GetButtonDown("Cancel")) {
                    GameMaster.SceneChanger(SceneName.Title);
                }
                break;

            case Select.Music:
                if (Input.GetButtonDown("Cancel")) {
                    charactorAnimator.SetBool("Charactor→Music", false);
                    panelAnimator.SetBool("Charactor→Music", false);
                    s = Select.Charactor;
                }

                if (Button2.GetButtonDown("Button2_3")) {
                    selecter += -(int)Input.GetAxisRaw("Button2_3");
                }
                if (selecter < 0) {
                    selecter = 0;
                }
                if (selecter > musicUI.Length - 1) {
                    selecter = musicUI.Length - 1;
                }
                Vector3 pos = icon.transform.position;
                pos.y = musicUI[selecter].transform.position.y;
                icon.transform.position = pos;
                if (Input.GetButtonDown("Return")) {
                    charactorAnimator.SetBool("Music→Settings", true);
                    panelAnimator.SetBool("Music→Settings",true);
                    s = Select.Settings;
                }
                break;

            case Select.Settings:
                if (Input.GetButtonDown("Cancel")) {
                    s = Select.Music;
                    charactorAnimator.SetBool("Music→Settings",false);
                    panelAnimator.SetBool("Music→Settings", false);
                }

                if (Input.GetButtonDown("Return")) {

                    GameMaster.MusicName = musics[selecter].name;
                    GameMaster.SceneChanger(SceneName.Main);
                }
                break;
            default:
                Debug.Log("遷移失敗");
                GameMaster.SceneChanger(SceneName.Title);
                break;
        }
    }
}
