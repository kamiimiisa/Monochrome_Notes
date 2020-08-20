using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Config;

public class LoadManager : MonoBehaviour {

    private AsyncOperation async;
    [SerializeField] private Image slider;
    private static SceneName nextScene = SceneName.Main;
    public static SceneName NextScene {
        get { return nextScene; }
        set { nextScene = value; }
    }
    // Use this for initialization
    void Start() {

        //　コルーチンを開始
        StartCoroutine("LoadData");
    }

    IEnumerator LoadData() {
        // シーンの読み込みをする
        async = SceneManager.LoadSceneAsync(nextScene.ToString());

        //　読み込みが終わるまで進捗状況をスライダーの値に反映させる
        while (!async.isDone) {
            var progressVal = Mathf.Clamp01(async.progress / 0.9f);
            slider.fillAmount = progressVal;
            yield return null;
        }
    }


}
