using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatteMaterial : AbstractMaterial
{
    private LambertianBRDF _ambientBRDF;
    private LambertianBRDF _diffuseBRDF;

    private Vector3 _rayDir;  

    public MatteMaterial(Vector3 rayDir)
    {
        _ambientBRDF = new LambertianBRDF();
        _diffuseBRDF = new LambertianBRDF();

        _rayDir = rayDir;
    }

    /// <summary>
    /// Set ambient reflection coefficient
    /// </summary>
    /// <param name="ka">Ambient reflection coefficient</param>
    public void SetKA(float ka)
    {
        _ambientBRDF.ReflectionCoefficient = ka;
    }

    /// <summary>
    /// Set diffuse reflection coefficient
    /// </summary>
    /// <param name="kd"></param>
    public void SetKD(float kd)
    {
        _diffuseBRDF.ReflectionCoefficient = kd;
    }

    public void SetCD(Color cd)
    {
        _ambientBRDF.DiffuseColor = cd;
        _diffuseBRDF.DiffuseColor = cd;
    }

    public override Color Shade(RaycastHit hit, int depth)
    {
        Vector3 wout = -_rayDir;

        // Initialise color with ambient light 
        Color ambientRho = _ambientBRDF.Rho(hit, wout);
        //Debug.Log("MatteMaterial - AmbientRho: " + ambientRho);
        Color worldAmbientL = RayTraceUtility.GlobalWorld.GlobalAmbientLight.L(hit);
        //Debug.Log("MatteMaterial - WorldAmbientL: " + worldAmbientL);
        Color L = ambientRho * worldAmbientL;
        //Debug.Log("MatteMaterial - AfterAmbientBRDF - Color: " + L);

        var lights = RayTraceUtility.GlobalWorld.GlobalLights;

        // Iterate over global light sources and add diffuse radiance to color
        for(int i = 0; i < lights.Count; ++i)
        {
            // Get input ray direction
            Vector3 wi = lights[i].GetDirection(hit);
            
            // ToDo: Make sure this dot product is correct
            float ndotwi = Vector3.Dot(hit.normal, wi);

            if(ndotwi > 0f)
            {
                L += _diffuseBRDF.F(hit, wout, wi) * lights[i].L(hit) * ndotwi;
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
