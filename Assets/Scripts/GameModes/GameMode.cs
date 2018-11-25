namespace GameModes {
    public enum GameModeType {
        FreeForAll,
        Ninja
    }

    public abstract class GameMode {
        /// <summary>
        /// The minimum time required between spawning cubes.
        /// </summary>
        public abstract float MinPunchDelta { get; }

        public abstract void PaintBox(ref BoxEntry Box);
    }
}