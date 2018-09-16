using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Random = System.Random;

public struct BoxEntry {
    public float Timestamp;
    public Vector2 Position;
}

public class MapReader : MonoBehaviour {
    public AudioClip Song;
    public GameObject Dispenser;
    public int ChunkSize = 16384;
    public int Subchunks = 4;
    [Tooltip("Smoothed samples in each direction.")]
    public int Smoothing = 10;
    [Tooltip("Hit detection decay time. Lower values increase difficulty.")]
    public float Decay = 3;

    [NonSerialized] public string Progress = string.Empty;

    const string ReadyText = "Ready";

    float ChunkFrequency; // chunks / sec
    float[] Samples;
    int Channels, SampleRate, ChunkStep;
    Random Rand = new Random();
    Task Worker;
    List<BoxEntry> Boxes = new List<BoxEntry>();

#if UNITY_EDITOR
    float[] ChunkCache;
#endif

    float[] GetChunkRMS() {
        Progress = "Preparing chunk division";
        int SampleCount = Samples.Length;
        int Chunks = SampleCount / ChunkSize * Subchunks - (Subchunks - 1);
        float[] ChunkRMS = new float[Chunks];
        for (int Chunk = 0; Chunk < Chunks; ++Chunk) {
            float ThisRMS = 0;
            int ChunkStart = Chunk * ChunkStep, ChunkEnd = ChunkStart + ChunkSize;
            for (int Sample = ChunkStart; Sample < ChunkEnd; Sample += Channels) // TODO: handle multichannel properly, left / right structures
                ThisRMS += Samples[Sample] * Samples[Sample];
            ChunkRMS[Chunk] = Mathf.Sqrt(ThisRMS / ChunkSize);
            Progress = "Chunk division: " + (Chunk / (float)Chunks).ToString("0%");
        }
        return ChunkRMS;
    }

    float[] Smooth(float[] Input) {
        Progress = "Smoothing";
        float[] Smoothed = new float[Input.Length];
        float SmoothDiv = 1f / (2 * Smoothing + 1);
        for (int i = Smoothing, e = Input.Length - Smoothing - 1; i < e; ++i) {
            for (int j = i - Smoothing, f = i + Smoothing; j <= f; ++j)
                Smoothed[i] += Input[j];
            Smoothed[i] *= SmoothDiv;
        }
        return Smoothed;
    }

    void AddBox(float Timestamp) {
        Boxes.Add(new BoxEntry() {
            Timestamp = Timestamp,
            Position = new Vector2((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1)
        });
    }

    void Loader() {
        ChunkFrequency = SampleRate * Subchunks / (float)ChunkSize;
        float[] ChunkRMS = Smooth(GetChunkRMS());
#if UNITY_EDITOR
        ChunkCache = ChunkRMS;
#endif
        int LastRMSBox = 0;
        float ToSeconds = ChunkStep / (float)SampleRate;
        for (int Chunk = 1, End = ChunkRMS.Length - 1; Chunk < End; ++Chunk) {
            ChunkRMS[Chunk + 1] = Mathf.Max(ChunkRMS[Chunk] - Decay / ChunkFrequency, ChunkRMS[Chunk + 1]); // Smoothing
            if (ChunkRMS[Chunk] > ChunkRMS[Chunk - 1] && ChunkRMS[Chunk] > ChunkRMS[Chunk + 1]) {
                int Punch = Chunk - LastRMSBox;
                LastRMSBox = Chunk;
                float PunchDelta = Punch * ToSeconds;
                if (PunchDelta > .1f) { // TODO: structures, not pure random
                    float Timestamp = Chunk * ChunkStep / (float)SampleRate;
                    AddBox(Timestamp);
                    if (PunchDelta > .5f) AddBox(Timestamp);
                }
            }
        }
        Progress = ReadyText;
    }

    void Start() {
        ChunkStep = ChunkSize / Subchunks;
        Channels = Song.channels;
        Samples = new float[Song.samples * Channels];
        Song.GetData(Samples, 0);
        SampleRate = Song.frequency * Channels; // TODO: handle channels properly
        Worker = new Task(Loader);
        Worker.Start();
    }

#if UNITY_EDITOR
    bool Spawned = false;
    float Playtime = 0;

    void OnGUI() {
        if (ChunkCache == null)
            return;
        Texture2D Hue = new Texture2D(1, 1);
        int LastHSVTick = -1;
        int PlayPos = Mathf.RoundToInt(Playtime * ChunkFrequency);
        for (int i = PlayPos, e = Math.Min(ChunkCache.Length, PlayPos + 250); i < e; ++i) {
            int HSVTick = Mathf.FloorToInt(i / ChunkFrequency);
            if (HSVTick != LastHSVTick) {
                Hue.SetPixel(0, 0, Color.HSVToRGB((LastHSVTick = HSVTick) * .2f % 1, 1, 1));
                Hue.Apply();
            }
            GUI.DrawTexture(new Rect(i - PlayPos, Screen.height - Mathf.Round(Screen.height * ChunkCache[i]), 1, 1), Hue);
        }
        Destroy(Hue);
    }
#endif

    void Update() {
#if UNITY_EDITOR
        if (Spawned)
            Playtime += Time.deltaTime;
        if (!Spawned && Progress.Equals(ReadyText)) {
            Spawned = true;
#else
        if (Progress.Equals(ReadyText)) {
#endif
            CubeDispenser NewDispenser = Instantiate(Dispenser).GetComponent<CubeDispenser>();
            NewDispenser.Boxes = Boxes;
            NewDispenser.Song = Song;
#if !UNITY_EDITOR
            Destroy(gameObject);
#endif
        }
    }
}