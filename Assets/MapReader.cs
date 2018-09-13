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
    public int ChunkSize = 512; // Beat detection optimization factor
    public float Decay = .005f; // Also means difficulty

    [NonSerialized] public string Progress = string.Empty;

    const string ReadyText = "Ready";

    float[] Samples;
    int Channels, SampleRate;
    Task Worker;
    List<BoxEntry> Boxes = new List<BoxEntry>();

    void Loader() {
        int SampleCount = Samples.Length, Chunks = SampleCount / ChunkSize, ChunksPerSec = SampleRate / ChunkSize;
        float[] ChunkRMS = new float[Chunks];
        Lowpass LPF = new Lowpass(SampleRate, 100);
        for (int Chunk = 0; Chunk < Chunks; ++Chunk) {
            float ThisRMS = 0;
            int ChunkStart = Chunk * ChunkSize, ChunkEnd = ChunkStart + ChunkSize;
            for (int Channel = 0; Channel < Channels; ++Channel)
                LPF.Process(Samples, ChunkStart, ChunkEnd, Channel, Channels);
            for (int Sample = ChunkStart; Sample < ChunkEnd; ++Sample)
                ThisRMS += Samples[Sample] * Samples[Sample];
            ChunkRMS[Chunk] = Mathf.Sqrt(ThisRMS / ChunkSize);
            Progress = (Chunk / (float)Chunks).ToString("0%");
        }
        int LastRMSBox = 0;
        Random Rand = new Random();
        float ToSeconds = ChunkSize / (float)SampleRate;
        for (int Chunk = 1, End = Chunks - 1; Chunk < End; ++Chunk) {
            ChunkRMS[Chunk + 1] = Mathf.Max(ChunkRMS[Chunk] - Decay * ChunksPerSec /* TODO: sec legyen */, ChunkRMS[Chunk + 1]); // Smoothing
            if (ChunkRMS[Chunk] > ChunkRMS[Chunk - 1] && ChunkRMS[Chunk] > ChunkRMS[Chunk + 1]) {
                Debug.Log(Chunk);
                int Punch = Chunk - LastRMSBox;
                LastRMSBox = Chunk;
                float PunchDelta = Punch * ToSeconds;
                if (PunchDelta > .33f) { // TODO: structures, not pure random
                    for (int i = 0; i <= PunchDelta * 4; ++i) {
                        Boxes.Add(new BoxEntry() {
                            Timestamp = Chunk * ChunkSize / (float)SampleRate,
                            Position = new Vector2((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1)
                        });
                    }
                }
            }
        }
        Progress = ReadyText;
    }

    void Start() {
        Channels = Song.channels;
        Samples = new float[Song.samples];
        Song.GetData(Samples, 0);
        SampleRate = Song.frequency;
        Worker = new Task(Loader);
        Worker.Start();
    }

    void Update() {
        if (Progress.Equals(ReadyText)) {
            CubeDispenser NewDispenser = Instantiate(Dispenser).GetComponent<CubeDispenser>();
            NewDispenser.Boxes = Boxes;
            NewDispenser.Song = Song;
            Destroy(gameObject);
        }
    }
}