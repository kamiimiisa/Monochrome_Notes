using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetValueSlider : MonoBehaviour {

    private Text text;
    [SerializeField] private Slider slider;
    // Use this for initialization
    void Start() {
        text = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        NumSet(slider.value);
    }

    public void NumSet(float _num) {
        text.text = _num.ToString();
    }
}