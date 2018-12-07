using UnityEngine;
using UnityEngine.UI;
using System;
using UI;
using Cavern;

public class ScoreCollector : MonoBehaviour {
    public static ScoreCollector Instance;

    public GameObject ScoreAddition;
    public AudioClip Slash;
    public Text ScoreDisplay;

    public int LaneCount = 1;
    public Image[] LaneElements;

    public int ComboStep = 2;
    public int MaxCombo = 50;

    public float DecayMultiplier = 0.15f;

    [NonSerialized] public int Score;

    int ElemsPerLane;
    int Combo = 1;

    void Start() {
        ScoreDisplay.transform.parent.SetParent(null); // Connected for single prefab, needs to be disconnected for Canvas display
        ScoreDisplay.text = "0";
        if (Instance)
            Destroy(Instance);
        Instance = this;
        ElemsPerLane = LaneElements.Length / LaneCount;
    }

    void FixedUpdate() {
        ScoreDisplay.enabled = !Menu.Instance.GameOverUI.activeSelf;
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

    void OnDestroy() {
        if (ScoreDisplay)
            Destroy(ScoreDisplay.transform.parent.gameObject);
    }

    public void OnHit(Target Hit) {
        if (Menu.Instance.GameOverUI.activeSelf)
            return;
        AudioSource3D.PlayClipAtPoint(Slash, Hit.transform.position);
        int Lane = Hit.transform.position.x < 0 ? 0 : 1;
        Color TargetColor = Hit.GetComponent<Renderer>().material.color;
        AddToLane(Lane, TargetColor);
        Destroy(Hit.gameObject);
    }

    public void GameOver() {
        Menu Base = Menu.Instance;
        if (!Base.GameOverUI.activeSelf) {
            Base.GameOverUI.SetActive(true);
            Base.GameOverScore.text = Score.ToString();
            string Song = Base.SongName;
            if (!Base.HighScores.ContainsKey(Song) || Base.HighScores[Song] < Score)
                Base.HighScores[Song] = Score;
            Base.GameOverHighScore.text = Base.HighScores[Song].ToString();
        }
    }

    void AddScore(int ScoreGain) {
        Score += ScoreGain;
        ScoreDisplay.text = Score.ToString();
        GameObject Addition = Instantiate(ScoreAddition);
        Addition.transform.SetParent(ScoreDisplay.transform.parent, false);
        Addition.GetComponent<ScoreAdditionDisplay>().Score = ScoreGain;
    }

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
        for (int i = 0; i <= ElemsPerLane; ++i) {
            if (i == ElemsPerLane) {
                if ((Tint.r == Tint.g && Tint.r == Tint.b) || IsGreyscale(Lane, i - 1))
                    GameOver();
                else {
                    int Wiped = 0;
                    for (int j = i - 1; j >= 0; --j) {
                        if (IsGreyscale(Lane, j))
                            break;
                        ++Wiped;
                        ClearElement(Lane, i = j);
                    }
                    if (Wiped == 0)
                        GameOver();
                    else
                        AddScore(Wiped * 25);
                }
            }
            if (i != ElemsPerLane && IsWhite(Lane, i)) {
                SetElement(Lane, i, Tint);
                AddScore(Combo);
                Combo = Math.Min(Combo + ComboStep, MaxCombo);
                return;
            }
        }
    }
}