using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_A : Character {
    private int safeNum = 10;

    public override void Skill() {
        MainManager.SafeNum = safeNum;
    }
}
