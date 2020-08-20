using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class MusicState : MonoBehaviour{

    private MusicState nextMusic;
    private MusicState beforeMusic;
    [SerializeField] private Sprite jacket;
    [SerializeField] private AudioClip music;
    [SerializeField] private AudioClip demoMusic;

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
    public AudioClip DemoMusic {
        get { return demoMusic; }
    }
}
