using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LambertianBRDF : AbstractBRDF
{
    private float _kd;
    
    // Reflection coefficient
    public float ReflectionCoefficient { get => _kd; set { _kd = value; } }
    
    private Color _cd;  
    
    // Diffuse color
    public Color DiffuseColor { get => _cd; set { _cd = value; } }

    public LambertianBRDF() : base()
    {
        _kd = 0.5f;
        _cd = new Color(0f, 0f, 0f);
    }

    public override Color F(RaycastHit hit, Vector3 wo, Vector3 wi)
    {
        return _kd * _cd * INV_PI;
    }    

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi)
    {
        throw new System.NotImplementedException();
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf)
    {
        Vector3 w = hit.normal;
        Vector3 v = new Vector3(0.0034f, 1f, 0.0071f);
        v.x = Mathf.Pow(v.x, w.x);
        v.y = Mathf.Pow(v.y, w.y);
        v.z = Mathf.Pow(v.z, w.z);
        v = v.normalized;

        Vector3 u = new Vector3(Mathf.Pow(v.x, w.x), Mathf.Pow(v.y, w.y), Mathf.Pow(v.z, w.z));

        Vector3 sp = _sampler.SampleHemisphere();

        wi = sp.x * u + sp.y * v + sp.z * w;
        wi = wi.normalized;
                
        pdf = Vector3.Dot(hit.normal, wi) * INV_PI;

        return (_kd * _cd * INV_PI);
    }

    public override Color Rho(RaycastHit hit, Vector3 wo)
    {
        return _kd * _cd;
    }
}
