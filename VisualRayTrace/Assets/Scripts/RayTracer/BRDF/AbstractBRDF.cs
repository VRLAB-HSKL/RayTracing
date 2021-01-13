using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBRDF
{
    protected readonly static float INV_PI = 0.3183098861837906715F;

    protected AbstractSampler _sampler;

    public void SetSampler(AbstractSampler sampler)
    {
        _sampler = sampler;
    }

    public abstract Color F(RaycastHit hit, Vector3 wo, Vector3 wi);
    public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi);
    public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf);
    public abstract Color Rho(RaycastHit hit, Vector3 wo);
}
