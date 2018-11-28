using Cavern;
using GameModes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Random = System.Random;

namespace AudioProcessor {
    public class MapReader : MonoBehaviour {
        const bool DebugMode = true;

        public AudioClip Song;
        public GameObject Collector;
        public GameObject Dispenser;
        public int ChunkSize = 16384;
        public int Subchunks = 4;
        [Tooltip("Smoothed samples in each direction. Should only be >1 on very intense tracks.")]
        public int Smoothing = 1;
        [Tooltip("Hit detection decay rate. Higher values increase difficulty.")]
        public float Decay = .05f;
        public GameModeType Mode;

        [NonSerialized] public string Progress = string.Empty;

        /// <summary>
        /// Chunks per second.
        /// </summary>
        float ChunkFrequency;
        /// <summary>
        /// RMS value of each chunk. In debug mode, these are used for chunk display.
        /// </summary>
        float[] ChunkCache;
        float[] Samples;
        int Channels, SampleRate, ChunkStep;
        string SongName;
        Random Rand;
        Task Worker;
        List<BoxEntry> Boxes = new List<BoxEntry>();
        GameMode ModeClass;

        static ScoreCollector CollectorInstance;
        static CubeDispenser DispenserInstance;

        public static void DestroyMap() {
            if (CollectorInstance)
                Destroy(CollectorInstance.gameObject);
            if (DispenserInstance)
                Destroy(DispenserInstance.gameObject);
        }

        // Debug vars
        bool Spawned = false, WasBox = false;
        int Box = 0;

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

        void AddBox(float Timestamp, Vector2 Position) {
            BoxEntry NewEntry = new BoxEntry() {
                Timestamp = Timestamp,
                Position = Position
            };
            ModeClass.PaintBox(ref NewEntry);
            Boxes.Add(NewEntry);
        }

        void Loader() {
            int Seed = 0;
            for (int i = 0, c = SongName.Length; i < c; ++i)
                Seed += SongName[i];
            Rand = new Random(Seed);
            ChunkFrequency = SampleRate * Subchunks / (float)ChunkSize;
            ChunkCache = Smooth(GetChunkRMS());
            int LastRMSBox = 0;
            float ToSeconds = ChunkStep / (float)SampleRate, MinPunchDelta = ModeClass.MinPunchDelta, DecayPerBlock = Decay / ChunkFrequency;
            for (int Chunk = 1, End = ChunkCache.Length - 1; Chunk < End; ++Chunk) {
                int Punch = Chunk - LastRMSBox;
                ChunkCache[Chunk + 1] = Mathf.Max(ChunkCache[Chunk] - DecayPerBlock * Punch, ChunkCache[Chunk + 1]); // Smoothing
                if (ChunkCache[Chunk] > ChunkCache[Chunk - 1] && ChunkCache[Chunk] > ChunkCache[Chunk + 1]) {
                    LastRMSBox = Chunk;
                    float PunchDelta = Punch * ToSeconds;
                    if (PunchDelta > MinPunchDelta) {
                        float Timestamp = Chunk * ChunkStep / (float)SampleRate;
                        Vector2 BoxPos = new Vector2((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1);
                        if (PunchDelta > .5f) {
                            BoxPos.x = BoxPos.x * .75f + Mathf.Sign(BoxPos.x) * .25f;
                            BoxPos.y = BoxPos.y * .75f + Mathf.Sign(BoxPos.y) * .25f;
                            AddBox(Timestamp, new Vector2(-BoxPos.x, Rand.NextDouble() > .25 ? BoxPos.y : -BoxPos.y));
                        }
                        AddBox(Timestamp, BoxPos);
                    }
                }
            }
            Progress = string.Empty;
        }

        void Start() {
            ChunkStep = ChunkSize / Subchunks;
            Channels = Song.channels;
            Samples = new float[Song.samples * Channels];
            Song.GetData(Samples, 0);
            SongName = Song.name;
            SampleRate = Song.frequency * Channels;
            switch (Mode) {
                case GameModeType.Ninja:
                    ModeClass = new Ninja();
                    break;
                default:
                    ModeClass = new FreeForAll();
                    break;
            }
            Worker = new Task(Loader);
            Worker.Start();
        }

        void OnGUI() {
            if (ChunkCache == null)
                return;
            Texture2D Hue = new Texture2D(1, 1);
            int LastHSVTick = -1;
            if (WasBox) {
                Hue.SetPixel(1, 1, Color.white);
                Hue.Apply();
            }
            float Playtime = DispenserInstance.GetComponent<AudioSource3D>().time;
            int PlayPos = Mathf.RoundToInt(Playtime * ChunkFrequency);
            for (int i = PlayPos, e = Math.Min(ChunkCache.Length, PlayPos + 250); i < e; ++i) {
                if (!WasBox) {
                    int HSVTick = Mathf.FloorToInt(i / ChunkFrequency);
                    if (HSVTick != LastHSVTick) {
                        Hue.SetPixel(0, 0, Color.HSVToRGB((LastHSVTick = HSVTick) * .2f % 1, 1, 1));
                        Hue.Apply();
                    }
                }
                GUI.DrawTexture(new Rect(i - PlayPos, Screen.height - Mathf.Round(Screen.height * ChunkCache[i]), 1, 1), Hue);
            }
            Destroy(Hue);
            GUI.Label(new Rect(0, 0, 200, 25), "Playtime: " + Playtime);
            WasBox = false;
            while (Box < Boxes.Count && Boxes[Box].Timestamp < Playtime) {
                WasBox = true;
                ++Box;
            }
        }

        void Update() {
            if (!Spawned && Progress.Length == 0) {
                Spawned = true;
                if (CollectorInstance) Destroy(CollectorInstance.gameObject);
                if (DispenserInstance) Destroy(DispenserInstance.gameObject);
                CollectorInstance = Instantiate(Collector).GetComponent<ScoreCollector>();
                DispenserInstance = Instantiate(Dispenser).GetComponent<CubeDispenser>();
                DispenserInstance.Boxes = Boxes;
                DispenserInstance.Song = Song;
                if (!DebugMode)
#pragma warning disable CS0162 // Unreachable code detected
                    Destroy(gameObject);
#pragma warning restore CS0162 // Unreachable code detected
            }
        }
    }
}