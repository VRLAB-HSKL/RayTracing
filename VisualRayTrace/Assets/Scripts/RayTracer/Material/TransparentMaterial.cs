using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentMaterial : PhongMaterial
{
    private PerfectSpecularBRDF _reflectiveBRDF;
    private PerfectTransmitterBTDF _specularBTDF;

    public TransparentMaterial(Vector3 rayDir, 
        float ks, float exp, float ior, float kr, float kt)
        : base(rayDir)
    {
        _reflectiveBRDF = new PerfectSpecularBRDF();
        _specularBTDF = new PerfectTransmitterBTDF(rayDir);

        _specularBRDF.KS = ks;
        _specularBRDF.SetSpecularExponent(exp);
        
        _specularBTDF.IOR = ior;

        _reflectiveBRDF.ReflectionCoefficient = kr;
        _specularBTDF.KT = kt;

    }



    //public void SetReflectionCoefficient(float ks)
    //{
    //    _reflectiveBRDF.ReflectionCoefficient = ks;
    //}

    //public void SetIOR(float ior)
    //{
    //    _specularBTDF.IOR = ior;

    //}

    public override Color Shade(RaycastHit hit,int depth)
    {
        Color L = base.Shade(hit, depth);

        Vector3 wo = -_rayDir;
        Vector3 wi = Vector3.zero;
        Color fr = _reflectiveBRDF.SampleF(hit, wo, out wi);
        Ray reflectedRay = new Ray(hit.point, wi);

        if(_specularBTDF.Tir(hit))
        {
            L += RayTraceUtility.GlobalWorld.Tracer.TraceRay(reflectedRay, depth + 1);
        }
        else
        {
            Vector3 wt = Vector3.zero;
            // Compute wt
            Color ft = _specularBTDF.SampleF(hit, wo, out wt);

            Ray transmittedRay = new Ray(hit.point, wt);

            L += fr * RayTraceUtility.GlobalWorld.Tracer.TraceRay(reflectedRay, depth + 1) 
                * Mathf.Abs(Vector3.Dot(hit.normal, wi));

            L += ft * RayTraceUtility.GlobalWorld.Tracer.TraceRay(transmittedRay, depth + 1)
                * Mathf.Abs(Vector3.Dot(hit.normal, wt));
        }

        return L;
    }
}
