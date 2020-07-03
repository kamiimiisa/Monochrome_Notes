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
        Tutorial,
        Exit,
    }

    public enum PouseStatus {
        Retry,
        Resume,
        MusicSelect,
    }
}