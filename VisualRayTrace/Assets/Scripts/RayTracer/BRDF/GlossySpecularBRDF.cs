using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlossySpecularBRDF : AbstractBRDF
{
    private float _ks;
    public float KS { get => _ks; set { _ks = value; } }

    private Color _cs;  // specular color
    private float _exp; // specular exponent

    public float SpecularExponent { get => _exp; set { _exp = value; } }

    public GlossySpecularBRDF() : base()
    {
        _ks = 0f;
        _cs = new Color(1f, 1f, 1f);

        _exp = 1f;
    }

    public void SetSampler(AbstractSampler sampler, float exp)
    {
        _sampler = sampler;
        _sampler.MapSamplesToHemisphere(exp);
    }

    public void SetSpecularExponent(float exp)
    {
        _exp = exp;
    }

    public void SetSamples(int numSamples, int numSteps, float hStep, float vStep, float exp)
    {
        _sampler = new MultiJitteredSampler(numSamples, numSteps, hStep, vStep);
        _sampler.MapSamplesToHemisphere(exp);
    }

    public override Color F(RaycastHit hit, Vector3 wi, Vector3 wo)
    {
        Color L = new Color();
        // ToDo: Make sure this dot product is correct
        float ndotwi = Vector3.Dot(hit.normal, wi);

        // ToDo: Refactor this !
        Vector3 r = new Vector3();
        r.x = -wi.x + 2f * hit.normal.x * ndotwi;
        r.y = -wi.y + 2f * hit.normal.y * ndotwi;
        r.z = -wi.z + 2f * hit.normal.z * ndotwi;
        
        //-wi + 2f * hit.normal * ndotwi); // mirror reflection direction
        
        // ToDo: Make sure this dot product is correct
        float rdotwo = Vector3.Dot(r,wo);

        if (rdotwo > 0.0)
        {
            L = _ks * _cs * Mathf.Pow(rdotwo, _exp);
        }            

        return L;
    }

    public override Color Rho(RaycastHit hit, Vector3 wo)
    {
        return Color.black;
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi)
    {
        throw new System.NotImplementedException();
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf)
    {
        float ndotwo = Vector3.Dot(hit.normal, wo);

        Vector3 r = -wo + 2f * hit.normal * ndotwo;     // direction of mirror reflection
        Vector3 w = r;
        Vector3 u = new Vector3(0.00424f, 1f, 0.00764f);
        u.x = Mathf.Pow(u.x, w.x);
        u.y = Mathf.Pow(u.y, w.y);
        u.z = Mathf.Pow(u.z, w.z);
        u = u.normalized;

        Vector3 v = new Vector3(Mathf.Pow(u.x, w.x), Mathf.Pow(u.y, w.y), Mathf.Pow(u.z, w.z)); // u ^ w;
        Vector3 sp = _sampler.SampleHemisphere();
        wi = sp.x * u + sp.y * v + sp.z * w;            // reflected ray direction

        // ToDo: Make sure this dot product is correct
        if (Vector3.Dot(hit.normal,wi) < 0f)                       // reflected ray is below tangent plane
            wi = -sp.x * u - sp.y * v + sp.z * w;

        // ToDo: Make sure this dot product is correct
        float phong_lobe = Mathf.Pow(Vector3.Dot(r,wi), _exp);
        pdf = phong_lobe * (Vector3.Dot(hit.normal, wi));

        return (_ks * _cs * phong_lobe);
    }
}
