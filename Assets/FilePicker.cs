using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FilePicker {
    public bool Open = false;
    public bool OfferOpenAll = false;
    public Rect Position;
    public List<string> FileFormats = new List<string>(new string[] { ".wav", ".ogg" });

    public string Folder {
        get { return FullLocation; }
        set { FullLocation = value; CacheFolder(value); }
    }

    public delegate void FileLoadHandler(FileInfo Loaded);
    public event FileLoadHandler OnFileLoaded;

    bool TreeTop;
    DirectoryInfo[] Folders;
    List<FileInfo> Files;
    string Location = "\\", FullLocation = "\\";
    Vector2 PickerScroll;

    void CacheFolder(string Path) {
        if (!Directory.Exists(Path))
            Path = "\\";
        TreeTop = Path == "\\";
        if (TreeTop) {
            Location = "Drives";
            string[] Drives = Directory.GetLogicalDrives();
            Folders = new DirectoryInfo[Drives.Length];
            for (int i = 0; i < Drives.Length; i++)
                Folders[i] = new DirectoryInfo(Drives[i]);
            Files = new List<FileInfo>();
        } else {
            DirectoryInfo Info = new DirectoryInfo(Path);
            Location = Info.Name;
            FullLocation = Info.FullName;
            Folders = Info.GetDirectories();
            FileInfo[] AllFiles = Info.GetFiles();
            Files = new List<FileInfo>();
            foreach (FileInfo Entry in AllFiles) {
                IEnumerator<string> Format = FileFormats.GetEnumerator();
                while (Format.MoveNext())
                    if (Entry.Name.ToLower().EndsWith(Format.Current))
                        Files.Add(Entry);
            }
        }
        PickerScroll = new Vector2(0, 0);
    }

    public void Show(bool KeepLocation = true) {
        Position = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 400);
        CacheFolder(KeepLocation ? FullLocation : "\\");
        Open = true;
    }

    public void Toggle(bool KeepLocation = true) {
        if (Open)
            Open = false;
        else
            Show(KeepLocation);
    }

    void WindowTick(int ID) {
        GUI.color = Color.red;
        if (GUI.Button(new Rect(285, 5, 10, 10), ""))
            Open = false;
        GUI.color = Color.white;
        bool OfferOpenAll = this.OfferOpenAll && Location != "Drives";
        int Top = -20, ListHeight = (Folders.Length + Files.Count + (OfferOpenAll ? 2 : 0) + (TreeTop ? 0 : 2) + (Files.Count > 0 ? 1 : 0)) * 20,
            ButtonWidth = ListHeight > 375 ? 272 : 290;
        TextAnchor OldAlign = GUI.skin.button.alignment;
        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        PickerScroll = GUI.BeginScrollView(new Rect(5, 20, 290, 375), PickerScroll, new Rect(0, 0, ButtonWidth, ListHeight));
        if (!TreeTop && GUI.Button(new Rect(0, Top += 20, ButtonWidth, 20), "Top"))
            CacheFolder("\\");
        if (!TreeTop && GUI.Button(new Rect(0, Top += 20, ButtonWidth, 20), "Up"))
            CacheFolder(Location.Length == 3 ? "\\" : Directory.GetParent(FullLocation).FullName);
        for (int i = 0; i < Folders.Length; i++)
            if (GUI.Button(new Rect(0, Top += 20, ButtonWidth, 20), Folders[i].Name))
                CacheFolder(Folders[i].FullName);
        if (OfferOpenAll && GUI.Button(new Rect(0, Top += 40, ButtonWidth, 20), "Open all files")) {
            IEnumerator<FileInfo> Enumerator = Files.GetEnumerator();
            while (Enumerator.MoveNext())
                OnFileLoaded?.Invoke(Enumerator.Current);
            Open = false;
        }
        Top += 20;
        IEnumerator<FileInfo> Enumer = Files.GetEnumerator();
        while (Enumer.MoveNext()) {
            FileInfo CurFile = Enumer.Current;
            if (GUI.Button(new Rect(0, Top += 20, ButtonWidth, 20), CurFile.Name)) {
                OnFileLoaded?.Invoke(CurFile);
                Open = false;
            }
        }
        GUI.skin.button.alignment = OldAlign;
        GUI.EndScrollView();
        GUI.DragWindow();
    }

    public void OnGUI(int WindowID) {
        if (Open)
            GUI.Window(WindowID, Position, WindowTick, "File Picker");
    }
}