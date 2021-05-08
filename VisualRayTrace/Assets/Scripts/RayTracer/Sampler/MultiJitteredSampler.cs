using UnityEngine;

public class MultiJitteredSampler : AbstractSampler
{
    public MultiJitteredSampler(int numSamples, int numSteps, float hStep, float vStep)
        : base(numSamples, numSteps, hStep, vStep)
    {
        GenerateSamples();
    }

    /// <summary>
    /// Source: Ray tracing from the ground up - DL Code
    /// </summary>
    public override void GenerateSamples()
    {
        // _numSamples needs to be a perfect square
        int n = (int)System.Math.Sqrt((float)_numSamples);
        float subcell_width = _hStep / ((float)_numSamples);
        float subcell_height = _vStep / ((float)_numSamples);

        // Fill the samples array with dummy points to allow us to use the [ ] notation when we set the 
        // initial patterns
        Vector2 fill_point = new Vector2(0.0f, 0.0f);
        for (int j = 0; j < _numSamples * _numSets; j++)
        {
            _samples.Add(fill_point);
        }

        // distribute points in the initial patterns
        for (int p = 0; p < _numSets; p++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int currentIndex = i * n + j + p * _numSamples;
                    float x = (i * n + j) * subcell_width + Random.Range(0, subcell_width);
                    float y = (j * n + i) * subcell_height + Random.Range(0, subcell_height);
                    _samples[currentIndex].Set(x, y);
                }
            }
        }

        // shuffle x coordinates
        for (int p = 0; p < _numSets; p++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int k = Random.Range(j, n - 1);

                    int index01 = i * n + j + p * _numSamples;
                    int index02 = i * n + k + p * _numSamples;

                    float t = _samples[index01].x;
                    _samples[index01].Set(_samples[index02].x, _samples[index01].y);
                    _samples[index02].Set(t, _samples[index02].y);

                }
            }
        }

        // shuffle y coordinates

        for (int p = 0; p < _numSets; p++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int k = Random.Range(j, n - 1);

                    int index01 = j * n + i + p * _numSamples;
                    int index02 = k * n + i + p * _numSamples;

                    float t = _samples[index01].y;
                    _samples[index01].Set(_samples[index01].x, _samples[index02].y);
                    _samples[index02].Set(_samples[index02].x, t);
                }
            }
        }
    }
}
