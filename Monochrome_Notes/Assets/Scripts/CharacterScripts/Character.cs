using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Character : MonoBehaviour{
    
    public abstract void Skill();
    private Sprite characterSprite;
    public Sprite CharacterSprite {
        get { return characterSprite; }
        set { characterSprite = value; }
    }

}
