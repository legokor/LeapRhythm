using Cavern;
using UnityEngine;

namespace AudioProcessor {
    public class MapEffects : MonoBehaviour {
        public float EffectDecay = .5f;

        [Header("Glass painiting")]
        public Color BaseColor = new Color(0, 1, 1, .1f);
        public Color BeatColor = new Color(0, .5f, 1, .75f);
        public Material Glass;

        [Header("Particle pushing")]
        public float ColorMultiplier = 4;
        public float SpeedMultiplier = 15;
        public Material ParticleMat;
        public ParticleSystem[] Particles;

        float LastGain = 0, LastSpeed = 0;
        Highpass HPF;
        Lowpass LPF;

        void Start() {
            LPF = new Lowpass(AudioListener3D.Current.SampleRate, 120);
            HPF = new Highpass(AudioListener3D.Current.SampleRate, 2400);
        }

        void Update() {
            float Decay = EffectDecay * Time.deltaTime;
            LastGain -= Decay;
            LastSpeed -= Decay;
            if (Glass)
                Glass.color = Color.Lerp(BaseColor, BeatColor, LastGain);
            if (ParticleMat) {
                Color ParticleTarget = Color.HSVToRGB(Mathf.Lerp(.5f, 0, LastSpeed * ColorMultiplier), 1, 1);
                ParticleTarget.a = .5f;
                ParticleMat.SetColor("_TintColor", ParticleTarget);
            }
            float SpeedTarget = LastSpeed * SpeedMultiplier;
            foreach (ParticleSystem Particle in Particles) {
                ParticleSystem.MainModule Main = Particle.main;
                Main.simulationSpeed = SpeedTarget;
            }
        }

        void OnAudioFilterRead(float[] Data, int Channels) {
            int Total = Data.Length, SamplesPerChannel = Total / Channels;
            float[] Downmix = new float[SamplesPerChannel];
            for (int Channel = 0; Channel < Channels; ++Channel) {
                int ChSample = 0;
                for (int Sample = Channel; Sample < Total; Sample += Channels)
                    Downmix[ChSample++] += Data[Sample];
            }
            float[] Downmix2 = (float[])Downmix.Clone();
            if (LPF != null)
                LPF.Process(Downmix, 0, SamplesPerChannel);
            if (HPF != null)
                HPF.Process(Downmix2, 0, SamplesPerChannel);
            float Gain = 0, Speed = 0;
            for (int Sample = 0; Sample < SamplesPerChannel; ++Sample) {
                Gain += Downmix[Sample] * Downmix[Sample];
                Speed += Downmix2[Sample] * Downmix2[Sample];
            }
            LastGain = Mathf.Max(Mathf.Sqrt(Gain / SamplesPerChannel), LastGain);
            LastSpeed = Mathf.Max(Mathf.Sqrt(Speed / SamplesPerChannel), LastSpeed);
        }
    }
}