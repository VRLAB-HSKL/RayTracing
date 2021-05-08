using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhongMaterial : AbstractMaterial
{
    protected LambertianBRDF _ambientBRDF;
    protected LambertianBRDF _diffuseBRDF;
    protected GlossySpecularBRDF _specularBRDF;

    protected Vector3 _rayDir;

    public void SetDir(Vector3 newDir)
    {
        _rayDir = newDir;
    }

    public PhongMaterial(Vector3 rayDir)
    {
        _ambientBRDF = new LambertianBRDF();
        _diffuseBRDF = new LambertianBRDF();
        _specularBRDF = new GlossySpecularBRDF();

        _rayDir = rayDir;
    }

    public void SetKA(float ka)
    {
        _ambientBRDF.ReflectionCoefficient = ka;
    }

    public void SetKD(float kd)
    {        
        _diffuseBRDF.ReflectionCoefficient = kd;
    }

    public void SetCD(Color cd)
    {
        _ambientBRDF.DiffuseColor = cd;
        _diffuseBRDF.DiffuseColor = cd;
    }

    public void SetKS(float ks)
    {
        _specularBRDF.KS = ks;
    }

    public void SetExp(int exp)
    {
        _specularBRDF.SpecularExponent = exp;
    }

    public override Color Shade(RaycastHit hit, int depth)
    {
        Vector3 wo = -_rayDir;
        Color L = _ambientBRDF.Rho(hit, wo) * RayTraceUtility.GlobalWorld.GlobalAmbientLight.L(hit);

        var lights = RayTraceUtility.GlobalWorld.GlobalLights;
        int numLights = lights.Count;

        for(int i = 0; i < numLights; ++i)
        {
            Vector3 wi = lights[i].GetDirection(hit);

            float ndotwi = Vector3.Dot(hit.normal, wi);

            if(ndotwi > 0f)
            {
                bool inShadow = false;

                if(lights[i].CastShadows)
                {
                    Ray shadowRay = new Ray(hit.point, wi);
                    inShadow = lights[i].InShadow(shadowRay, hit);
                }

                if(!inShadow)
                {
                    var diff = _diffuseBRDF.F(hit, wo, wi);
                    var spec = _specularBRDF.F(hit, wo, wi);
                    var lambertianLight = (diff + spec);
                    var worldLights = lights[i].L(hit) * ndotwi;
                    L += lambertianLight * worldLights;
                }                
            }
        }

        return L;
    }

    public override Color AreaLightShade(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    public override Color PathShade(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    
}
