using UnityEngine;
using UnityEngine.UI;
using System;

public class ScoreCollector : MonoBehaviour {
    public static ScoreCollector Instance;

    public int LaneCount = 1;
    public Image[] LaneElements;

    public int ComboStep = 2;
    public int MaxCombo = 50;

    public float DecayMultiplier = 0.15f;

    [NonSerialized] public bool GameOver;
    [NonSerialized] public int Score;

    public delegate void GameOverDelegate();
    public event GameOverDelegate OnGameOver;

    public delegate void ScoreDelegate(int ScoreGain);
    public event ScoreDelegate OnScore;

    int ElemsPerLane;
    int Combo = 1;

    void Start() {
        if (Instance)
            Destroy(Instance);
        Instance = this;
        ElemsPerLane = LaneElements.Length / LaneCount;
        OnScore += AddScore;
    }

    void FixedUpdate() {
        // TODO: decay (colors lighten, if a block is moved, reset decay)
        float DecayRate = Time.fixedDeltaTime * DecayMultiplier;
        for (int i = 0, c = LaneElements.Length; i < c; ++i) {
            if (!IsWhite(LaneElements[i])) {
                if (IsGreyscale(LaneElements[i])) {
                    float TargetColor = LaneElements[i].color.r - DecayRate;
                    if (TargetColor <= 0) {
                        int Lane = i / ElemsPerLane;
                        for (int j = i % ElemsPerLane + 1; j < ElemsPerLane; ++j)
                            SetElement(Lane, j - 1, GetElement(Lane, j));
                        SetElement(Lane, ElemsPerLane - 1, new Color(1, 1, 1, 1));
                        --i;
                    } else LaneElements[i].color = new Color(TargetColor, TargetColor, TargetColor, 1);
                }
            }
        }
    }

    public void OnHit(Target Hit) {
        int Lane = Hit.transform.position.x < 0 ? 0 : 1;
        Color TargetColor = Hit.GetComponent<Renderer>().material.color;
        AddToLane(Lane, TargetColor);
        Destroy(Hit);
    }

    void AddScore(int ScoreGain) => Score += ScoreGain;
    void SetElement(int Lane, int Element, Color Tint) => LaneElements[ElemsPerLane * Lane + Element].color = Tint;
    void ClearElement(int Lane, int Element) => SetElement(Lane, Element, new Color(1, 1, 1, 1));
    Color GetElement(int Lane, int Element) => LaneElements[ElemsPerLane * Lane + Element].color;
    bool IsGreyscale(Image Block) => Block.color.r == Block.color.g && Block.color.r == Block.color.b;
    bool IsGreyscale(int Lane, int Element) => IsGreyscale(LaneElements[ElemsPerLane * Lane + Element]);
    bool IsWhite(Image Block) => Block.color.r == 1 && Block.color.g == 1 && Block.color.b == 1;
    bool IsWhite(int Lane, int Element) => IsWhite(LaneElements[ElemsPerLane * Lane + Element]);

    void ClearLane(int Lane) {
        for (int i = 0; i < ElemsPerLane; ++i)
            ClearElement(Lane, i);
    }

    void AddToLane(int Lane, Color Tint) {
        for (int i = 0; i < ElemsPerLane; ++i) {
            if (IsWhite(Lane, i)) {
                if (i + 1 == ElemsPerLane) {
                    if (Tint.r == Tint.g && Tint.r == Tint.b) {
                        GameOver = true;
                        OnGameOver?.Invoke();
                    } else {
                        int Wiped = 0;
                        for (int j = i; j >= 0; --j) {
                            if (IsGreyscale(Lane, j))
                                break;
                            ++Wiped;
                            ClearElement(Lane, i = j);
                        }
                        if (Wiped > 0)
                            OnScore?.Invoke(Wiped * 25);
                    }
                }
                SetElement(Lane, i, Tint);
                OnScore?.Invoke(Combo);
                Combo = Math.Max(Combo + ComboStep, MaxCombo);
                return;
            }
        }
    }
}