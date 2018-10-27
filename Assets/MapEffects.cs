using Cavern;
using UnityEngine;

public class MapEffects : MonoBehaviour {
    public Color BaseColor = new Color(0, 1, 1, .1f);
    public Color BeatColor = new Color(0, .5f, 1, .75f);

    public Material Glass;
    public float KickDecay = .5f;

    float LowpassedGain = 0;
    Lowpass LPF;

    void Start() {
        Glass.color = BaseColor;
        LPF = new Lowpass(AudioListener3D.Current.SampleRate, 120);
    }

    void Update() {
        LowpassedGain -= KickDecay * Time.deltaTime;
        Glass.color = Color.Lerp(BaseColor, BeatColor, LowpassedGain);
    }

    void OnAudioFilterRead(float[] Data, int Channels) {
        int Total = Data.Length, SamplesPerChannel = Total / Channels;
        float[] Downmix = new float[SamplesPerChannel];
        for (int Channel = 0; Channel < Channels; ++Channel) {
            int ChSample = 0;
            for (int Sample = Channel; Sample < Total; Sample += Channels)
                Downmix[ChSample++] += Data[Sample];
        }
        if (LPF != null)
            LPF.Process(Downmix, 0, SamplesPerChannel);
        float Gain = 0;
        for (int Sample = 0; Sample < SamplesPerChannel; ++Sample)
            Gain += Downmix[Sample] * Downmix[Sample];
        Gain = Mathf.Sqrt(Gain / SamplesPerChannel);
        LowpassedGain = Mathf.Max(Gain, LowpassedGain);
    }
}