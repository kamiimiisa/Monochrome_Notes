using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class Charactor_B : Charactor {

    private Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>()
    {
        {Judge.Pafect, 0.06f},
        {Judge.Graet, 0.1f},
        {Judge.Miss, 0.2f},
        {Judge.HoldStart,0.1f},
        {Judge.Hold,0.15f},
        {Judge.Break,0.2f},
        {Judge.BreakStart,0.2f},
    };

    override public void Skill() {
        MainManager.JUDGE_RANGE = JUDGE_RANGE;
    }
}
