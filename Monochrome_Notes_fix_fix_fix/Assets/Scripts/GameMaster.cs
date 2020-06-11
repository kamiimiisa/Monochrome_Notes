using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Config;
using System.IO;

public class GameMaster : SingletonMonoBehaviour<GameMaster> {

    private static string musicName = "tutorial";
    public static string MusicName {
        get { return musicName; }
        set { musicName = value; }
    }

    private static string musicLevel = "Normal";
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

    private static float noteSpeed = 13f;
    public static float NoteSpeed {
        get { return noteSpeed ; }
        set { noteSpeed = value; }
    }

    private static int ajust = 0;
    public static int Ajust {
        get { return ajust; }
        set { ajust = value; }
    }

    private static int musicVolume =10;
    public static int MusicVolume {
        get { return musicVolume; }
        set { musicVolume = value; }
    }

    private static int seVolume = 10;
    public static int SEVolume {
        get { return seVolume; }
        set { seVolume = value; }
    }
    
    private static float deltaTime;
    public static float DeltaTime{
        get { return deltaTime; }
    }



    private TextAsset csvFile;
    private static List<string[]> csv = new List<string[]>();
    public static List<string[]> CSV {
        get { return csv; }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;
    }

    public void Awake()
    {


        CSVReader();
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        } else {

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

    private void CSVReader() {
        csvFile = Resources.Load("CSV/" + "Monochrome_Note_MusicList") as TextAsset;
        StringReader reader = new StringReader(csvFile.text);

        while(reader.Peek() != -1) {
            string line = reader.ReadLine();
            csv.Add(line.Split(','));
        }
    }

}
