using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBRDF
{
    /// <summary>
    /// Precalculated value of 1 / pi to prevent expensive float division in functions
    /// </summary>
    protected readonly static float INV_PI = 0.3183098861837906715F;

    /// <summary>
    /// Sampler instanced used for sampling operations
    /// </summary>
    protected static AbstractSampler _sampler =
        new MultiJitteredSampler(83, 50, RayTraceUtility.H_STEP, RayTraceUtility.V_STEP);

    /// <summary>
    /// Sampler set function
    /// </summary>
    /// <param name="sampler"></param>
    public void SetSampler(AbstractSampler sampler)
    {
        _sampler = sampler;
    }

    public abstract Color F(RaycastHit hit, Vector3 wo, Vector3 wi);
    public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi);
    public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf);
    public abstract Color Rho(RaycastHit hit, Vector3 wo);
}
