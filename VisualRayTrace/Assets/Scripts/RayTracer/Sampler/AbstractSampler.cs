using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSampler
{
    protected int _numSamples;
    protected int _numSets;
    protected List<Vector2> _samples;
    protected List<int> _shuffeledIndices;
    protected int _count; //ulong _count;
    protected int _jump;

    protected List<Vector2> _diskSamples;
    protected List<Vector3> _hemisphereSamples;

    protected float _hStep;
    protected float _vStep;

    protected AbstractSampler(int numSamples, int numSets, float hStep, float vStep)
    {
        _numSamples = numSamples;
        _numSets = numSets;
        _samples = new List<Vector2>(numSamples * numSets);
        _count = 0;
        _jump = 0;


        _hStep = hStep;
        _vStep = vStep;

        SetupShuffledIndices();
    }

    public abstract void GenerateSamples();

    public void SetupShuffledIndices()
    {
        _shuffeledIndices = new List<int>(_numSamples * _numSets);
        List<int> indices = new List<int>();

        for (int j = 0; j < _numSamples; j++)
            indices.Add(j);

        for (int p = 0; p < _numSets; p++)
        {
            indices = Shuffle(indices);

            for (int j = 0; j < _numSamples; j++)
                _shuffeledIndices.Add(indices[j]);
        }
    }

    public void ShuffleSamples()
    {
        throw new NotImplementedException();
    }


    public void MapSamplesToUnitDisk()
    {
        int size = _samples.Count;

        float r = 0f, phi = 0f; // Polar coordinates
        Vector2 sp; // Sample point on unit disk

        _diskSamples = new List<Vector2>(size);

        for(int j = 0; j < size; ++j)
        {
            // Map sample point to [-1, 1] [-1, 1]
            sp.x = 2.0f * _samples[j].x - 1f;
            sp.y = 2.0f * _samples[j].y - 1f;

            // Sectors 1 and 2
            if(sp.x > -sp.y)
            {
                // Sector 1
                if(sp.x > sp.y)
                {
                    r = sp.x;
                    phi = sp.y / sp.x;
                }
                // Sector 2
                else
                {
                    r = sp.y;
                    phi = 2f - sp.x / sp.y;
                }
            }
            // Secots 3 and 4
            else
            {
                // Sector 3
                if(sp.x < sp.y)
                {
                    r = -sp.x;
                    phi = 4f + sp.y / sp.x;
                }
                // Sector 4
                else
                {
                    r = -sp.y;

                    // Avoid division by zero at origin
                    if(sp.y != 0f)
                    {
                        phi = 6f - sp.x / sp.y;
                    }
                    else
                    {
                        phi = 0f;
                    }
                }
            }

            phi *= Mathf.PI / 4f;

            _diskSamples[j] = new Vector2(r * Mathf.Cos(phi), r * Mathf.Sin(phi));
        }
    }

    public void MapSamplesToHemisphere(float e)
    {
        int size = _samples.Count;
        _hemisphereSamples = new List<Vector3>(_numSamples * _numSets);

        for(int j = 0; j < size; ++j)
        {
            float cos_phi = Mathf.Cos(2f * Mathf.PI * _samples[j].x);
            float sin_phi = Mathf.Sin(2f * Mathf.PI * _samples[j].x);
            float cos_theta = Mathf.Pow(1f - _samples[j].y, 1f / (e + 1f));
            float sin_theta = Mathf.Sqrt(1f - cos_theta * cos_theta);
            float pu = sin_theta * cos_phi;
            float pv = sin_theta * sin_phi;
            float pw = cos_theta;

            _hemisphereSamples.Add(new Vector3(pu, pv, pw));
        }
    }


    public Vector2 SampleUnitSquare()
    {
        Vector2 retVec = new Vector2();

        // Use local value copy of class members to prevent OutOfRange exceptions
        // due to Unity multi-threading/parallelizing everything and calculating false values...
        int localCount = _count;
        int localJump = _jump;

        if (localCount % _numSamples == 0)
            localJump = (UnityEngine.Random.Range(0, int.MaxValue) % _numSets) * _numSamples;

        try
        {
            // Class member stil has to be incremented if we use local copy
            ++_count;

            // Original book implementation (C++)
            //return _samples[_jump + _shuffeledIndices[_jump + _count++ % _numSamples]];

            retVec = _samples[localJump + _shuffeledIndices[localJump + localCount % _numSamples]];
        }
        catch (ArgumentOutOfRangeException)
        {
            string prefix = "SampleUnitSquare - ";

            int idx01 = localJump + (localCount) % _numSamples;
            Debug.Log(prefix + "Jump: " + localJump);
            Debug.Log(prefix + "Count: " + localCount);
            Debug.Log(prefix + "ShuffIndices[Idx]: " + idx01);
            Debug.Log(prefix + "SuffIndicesRetValue: " + _shuffeledIndices[idx01]);
            Debug.Log(prefix + "ShuffeledIndicesCollSize: " + _shuffeledIndices.Count);

            int idx02 = localJump + idx01;
            Debug.Log(prefix + "SamplesColl[Idx]: " + idx02);
            Debug.Log(prefix + "SamplesCollSize: " + _samples.Count);
        }

        return retVec;
    }

    public Vector2 SampleUnitDisk()
    {
        Vector2 retVec = new Vector2();

        // Use local value copy of class members to prevent OutOfRange exceptions
        // due to Unity multi-threading/parallelizing everything and calculating false values...
        var localCount = _count;
        var localJump = _jump;

        if (localCount % _numSamples == 0)
            localJump = (UnityEngine.Random.Range(0, int.MaxValue) % _numSets) * _numSamples;

        try
        {
            // Class member stil has to be incremented if we use local copy
            ++_count;


            retVec = _diskSamples[localJump + _shuffeledIndices[localJump + localCount % _numSamples]];
        }
        catch (ArgumentOutOfRangeException)
        {
            string prefix = "SampleUnitDisk - ";

            int idx01 = localJump + (localCount) % _numSamples;
            Debug.Log(prefix + "Jump: " + localJump);
            Debug.Log(prefix + "Count: " + localCount);
            Debug.Log(prefix + "ShuffIndices[Idx]: " + idx01);
            Debug.Log(prefix + "SuffIndicesRetValue: " + _shuffeledIndices[idx01]);
            Debug.Log(prefix + "ShuffeledIndicesCollSize: " + _shuffeledIndices.Count);

            int idx02 = localJump + idx01;
            Debug.Log(prefix + "SamplesColl[Idx]: " + idx02);
            Debug.Log(prefix + "SamplesCollSize: " + _samples.Count);
        }

        return retVec;
    }

    public Vector3 SampleHemisphere()
    {
        Vector3 retVec = new Vector3();

        // Use local value copy of class members to prevent OutOfRange exceptions
        // due to Unity multi-threading/parallelizing everything and calculating false values...
        var localCount = _count;
        var localJump = _jump;

        if (localCount % _numSamples == 0)
            localJump = (UnityEngine.Random.Range(0, int.MaxValue) % _numSets) * _numSamples;

        try
        {
            // Class member stil has to be incremented if we use local copy
            ++_count;

            retVec = _hemisphereSamples[localJump + _shuffeledIndices[localJump + localCount % _numSamples]];
        }
        catch (ArgumentOutOfRangeException)
        {
            string prefix = "SampleHemisphere - ";

            int idx01 = localJump + (localCount) % _numSamples;
            Debug.Log(prefix + "Jump: " + localJump);
            Debug.Log(prefix + "Count: " + localCount);
            Debug.Log(prefix + "ShuffIndices[Idx]: " + idx01);
            Debug.Log(prefix + "SuffIndicesRetValue: " + _shuffeledIndices[idx01]);
            Debug.Log(prefix + "ShuffeledIndicesCollSize: " + _shuffeledIndices.Count);

            int idx02 = localJump + idx01;
            Debug.Log(prefix + "SamplesColl[Idx]: " + idx02);
            Debug.Log(prefix + "SamplesCollSize: " + _samples.Count);
        }

        return retVec;
    }


    // Fisher-Yates Shuffle
    // https://stackoverflow.com/a/1262619
    private static List<int> Shuffle(List<int> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }
}
