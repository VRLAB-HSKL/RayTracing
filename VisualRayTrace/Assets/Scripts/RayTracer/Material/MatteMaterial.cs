using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatteMaterial : AbstractMaterial
{
    private Lambertian _ambientBRDF;
    private Lambertian _diffuseBRDF;

    private Vector3 _rayDir;
    private AbstractLight[] _lights;

    private AmbientLight _worldAmbientLight;

    public MatteMaterial(Vector3 rayDir, AbstractLight[] lights, AmbientLight worldAmbientLight)
    {
        _ambientBRDF = new Lambertian();
        _diffuseBRDF = new Lambertian();

        _rayDir = rayDir;
        _lights = lights;
        _worldAmbientLight = worldAmbientLight;
    }

    public void SetKA(float ka)
    {
        _ambientBRDF.KD = ka;
    }

    public void SetKD(float kd)
    {
        _ambientBRDF.KD = kd;
    }

    public void SetCD(Color cd)
    {
        _ambientBRDF.CD = cd;
        _diffuseBRDF.CD = cd;
    }

    public override Color Shade(RaycastHit hit)
    {
        Vector3 wout = -_rayDir;

        Color L = _ambientBRDF.Rho(hit, wout) * _worldAmbientLight.L(hit);
        int numLights = _lights.Length;

        for(int i = 0; i < numLights; ++i)
        {
            Vector3 wi = _lights[i].GetDirection(hit);
            
            // ToDo: Make sure this dot product is correct
            float ndotwi = Vector3.Dot(hit.normal, wi);

            if(ndotwi > 0f)
            {
                L += _diffuseBRDF.F(hit, wout, wi) * _lights[i].L(hit) * ndotwi;
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
