using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhongMaterial : AbstractMaterial
{
    protected Lambertian _ambientBRDF;
    protected Lambertian _diffuseBRDF;
    protected GlossySpecular _specularBRDF;

    protected Vector3 _rayDir;
    protected AbstractLight[] _lights;
    protected AbstractLight _worldAmbientLight;

    public PhongMaterial(Vector3 rayDir, RayTraceUtility.WorldInformation world)
    {
        _ambientBRDF = new Lambertian();
        _diffuseBRDF = new Lambertian();
        _specularBRDF = new GlossySpecular();

        _rayDir = rayDir;
        _lights = world.GlobalLights.ToArray();
        _worldAmbientLight = world.GlobalAmbientLight;
    }

    public void SetKA(float ka)
    {
        _ambientBRDF.KD = ka;
    }

    public void SetKD(float kd)
    {        
        _diffuseBRDF.KD = kd;
    }

    public void SetCD(Color cd)
    {
        _ambientBRDF.CD = cd;
        _diffuseBRDF.CD = cd;
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
        Color L = _ambientBRDF.Rho(hit, wo) * _worldAmbientLight.L(hit);
        int numLights = _lights.Length;

        for(int i = 0; i < numLights; ++i)
        {
            Vector3 wi = _lights[i].GetDirection(hit);

            // ToDo: Make sure this dot product is correct
            float ndotwi = Vector3.Dot(hit.normal, wi);

            if(ndotwi > 0f)
            {
                bool inShadow = false;

                if(_lights[i].CastShadows)
                {
                    Ray shadowRay = new Ray(hit.point, wi);
                    inShadow = _lights[i].InShadow(shadowRay, hit);
                }

                if(!inShadow)
                {
                    var diff = _diffuseBRDF.F(hit, wo, wi);
                    var spec = _specularBRDF.F(hit, wo, wi);
                    var lambertianLight = (diff + spec);
                    var worldLights = _lights[i].L(hit) * ndotwi;
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
