using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{

    private enum Select
    {
        Start, Tutorial, Exit
    }
    private Select select = Select.Start;
    [SerializeField] GameObject[] icon = new GameObject[3];
    string _sceneName;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        switch (select)
        {
            case Select.Start:
                DownSelect(Select.Tutorial);
                loadScene("MusicSelect");
                break;
            case Select.Tutorial:
                UpSelect(Select.Start);
                DownSelect(Select.Exit);
                loadScene("Tutorial");
                break;
            case Select.Exit:
                UpSelect(Select.Tutorial);
                loadScene("Exit");
                break;
            default:
                Debug.Log("バグです");
                break;
        }
    }

    void UpSelect(Select _select)
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            select = _select;
            IconSelect(_select);
        }
    }

    void DownSelect(Select _select)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            select = _select;
            IconSelect(_select);
        }
    }

    void IconSelect(Select _select)
    {
        for (int i = 0; i < icon.Length; i++)
        {
            icon[i].SetActive(false);
        }
        icon[(int)_select].SetActive(true);
    }

    void loadScene(string _sceneName)
    {
        if(Input.GetKeyDown(KeyCode.Space) && _sceneName != "Exit")
        {
            SceneManager.LoadScene(_sceneName);
        }

        if(Input.GetKeyDown(KeyCode.Space) && _sceneName == "Exit")
        {
            Application.Quit();
        }
    }
}

