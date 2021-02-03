using System;
using UnityEngine;

public class RegularSampler : AbstractSampler
{
    public RegularSampler(int numSamples, int numSteps, float hStep, float vStep)
        : base(numSamples, numSteps, hStep, vStep)
    {
        GenerateSamples();
    }

    public override void GenerateSamples()
    {
        int n = (int)Math.Sqrt((float)_numSamples);

        for (int j = 0; j < _numSets; j++)
        {
            for (int p = 0; p < n; p++)
            {
                for (int q = 0; q < n; q++)
                {
                    //_samples.Add(new Vector2((q + 0.5f) / (float)n, (p + 0.5f) / (float)n));

                    _samples.Add(new Vector2((q * _hStep) / (float)n, (p * _vStep) / (float)n));

                    //_samples.Add(new Vector2((q * (_hStep * 0.5f)) / (float)n, (p * (_vStep * 0.5f)) / (float)n));
                }
            }
        }
    }
}

