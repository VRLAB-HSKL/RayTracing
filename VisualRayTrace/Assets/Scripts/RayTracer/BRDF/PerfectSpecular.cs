using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectSpecular : AbstractBRDF
{
    private float _kr; // reflection coefficient
    private Color _cr; // the reflection color
    
    public PerfectSpecular() : base()
    {
        _kr = 0f;
        _cr = new Color(1f, 1f, 1f);
    }

    public override Color F(RaycastHit hit, Vector3 wi, Vector3 wo)
    {
        return Color.black;
    }

    public override Color Rho(RaycastHit hit, Vector3 wo)
    {
        return Color.black;
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, Vector3 wi)
    {
        // ToDo: Make sure this dot product is correct
        float ndotwo = Vector3.Dot(hit.normal, wo);
        wi = -wo + 2f * hit.normal * ndotwo;
        // ToDo: Make sure this dot product is correct
        return (_kr * _cr / Mathf.Abs(Vector3.Dot(hit.normal,wi)));
        // why is this fabs? 
        // kr would be a Fresnel term in a Fresnel reflector
        // for transparency when ray hits inside surface?, if so it should go in Chapter 24
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, Vector3 wi, out float pdf)
    {
        // ToDo: Make sure this dot product is correct
        float ndotwo = Vector3.Dot(hit.normal, wo);
        wi = -wo + 2f * hit.normal * ndotwo;
        // ToDo: Make sure this dot product is correct
        pdf = Mathf.Abs(Vector3.Dot(hit.normal, wi));
        return (_kr * _cr);
    }
}
