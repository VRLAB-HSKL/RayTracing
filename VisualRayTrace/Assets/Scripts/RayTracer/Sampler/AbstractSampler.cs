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

    public Vector2 SampleUnitSquare()
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

    // Fisher-Yates Shuffle
    private List<int> Shuffle(List<int> list)
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
