using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhongMaterial : AbstractMaterial
{
    protected Lambertian _ambientBRDF;
    protected Lambertian _diffuseBRDF;
    protected GlossySpecular _specularBRDF;

    private Vector3 _rayDir;
    private AbstractLight[] _lights;

    private AmbientLight _worldAmbientLight;

    public override Color Shade(RaycastHit hit)
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

                // ToDo: Add shadows
                //if(_lights[i])

                if(!inShadow)
                {
                    L += (_diffuseBRDF.F(hit, wo, wi) + _specularBRDF.F(hit, wo, wi)) * _lights[i].L(hit) * ndotwi;
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
