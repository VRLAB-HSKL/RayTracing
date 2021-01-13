using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FresnelReflectorBRDF : AbstractBRDF
{
    public float EtaIn { get; set; }
    public float EtaOut { get; set; }

    private float _kr; // reflection coefficient
    public float ReflectionCoefficient { get => _kr; set { _kr = value; } }
    public Color ReflectionColor { get; set; } // the reflection color

    public float Fresnel(RaycastHit hit, Vector3 direction)
    {
        Vector3 normal = hit.normal;
        float ndotd = Vector3.Dot(-normal, direction);
        float eta = 0f;

        if(ndotd < 0f)
        {
            normal = -normal;
            eta = EtaOut / EtaIn;
        }
        else
        {
            eta = EtaIn / EtaOut;
        }

        float cos_theta_i = Vector3.Dot(-normal, direction);
        float temp = 1f - (1f - cos_theta_i * cos_theta_i) / (eta * eta);

        float cos_theta_t = Mathf.Sqrt(1f - (1f - cos_theta_i * cos_theta_i) / (eta * eta));
        float r_parallel = (eta * cos_theta_i - cos_theta_t) / (eta * cos_theta_i + cos_theta_t);
        float r_perpendicular = (cos_theta_i - eta * cos_theta_t) / (cos_theta_i + eta * cos_theta_t);
        float kr = 0.5f * (r_parallel * r_parallel + r_perpendicular * r_perpendicular);

        return kr;
    }

    public override Color F(RaycastHit hit, Vector3 wo, Vector3 wi)
    {
        throw new System.NotImplementedException();
    }

    public override Color Rho(RaycastHit hit, Vector3 wo)
    {
        throw new System.NotImplementedException();
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi)
    {
        // ToDo: Make sure this dot product is correct
        float ndotwo = Vector3.Dot(hit.normal, wo);
        wi = -wo + 2f * hit.normal * ndotwo;
        // ToDo: Make sure this dot product is correct
        return (_kr * ReflectionColor / Mathf.Abs(Vector3.Dot(hit.normal, wi)));
        // why is this fabs? 
        // kr would be a Fresnel term in a Fresnel reflector
        // for transparency when ray hits inside surface?, if so it should go in Chapter 24
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf)
    {
        throw new System.NotImplementedException();
    }
}
