using AudioProcessor;
using GameModes;
using LeapVR;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(LeapMouse))]
public class Menu : MonoBehaviour {
    public static Menu Instance;

    public Transform MenuPosition;
    public Transform IngamePosition;

    public GameObject Collector;
    public GameObject Dispenser;
    [Tooltip("The tick for Free For All mode which must be ticked initially. Others are detected automatically.")]
    public Toggle FFATicker;

    public GameObject GameOverUI;
    public Text LoadingText, GameOverScore, GameOverHighScore;

    GameModeType GameMode = GameModeType.FreeForAll;

    Canvas MenuCanvas;
    FileInfo LastSong;
    LeapMouse Mouse;
    FilePicker Picker = new FilePicker();

    public Dictionary<string, int> HighScores = new Dictionary<string, int>();
    public string SongName => LastSong.Name.Substring(0, LastSong.Name.LastIndexOf('.'));

    Dictionary<GameModeType, Toggle> ModeSwitchers = new Dictionary<GameModeType, Toggle>();

    const string HighScoresFile = "HighScores.txt";

    void Start() {
        if (Instance)
            Destroy(Instance);
        Instance = this;
        MenuCanvas = GetComponent<Canvas>();
        Mouse = GetComponent<LeapMouse>();
        Picker.Folder = PlayerPrefs.GetString("Folder", "\\");
        Picker.OnFileLoaded += StartSong;
        ModeSwitchers.Add(GameModeType.FreeForAll, FFATicker);
        string[] ScoresIn;
        try {
            ScoresIn = File.ReadAllLines(HighScoresFile);
        } catch {
            return;
        }
        foreach (string Score in ScoresIn) {
            int SeparatorPos = Score.IndexOf('\\'), Parsed;
            if (SeparatorPos != -1 && int.TryParse(Score.Substring(SeparatorPos + 1), out Parsed))
                HighScores[Score.Substring(0, SeparatorPos)] = Parsed;
        }
    }

    public void ShowPicker() {
        Picker.Show();
    }

    public void SetGameMode(Toggle Source) {
        if (!Source.isOn)
            return;
        GameModeType Mode = Source.GetComponent<GameModeMarker>().Mode;
        if (!ModeSwitchers.ContainsKey(Mode))
            ModeSwitchers.Add(Mode, Source);
        foreach (Toggle Target in ModeSwitchers.Values)
            if (Target != Source)
                Target.isOn = false;
        GameMode = Mode;
    }

    public void SetMenuVisibility(bool Target) {
        MenuCanvas.enabled = Target;
        Mouse.enabled = Target;
    }

    void StartSong(FileInfo Loaded) {
        LastSong = Loaded;
        MapReader Reader = (new GameObject()).AddComponent<MapReader>();
        Reader.Mode = GameMode;
        WWW Loader = new WWW("file://" + Loaded.FullName);
        while (!Loader.isDone) ;
        Reader.name = SongName;
        Reader.Progress = LoadingText;
        Reader.Song = Loader.GetAudioClip();
    }

    public void GameOver() {
        ScoreCollector.Instance.GameOver();
    }

    public void RestartSong() {
        EndSong();
        StartSong(LastSong);
    }

    public void EndSong() {
        MapReader.DestroyMap();
        GameOverUI.SetActive(false);
    }

    public void SetVR(bool State) {
        SBS.Enabled = State;
    }

    public void Exit() {
        Application.Quit();
    }

    void OnGUI() {
        Picker.OnGUI(1);
    }

    void Update() {
        if (GameOverUI.activeSelf) {
            if (!Mouse.enabled)
                Mouse.enabled = true;
        } else if (ScoreCollector.Instance) {
            SetMenuVisibility(false);
            if (Input.GetKeyDown(KeyCode.Escape))
                GameOver();
        } else if (MenuCanvas.enabled == Picker.Open)
            SetMenuVisibility(!Picker.Open);
        Transform Cam = Camera.main.transform;
        float Timing = Time.deltaTime * 5;
        Cam.position = Vector3.Lerp(Cam.position, ScoreCollector.Instance ? IngamePosition.position : MenuPosition.position, Timing);
        Cam.rotation = Quaternion.Lerp(Cam.rotation, ScoreCollector.Instance ? IngamePosition.rotation : MenuPosition.rotation, Timing);
    }

    void OnDestroy() {
        PlayerPrefs.SetString("Folder", Picker.Folder);
        PlayerPrefs.Save();
        StringBuilder ScoresOut = new StringBuilder();
        Dictionary<string, int>.Enumerator Enumer = HighScores.GetEnumerator();
        while (Enumer.MoveNext())
            ScoresOut.Append(Enumer.Current.Key).Append("\\").Append(Enumer.Current.Value);
        try {
            File.WriteAllText(HighScoresFile, ScoresOut.ToString());
        } catch { }
    }
}
