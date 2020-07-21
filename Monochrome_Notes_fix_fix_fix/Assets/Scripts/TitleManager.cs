using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Config;

public class TitleManager : MonoBehaviour {

    
    [SerializeField] GameObject[] keyboard;
    [SerializeField] GameObject[] joystick;

    // Use this for initialization
    void Start() {
        GameMaster.UiChangerConst(keyboard, joystick);
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetButtonDown("Return")) {
            GameMaster.SceneChanger(SceneName.MusicSelect);
        }
        if (Input.GetButtonDown("Cancel")) {
            GameMaster.SceneChanger(SceneName.Exit);
        }

        GameMaster.UiChanger(keyboard, joystick);
    }
    
}

