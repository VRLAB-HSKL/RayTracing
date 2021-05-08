using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectSpecularBRDF : AbstractBRDF
{
    private float _kr; // reflection coefficient
    
    public float ReflectionCoefficient { get => _kr; set { _kr = value; } }
    
    public Color ReflectionColor { get; set; } // the reflection color
    
    public PerfectSpecularBRDF() : base()
    {
        _kr = 0f;
        ReflectionColor = new Color(1f, 1f, 1f);
    }

    public override Color F(RaycastHit hit, Vector3 wi, Vector3 wo)
    {
        return Color.black;
    }

    public override Color Rho(RaycastHit hit, Vector3 wo)
    {
        return Color.black;
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi)
    {
        // ToDo: Make sure this dot product is correct
        float ndotwo = Vector3.Dot(hit.normal, wo);
        wi = -wo + 2f * hit.normal * ndotwo;
        // ToDo: Make sure this dot product is correct
        return (_kr * ReflectionColor / Mathf.Abs(Vector3.Dot(hit.normal,wi)));
        // why is this fabs? 
        // kr would be a Fresnel term in a Fresnel reflector
        // for transparency when ray hits inside surface?, if so it should go in Chapter 24
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf)
    {
        // ToDo: Make sure this dot product is correct
        float ndotwo = Vector3.Dot(hit.normal, wo);
        wi = -wo + 2f * hit.normal * ndotwo;
        // ToDo: Make sure this dot product is correct
        pdf = Mathf.Abs(Vector3.Dot(hit.normal, wi));
        return (_kr * ReflectionColor);
    }
}
