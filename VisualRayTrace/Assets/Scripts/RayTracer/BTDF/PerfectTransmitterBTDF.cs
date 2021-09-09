using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectTransmitterBTDF : AbstractBTDF
{
    private float _kt;

    public float KT { get => _kt; set { _kt = value; } }

    private float _ior;

    public float IOR { get => _ior; set { _ior = value; } }

    private Vector3 _rayDir;

    public PerfectTransmitterBTDF(Vector3 rayDir)
    {
        _rayDir = rayDir;
    }

    public override Color F(RaycastHit hit, Vector3 wo, Vector3 wi)
    {
        throw new System.NotImplementedException();
    }

    public override Color Rho(RaycastHit hit, Vector3 wo)
    {
        throw new System.NotImplementedException();
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wt)
    {
        Vector3 n = hit.normal;
        float cos_theta_i = Vector3.Dot(n, wo);
        float eta = _ior;

        if(cos_theta_i < 0f)
        {
            cos_theta_i = -cos_theta_i;
            n = -n;
            eta = (float)1f / eta;
        }

        float temp = 1f - (1f - cos_theta_i * cos_theta_i) / (eta * eta);
        float cos_theta_2 = Mathf.Sqrt(temp);
        wt = -wo / eta - (cos_theta_2 - cos_theta_i / eta) * n;

        return (_kt / (eta * eta) * Color.white / Mathf.Abs(Vector3.Dot(hit.normal, wt)));
    }

    public override Color SampleF(RaycastHit hit, Vector3 wo, Vector3 wi, out float pdf)
    {
        throw new System.NotImplementedException();
    }

    public override bool Tir(RaycastHit hit)
    {
        Vector3 wo = -_rayDir;
        float cos_theta_i = Vector3.Dot(hit.normal, wo);
        float eta = _ior;

        if(cos_theta_i < 0f)
        {
            eta = (float) (1f / eta);
        }

        return (1f - (1f - cos_theta_i * cos_theta_i) / (eta * eta)) < 0f;
    }
}
