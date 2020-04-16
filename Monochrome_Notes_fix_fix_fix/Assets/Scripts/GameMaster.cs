using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    private float noteSpeed;
    private string musicName;

    public void SetNoteSpeed(float _speed) {
        noteSpeed = _speed;
    }
    public float GetNoteSpeed() {
        return noteSpeed;
    }



    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
