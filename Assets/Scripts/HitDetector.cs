using UnityEngine;
using UnityEngine.Events;

public class HitDetector : MonoBehaviour {
    public UnityEvent OnHit;

    void OnTriggerEnter(Collider other) {
        OnHit.Invoke();
    }
}