using UnityEngine;

public class Target : MonoBehaviour {
    public void OnHit() {
        Destroy(gameObject);
    }
}