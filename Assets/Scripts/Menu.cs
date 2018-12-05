using AudioProcessor;
using GameModes;
using LeapVR;
using System.Collections.Generic;
using System.IO;
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
    public Text GameOverScore;

    GameModeType GameMode = GameModeType.FreeForAll;

    Canvas MenuCanvas;
    LeapMouse Mouse;
    FilePicker Picker = new FilePicker();
    MapReader Reader;

    Dictionary<GameModeType, Toggle> ModeSwitchers = new Dictionary<GameModeType, Toggle>();

    void Start() {
        if (Instance)
            Destroy(Instance);
        Instance = this;
        MenuCanvas = GetComponent<Canvas>();
        Mouse = GetComponent<LeapMouse>();
        Picker.Folder = PlayerPrefs.GetString("Folder", "\\");
        Picker.OnFileLoaded += StartSong;
        ModeSwitchers.Add(GameModeType.FreeForAll, FFATicker);
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
        Reader = (new GameObject()).AddComponent<MapReader>();
        Reader.Mode = GameMode;
        WWW Loader = new WWW("file://" + Loaded.FullName);
        while (!Loader.isDone) ;
        Reader.Song = Loader.GetAudioClip();
        int LastDot = Loaded.Name.LastIndexOf('.');
        Reader.name = Loaded.Name.Substring(0, LastDot);
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
        if (Reader) {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), Reader.Progress);
            return;
        }
        Picker.OnGUI(1);
    }

    void Update() {
        if (ScoreCollector.Instance)
            SetMenuVisibility(false);
        else if (MenuCanvas.enabled == Picker.Open)
            SetMenuVisibility(!Picker.Open);
        Transform Cam = Camera.main.transform;
        float Timing = Time.deltaTime * 5;
        Cam.position = Vector3.Lerp(Cam.position, ScoreCollector.Instance ? IngamePosition.position : MenuPosition.position, Timing);
        Cam.rotation = Quaternion.Lerp(Cam.rotation, ScoreCollector.Instance ? IngamePosition.rotation : MenuPosition.rotation, Timing);
        if (Input.GetKeyDown(KeyCode.Escape)) // TODO: pause menu
            EndSong();
    }

    void OnDestroy() {
        PlayerPrefs.SetString("Folder", Picker.Folder);
        PlayerPrefs.Save();
    }
}
