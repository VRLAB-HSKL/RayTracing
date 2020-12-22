using UnityEngine;

public class RandomSampler : AbstractSampler
{
    public RandomSampler(int numSamples, int numSteps, float hStep, float vStep)
        : base(numSamples, numSteps, hStep, vStep)
    {
        GenerateSamples();
    }

    public override void GenerateSamples()
    {
        for (int p = 0; p < _numSets; ++p)
        {
            for (int i = 0; i < _numSamples; ++i)
            {
                float hRnd = Random.Range(0f, _hStep - 1e-5f);
                float vRnd = Random.Range(0f, _vStep - 1e-5f);
                Vector2 sp = new Vector2(hRnd, vRnd);
                _samples.Add(sp);
            }
        }
    }
}

