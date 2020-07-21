using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class Character_B : Character {

    private static Dictionary<Judge, float> JUDGE_RANGE = new Dictionary<Judge, float>()
    { 
        {Judge.Pafect, 0.075f},
        {Judge.Graet, 0f},
        {Judge.Miss, 0.2f},
        {Judge.HoldStart,0.1f},
        {Judge.Hold,0.075f},
        {Judge.ExTap,0.2f},
    };

    private static Dictionary<Judge, int> JUDGE_SCORE = new Dictionary<Judge, int>()
    {
        {Judge.Pafect,2000},
        {Judge.Graet,500},
        {Judge.Miss,0},
        {Judge.HoldStart,500},
        {Judge.Hold,50},
        {Judge.HoldEnd,500},
        {Judge.ExTap,4000},
    };

    public override void Skill() {
        MainManager.JUDGE_RANGE = JUDGE_RANGE;
        MainManager.JUDGE_SCORE = JUDGE_SCORE;
    }
}
