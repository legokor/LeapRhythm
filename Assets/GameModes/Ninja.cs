using UnityEngine;

namespace GameModes {
    public class Ninja : GameMode {
        public override float MinPunchDelta { get { return .02f; } }

        public override void PaintBox(ref BoxEntry Box) {
            if (Mathf.Abs(Box.Position.x * Box.Position.y) < .2f) {
                float Hue = Box.Timestamp % 1;
                Box.Tint = Color.HSVToRGB(Hue, 1, 1);
                Box.Edge = Color.HSVToRGB(Hue, 1, .5f);
            } else {
                Box.Tint = new Color(.5f, .5f, .5f, 1);
                Box.Edge = new Color(.25f, .25f, .25f, 1);
            }
        }
    }
}
