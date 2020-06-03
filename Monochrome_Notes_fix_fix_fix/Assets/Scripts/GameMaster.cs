using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Config;

public class GameMaster : SingletonMonoBehaviour<GameMaster>
{
    
    private static string musicName = "tutorial";
    public static string MusicName
    {
        get { return musicName; }
        set { musicName = value; }
    }

    private static int score;
    public static int Score
    {
        get { return score; }
        set { score = value; }
    }

    private static int maxCombo;
    public static int MaxCombo
    {
        get { return maxCombo; }
        set { maxCombo = value; } 
    }

    private static int parfectNum;
    public static int ParfectNum
    {
        get { return parfectNum; }
        set { parfectNum = value; }
    }

    private static int greatNum;
    public static int GreatNum
    {
        get { return greatNum; }
        set { greatNum = value; }
    }

    private static int missNum;
    public static int MissNum
    {
        get { return missNum; }
        set { missNum = value; }
    }

    private static float noteSpeed = 10f;
    public static float NoteSpeed
    {
        get { return noteSpeed * 3; }
        set { noteSpeed = value; }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Awake()
    {
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    public static void SceneChanger(SceneName _sceneName) {
        if (_sceneName != SceneName.Exit) {
            SceneManager.LoadScene(_sceneName.ToString());
        } else {
            Application.Quit();
        }
    }

}
