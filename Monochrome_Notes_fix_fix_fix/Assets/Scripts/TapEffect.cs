using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapEffect : MonoBehaviour {


    private float effNum = 0;
    private Image effect;
    RectTransform rectTransform;
	// Use this for initialization
	void Start () {
        effect = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        float deltaTime = GameMaster.DeltaTime;
        effNum = Mathf.MoveTowards(effNum, 0, 30);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, effNum);
	}

    public void SetEffectData(Color _color,float _effNum) {
        effect.color = _color;
        effNum = _effNum;
    }
}
