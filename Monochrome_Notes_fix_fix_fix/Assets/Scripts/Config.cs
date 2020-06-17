namespace Config {
    public enum Line {
        Line1,
        Line2,
        Line3,
        Line4,
        Line5
    }

    public enum NoteType {
        Touch,
        HoldStart,
        Hold,
        HoldEnd,
        Break,
        BreakStart,
        BreakHold,
        BreakHoldEnd,
    }

    public enum Judge {
        Pafect,
        Graet,
        Miss,
        HoldStart,
        Hold,
        HoldEnd,
        Break,
        BreakStart,
        BreakHold,
        BreakHoldEnd,
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