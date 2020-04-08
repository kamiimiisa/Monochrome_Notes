using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

<<<<<<< Updated upstream
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
=======
public class TitleManager : MonoBehaviour {

	[SerializeField] GameObject[] cursor;
	enum Select
	{
		start,
		tutorial,
		exit
	}
	Select select = Select.start;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		//十字キーでのカーソル移動
		switch (select)
		{
			case Select.start:
				DownSelect(Select.tutorial);


				break;
			case Select.tutorial:
				UpSelect(Select.start);
				DownSelect(Select.exit);



				break;
			case Select.exit:
				UpSelect(Select.tutorial);



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
		}
		Visible(_select);

	}

	void DownSelect(Select _select) {
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			select = _select;
		}
		Visible(_select);
	}

	void Visible(Select _select)
	{
		for (int i = 0; i <cursor.Length; i++)
		{
			cursor[i].SetActive(false);
		}
		cursor[(int)System.Enum.ToObject(typeof(int), _select)].SetActive(true);
	}


>>>>>>> Stashed changes
}

