using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DielectricMaterial : PhongMaterial
{
    // Interior filter color
    public Color CfIn { get; set; }

    // Exterior filter color
    public Color CfOut { get; set; }

    FresnelReflectorBRDF _fresnelBRDF;
    FresnelTransmitterBTDF _fresnelBTDF;

    public DielectricMaterial(Vector3 raydir, Color cfIn, Color cfOut)
        : base(raydir)
    {
        CfIn = cfIn;
        CfOut = cfOut;

        _fresnelBRDF = new FresnelReflectorBRDF();
        _fresnelBTDF = new FresnelTransmitterBTDF(raydir);
    }


    public void SetExp(float exp)
    {
        //ToDo: Implement this
        return;
    }

    public void SetEtaIn(float etaIn)
    {
        _fresnelBRDF.EtaIn = etaIn;
    }

    public void SetEtaOut(float etaOut)
    {
        _fresnelBRDF.EtaOut = etaOut;
    }




    public override Color AreaLightShade(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    public override Color PathShade(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    public override Color Shade(RaycastHit hit, int depth)
    {
        Color L = base.Shade(hit, depth);

        Vector3 wi; // = Vector3.zero;
        Vector3 wo = -_rayDir;

        // Compute wi
        Color fr = _fresnelBRDF.SampleF(hit, wo, out wi);

        Ray reflectedRay = new Ray(hit.point, wi);
        float t = 0f;
        Color Lr = new Color();
        Color Lt = new Color();
        float ndotwi = Vector3.Dot(hit.normal, wi);

        // Total inner reflection
        if(_fresnelBTDF.Tir(hit))
        {
            if(ndotwi < 0f)
            {
                // Reflected ray is inside
                Lr = RayTraceUtility.GlobalWorld.Tracer.TraceRay(reflectedRay, depth + 1);

                // Inside filter color
                L.r += Mathf.Pow(CfIn.r, t) * Lr.r;
                L.g += Mathf.Pow(CfIn.g, t) * Lr.g;
                L.b += Mathf.Pow(CfIn.b, t) * Lr.b;
            }
            else
            {
                // Reflected ray is outside
                Lr = RayTraceUtility.GlobalWorld.Tracer.TraceRay(reflectedRay, t, depth + 1);

                // kr = 1
                // Outside filter color
                L.r += Mathf.Pow(CfOut.r, t) * Lr.r;
                L.g += Mathf.Pow(CfOut.g, t) * Lr.g;
                L.b += Mathf.Pow(CfOut.b, t) * Lr.b;
            }
        }
        else // No total internal reflection
        {
            Vector3 wt = Vector3.zero;
            
            // Compute wt
            Color ft = _fresnelBTDF.SampleF(hit, wo, out wt);
            Ray transmittedRay = new Ray(hit.point, wt);
            float ndotwt = Vector3.Dot(hit.normal, wt);

            if(ndotwi < 0f)
            {
                // Reflected ray is inside
                Lr = fr * RayTraceUtility.GlobalWorld.Tracer.TraceRay(reflectedRay, t, depth + 1) * Mathf.Abs(ndotwi);
                
                // Inside filter color
                L.r += Mathf.Pow(CfIn.r, t) * Lr.r;
                L.g += Mathf.Pow(CfIn.g, t) * Lr.g;
                L.b += Mathf.Pow(CfIn.b, t) * Lr.b;

                // Transmitted ray is outside
                Lt = ft * RayTraceUtility.GlobalWorld.Tracer.TraceRay(transmittedRay, t, depth + 1) * Mathf.Abs(ndotwt);

                // Outside filter color
                L.r += Mathf.Pow(CfOut.r, t) * Lt.r;
                L.g += Mathf.Pow(CfOut.g, t) * Lt.g;
                L.b += Mathf.Pow(CfOut.b, t) * Lt.b;
            }
            else 
            {
                // Reflected ray is outside

                Lr = fr * RayTraceUtility.GlobalWorld.Tracer.TraceRay(reflectedRay, t, depth + 1) * Mathf.Abs(ndotwi);

                // Outside filter color
                L.r += Mathf.Pow(CfOut.r, t) * Lr.r;
                L.g += Mathf.Pow(CfOut.g, t) * Lr.g;
                L.b += Mathf.Pow(CfOut.b, t) * Lr.b;

                // Transmitted ray is inside
                Lt = ft * RayTraceUtility.GlobalWorld.Tracer.TraceRay(transmittedRay, t, depth + 1) * Mathf.Abs(ndotwt);

                // Inside filter color
                L.r += Mathf.Pow(CfIn.r, t) * Lt.r;
                L.g += Mathf.Pow(CfIn.g, t) * Lt.g;
                L.b += Mathf.Pow(CfIn.b, t) * Lt.b;
            }
        }

        return L;
    }
}
