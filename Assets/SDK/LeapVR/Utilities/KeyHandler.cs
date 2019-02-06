using UnityEngine;
using UnityEngine.Events;

namespace LeapVR {
    /// <summary>
    /// Do something when a key is pressed.
    /// </summary>
    [AddComponentMenu("Leap VR/Utilities/Key Handler")]
    class KeyHandler : MonoBehaviour {
        public KeyCode Key = KeyCode.None;
        public UnityEvent Event;

        void Update() {
            if (Input.GetKeyDown(Key))
                Event?.Invoke();
        }
    }
}