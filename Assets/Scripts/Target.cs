using UnityEngine;

public class Target : MonoBehaviour {
    public void OnHit() {
        if (ScoreCollector.Instance)
            ScoreCollector.Instance.OnHit(this);
    }
}