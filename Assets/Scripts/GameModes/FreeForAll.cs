using UnityEngine;

namespace GameModes {
    public class FreeForAll : GameMode {
        public override float MinPunchDelta { get { return .1f; } }

        public override void PaintBox(ref BoxEntry Box) {
            Box.Tint = Box.Position.x < .5f ? new Color(0, .5f, 1, 1) : new Color(1, .5f, 0, 1);
            Box.Edge = Box.Position.x < .5f ? new Color(0, 0, .5f, 1) : new Color(.5f, 0, 0, 1);
        }
    }
}
