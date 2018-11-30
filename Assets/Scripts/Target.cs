using UnityEngine;

public class Target : MonoBehaviour {
    bool HitInFrame = false;

    public void OnHit() {
        if (!HitInFrame && ScoreCollector.Instance)
            ScoreCollector.Instance.OnHit(this);
        HitInFrame = true;
    }

    void Update() {
        HitInFrame = false;
    }
}