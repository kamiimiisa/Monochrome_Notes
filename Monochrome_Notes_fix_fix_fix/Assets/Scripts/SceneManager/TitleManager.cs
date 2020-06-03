using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Config;

public class TitleManager : MonoBehaviour {

    private SceneName sceneName = SceneName.MusicSelect;
    [SerializeField] GameObject[] icon = new GameObject[3];
    MyInput Button2 = new MyInput();

    private int selecter = 0;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Button2.GetButtonDown("Button2_3")) {
            selecter += -(int)Input.GetAxisRaw("Button2_3");
            if (selecter < 0) {
                selecter = 0;
            }
            if (selecter > icon.Length - 1) {
                selecter = icon.Length - 1;
            }
            IconSelect(selecter);
        }

        switch (selecter) {
            case 0:
                sceneName = SceneName.MusicSelect;
                break;
            case 1:
                sceneName = SceneName.Tutorial;
                break;
            case 2:
                sceneName = SceneName.Exit;
                break;
            default:
                break;
        }

        if (Input.GetButtonDown("Return")) {
            GameMaster.SceneChanger(sceneName);
        }
    }
    
    void IconSelect(int _index) {
        foreach (var _icon in icon) {
            _icon.SetActive(false);
        }
        icon[_index].SetActive(true);
    }
}

