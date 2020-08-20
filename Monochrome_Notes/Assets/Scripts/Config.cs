namespace Config {
    public enum Line {
        Line1,
        Line2,
        Line3,
        Line4,
        Line5,
        Line6,
        Line7,
        Line8,
    }

    public enum NoteType {
        Touch,
        HoldStart,
        Hold,
        HoldEnd,
        ExTap,
    }

    public enum Judge {
        Pafect,
        Graet,
        Miss,
        HoldStart,
        Hold,
        HoldEnd,
        ExTap,
    }

    public enum Sound {
        BGM,
        SE
    }

    public enum SceneName {
        Main,
        MusicSelect,
        Title,
        Load,
        Exit,
    }

    public enum ResultStatus {
        Retry,
        MusicSelect,
        Title,
    }

    public enum PouseStatus {
        Retry,
        Resume,
        MusicSelect,
    }

    public enum Level {
        EASY,
        NORMAL,
        HARD,
    }

    public enum ColorPatern {
        Tap,Hold,ExTap
    }

    public enum ControlMode {
        KeyBoard,
        JoyStick,
    }
}