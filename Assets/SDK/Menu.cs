using System.IO;
using UnityEngine;

public class Menu : MonoBehaviour {
    public GameObject Dispenser;

    FilePicker Picker = new FilePicker();
    MapReader Reader;

    void Start() {
        Picker.Folder = PlayerPrefs.GetString("Folder", "\\");
        Picker.OnFileLoaded += StartSong;
    }

    private void StartSong(FileInfo Loaded) {
        Reader = (new GameObject()).AddComponent<MapReader>();
        Reader.Dispenser = Dispenser;
        WWW Loader = new WWW("file://" + Loaded.FullName);
        while (!Loader.isDone) ;
        Reader.Song = Loader.GetAudioClip();
    }

    void OnGUI() {
        if (Reader) {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), Reader.Progress);
            return;
        }
        if (GUI.Button(new Rect(Screen.width - 205, 5, 200, 20), "Load new song"))
            Picker.Show();
        Picker.OnGUI(1);
    }

    void OnDestroy() {
        PlayerPrefs.SetString("Folder", Picker.Folder);
        PlayerPrefs.Save();
    }
}
