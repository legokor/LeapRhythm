using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Text))]
    public class ScoreAdditionDisplay : MonoBehaviour {
        public int Score;
        [Tooltip("1 / this seconds needed to leave the screen.")]
        public float MovementSpeed = .33f;

        static bool Direction = false;
        bool ThisDir;
        Vector3 Movement;

        void Start() {
            GetComponent<Text>().text = "+" + Score;
            ThisDir = Direction;
            Direction = !Direction;
            Movement = (ThisDir ? transform.right : -transform.right) * (((RectTransform)transform.parent).sizeDelta.x * MovementSpeed * .5f);
        }

        void Update() {
            transform.position += Movement * Time.deltaTime;
            Vector3 Viewport = Camera.main.WorldToViewportPoint(transform.position);
            if (Viewport.x < -1.5f || Viewport.x > 1.5f)
                Destroy(gameObject);
        }
    }
}