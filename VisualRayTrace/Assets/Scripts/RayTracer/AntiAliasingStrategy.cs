using System.Collections.Generic;
using UnityEngine;


public enum AASamplingStrategy { Regular, Random, Jittered, NRooks, MultiJittered, Hammersley }

public class AntiAliasingStrategy
{
    protected AbstractSampler _sampler;

    protected int _SampleSize;
    protected int _rootSampleSize;
    protected int _halfRootSampleSize;

    protected float _hStep;
    protected float _vStep;

    public AntiAliasingStrategy(AASamplingStrategy sampleStrat, int sampleSize, int numSteps, float hStep, float vStep)
    {
        _SampleSize = sampleSize > 0 ? sampleSize : 1;
        _rootSampleSize = (int)System.Math.Ceiling(Mathf.Sqrt((float)_SampleSize));
        _halfRootSampleSize = (int)(0.5 * _rootSampleSize);

        _hStep = hStep / (float)_rootSampleSize;
        _vStep = vStep / (float)_rootSampleSize;

        // Initialize sampler object based on parameter
        switch (sampleStrat)
        {
            case AASamplingStrategy.Regular:
                _sampler = new RegularSampler(sampleSize, numSteps, hStep, vStep);
                break;

            case AASamplingStrategy.Random:
                _sampler = new RandomSampler(sampleSize, numSteps, hStep, vStep);
                break;

            case AASamplingStrategy.Jittered:
                _sampler = new JitteredSampler(sampleSize, numSteps, hStep, vStep);
                break;

            case AASamplingStrategy.NRooks:
                _sampler = new NRooksSampler(sampleSize, numSteps, hStep, vStep);
                break;

            case AASamplingStrategy.MultiJittered:
                _sampler = new MultiJitteredSampler(sampleSize, numSteps, hStep, vStep);
                break;

            case AASamplingStrategy.Hammersley:
                _sampler = new HammersleySampler(sampleSize, numSteps, hStep, vStep);
                break;
        }


        //Debug.Log("RegularSampling - SampleSize: " + _SampleSize);
        //Debug.Log("RegularSampling - RootSampleSize: " + _rootSampleSize);
        //Debug.Log("RegularSampling - HalfRootSampleSize: " + _halfRootSampleSize);
        //Debug.Log("RegularSampling - HStep: " + _hStep);
        //Debug.Log("RegluarSampling - VStep: " + _vStep);
    }


    public Vector3[] CreateAARays(Vector3 initRay)
    {
        List<Vector3> rayList = new List<Vector3>();

        for (int i = 0; i < _SampleSize; ++i)
        {
            Vector2 offsets = _sampler.SampleUnitSquare();
            Vector3 tmpRay = new Vector3(initRay.x + offsets.x, initRay.y + offsets.y, initRay.z);
            rayList.Add(tmpRay);
        }

        //for (int x = -_halfRootSampleSize; x < _halfRootSampleSize; ++x)
        //{
        //    for (int y = -_halfRootSampleSize; y < _halfRootSampleSize; ++y)
        //    {
        //        Vector3 tmpRay =
        //            new Vector3(initRay.x + (x * _hStep), initRay.y + (y * _vStep), initRay.z);
        //        rayList.Add(tmpRay);
        //    }
        //}

        return rayList.ToArray();
    }
}
