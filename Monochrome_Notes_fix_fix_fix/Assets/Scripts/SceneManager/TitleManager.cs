using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{

    private enum Select
    {
        Start, Tutorial, Exit
    }
    private Select select = Select.Start;
    [SerializeField] GameObject[] icon = new GameObject[3];

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
                //ここにシーン遷移の処理
                break;
            case Select.Tutorial:
                UpSelect(Select.Start);
                DownSelect(Select.Exit);
                //ここにシーン遷移の処理
                break;
            case Select.Exit:
                UpSelect(Select.Tutorial);
                //ここにシーン遷移の処理
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
}

