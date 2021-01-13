using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatteMaterial : AbstractMaterial
{
    private Lambertian _ambientBRDF;
    private Lambertian _diffuseBRDF;

    private Vector3 _rayDir;
    private AbstractLight[] _lights;
    private AbstractLight _worldAmbientLight;
    

    public MatteMaterial(Vector3 rayDir, RayTraceUtility.WorldInformation world)
    {
        _ambientBRDF = new Lambertian();
        _diffuseBRDF = new Lambertian();

        _rayDir = rayDir;
        _lights = world.GlobalLights.ToArray();
        _worldAmbientLight = world.GlobalAmbientLight;
    }

    /// <summary>
    /// Set ambient reflection coefficient
    /// </summary>
    /// <param name="ka">Ambient reflection coefficient</param>
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

    public override Color Shade(RaycastHit hit, int depth)
    {
        Vector3 wout = -_rayDir;

        // Initialise color with ambient light 
        Color ambientRho = _ambientBRDF.Rho(hit, wout);
        //Debug.Log("MatteMaterial - AmbientRho: " + ambientRho);
        Color worldAmbientL = _worldAmbientLight.L(hit);
        //Debug.Log("MatteMaterial - WorldAmbientL: " + worldAmbientL);
        Color L = ambientRho * worldAmbientL;
        //Debug.Log("MatteMaterial - AfterAmbientBRDF - Color: " + L);

        // Iterate over global light sources and add diffuse radiance to color
        for(int i = 0; i < _lights.Length; ++i)
        {
            // Get input ray direction
            Vector3 wi = _lights[i].GetDirection(hit);
            
            // ToDo: Make sure this dot product is correct
            float ndotwi = Vector3.Dot(hit.normal, wi);

            if(ndotwi > 0f)
            {
                L += _diffuseBRDF.F(hit, wout, wi) * _lights[i].L(hit) * ndotwi;
            }
        }
        //Debug.Log("MatteMaterial - AfterDiffuseBRDF - Color: " + L);

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
