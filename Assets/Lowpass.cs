using UnityEngine;

/// <summary>Simple first-order lowpass filter from Cavern.</summary>
internal class Lowpass {
    private readonly float a1, a2, b1, b2;
    float x1, x2, y1, y2; // History

    public Lowpass(int SampleRate, float CenterFreq, float Q = .7071067811865475f) {
        float w0 = Mathf.PI * 2 * CenterFreq / SampleRate, Cos = Mathf.Cos(w0), Alpha = Mathf.Sin(w0) / (Q + Q), Divisor = 1 / (1 + Alpha);
        a1 = -2 * Cos * Divisor;
        a2 = (1 - Alpha) * Divisor;
        b2 = (b1 = (1 - Cos) * Divisor) * .5f;
    }

    /// <summary>Apply this filter to an array of samples. One filter should be applied to only one continuous stream of samples.</summary>
    /// <param name="Samples">Input samples</param>
    /// <param name="Channel">Channel to filter</param>
    /// <param name="Channels">Total channels</param>
    public void Process(float[] Samples, int Start, int End, int Channel = 0, int Channels = 1) {
        for (int Sample = Start + Channel; Sample < End; Sample += Channels) {
            float ThisSample = Samples[Sample];
            Samples[Sample] = b2 * (ThisSample + x2) + b1 * x1 - a1 * y1 - a2 * y2;
            y2 = y1;
            y1 = Samples[Sample];
            x2 = x1;
            x1 = ThisSample;
        }
    }
}