using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class MusicSelectManager : MonoBehaviour {

    private int selecter = 0;
    private MyInput Button2 = new MyInput();

    [SerializeField]
    private AudioClip[] musics;

    [SerializeField]
    private GameObject[] musicUI;

    [SerializeField]
    private GameObject icon; 


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Button2.GetButtonDown("Button2_3")) {
            selecter += -(int)Input.GetAxisRaw("Button2_3");
        }
        if (selecter < 0) {
            selecter = 0;
        }
        if(selecter > musicUI.Length - 1) {
            selecter = musicUI.Length - 1;
        }
        Vector3 pos = icon.transform.position;
        pos.y = musicUI[selecter].transform.position.y;
        icon.transform.position = pos;

        if (Input.GetKeyDown(KeyCode.Joystick1Button2)) {
            GameMaster.MusicName = musics[selecter].name;
            GameMaster.SceneChanger(SceneName.Main);
        }

    }
}
