using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class MusicState : MonoBehaviour{

    private MusicState nextMusic;
    private MusicState beforeMusic;
    [SerializeField] private Sprite jacket;
    [SerializeField] private int levelEsey = 0;
    [SerializeField] private int levelNomal = 0;
    [SerializeField] private int levelHard = 0;
    [SerializeField] private AudioClip music;

    public MusicState NextMusic {
        get { return nextMusic; }
        set { nextMusic = value; }
    }
    public MusicState BeforeMusic {
        get { return beforeMusic; }
        set { beforeMusic = value; }
    }
    public Sprite Jacket {
        get { return jacket; }
    }
    public AudioClip Music {
        get { return music; }
    }

    public int GetLevelNum(Level _level) {
        int _num = 0;
        switch (_level) {
            case Level.EASY:
                _num = levelEsey;
                break;
            case Level.NORMAL:
                _num = levelNomal;
                break;
            case Level.HARD:
                _num = levelHard;
                break;
            default:
                _num = 0;
                break;
        }

        return _num;
    }

}
