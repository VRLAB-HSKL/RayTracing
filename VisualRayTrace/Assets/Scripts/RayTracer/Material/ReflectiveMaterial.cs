using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectiveMaterial : PhongMaterial
{
    PerfectSpecularBRDF _reflectiveBRDF;

    public ReflectiveMaterial(Vector3 rayDir)
        : base(rayDir)
    {
        _reflectiveBRDF = new PerfectSpecularBRDF();
    }

    public void SetKR(float kr)
    {
        _reflectiveBRDF.ReflectionCoefficient = kr;
    }

    public void SetCR(Color cr)
    {
        _reflectiveBRDF.ReflectionColor = cr;
    }

    public override Color Shade(RaycastHit hit, int depth)
    {
        //Debug.Log("ReflectShade - Depth: " + depth);

        // Calculate direct illumination
        Color L = base.Shade(hit, depth);

        // Calculate reflection based color parts
        Vector3 wo = -_rayDir;
        Vector3 wi = Vector3.zero;
        Color fr = _reflectiveBRDF.SampleF(hit, wo, out wi);
        Ray reflected_ray = new Ray(hit.point, wi);

        L += fr * RayTraceUtility.GlobalWorld.Tracer.TraceRay(reflected_ray, depth + 1)
                * Vector3.Dot(hit.normal, wi);

        return L;
    }
}
