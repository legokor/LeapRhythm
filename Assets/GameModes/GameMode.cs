namespace GameModes {
    public enum GameModeType {
        FreeForAll,
        Ninja
    }

    public abstract class GameMode {
        public abstract void PaintBox(ref BoxEntry Box);
    }
}