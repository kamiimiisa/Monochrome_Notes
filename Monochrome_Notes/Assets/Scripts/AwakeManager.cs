using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class AwakeManager : MonoBehaviour {

    private float feadTime = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        feadTime += Time.deltaTime;
        if (feadTime >= 3) {
            GameMaster.SceneChanger(SceneName.Title);
        }
	}
}
