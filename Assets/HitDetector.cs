using UnityEngine;
using UnityEngine.Events;

public class HitDetector : MonoBehaviour {
    // TODO: impact speed detector
    public UnityEvent OnHit;

    void OnTriggerEnter(Collider other) {
        OnHit.Invoke();
    }
}