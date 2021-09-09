using UnityEngine;


public class HammersleySampler : AbstractSampler
{
    public HammersleySampler(int numSamples, int numSteps, float hStep, float vStep)
        : base(numSamples, numSteps, hStep, vStep)
    {
        GenerateSamples();
    }

    public override void GenerateSamples()
    {
        for (int p = 0; p < _numSets; p++)
            for (int j = 0; j < _numSamples; j++)
            {
                Vector2 pv = new Vector2((float)j * _hStep / (float)_numSamples, Phi(j * _vStep));
                _samples.Add(pv);
            }
    }

    private float Phi(float j)
    {
        float x = 0.0f;
        float f = _vStep * 0.5f;  //0.5f;

        while (j >= 0.5f)
        { // (j != 0){
            x += f * (float)((j * _vStep) % 2);
            j *= 0.5f; //j /= 2;
            f *= 0.5f;
        }

        return x;
    }
}

