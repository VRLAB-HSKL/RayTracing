using System;
using UnityEngine;

public class JitteredSampler : AbstractSampler
{
    public JitteredSampler(int numSamples, int numSteps, float hStep, float vStep)
        : base(numSamples, numSteps, hStep, vStep)
    {
        GenerateSamples();
    }

    public override void GenerateSamples()
    {
        int n = (int)Math.Sqrt(_numSamples);

        for (int p = 0; p < _numSets; ++p)
        {
            for (int j = 0; j < n; ++j)
            {
                for (int k = 0; k < n; ++k)
                {
                    float hRnd = UnityEngine.Random.Range(0f, _hStep - 1e-5f);
                    float vRnd = UnityEngine.Random.Range(0f, _vStep - 1e-5f);
                    //Vector2 sp = new Vector2((k *  + hRnd) / (float)n, (j + vRnd) / (float)n);
                    Vector2 sp = new Vector2((k * _hStep + hRnd) / (float)n, (j * _vStep + vRnd) / (float)n);
                    _samples.Add(sp);
                }
            }
        }
    }
}
