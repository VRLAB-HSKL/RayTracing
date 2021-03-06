using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientOccluder : AbstractLight
{
    private Vector3 _u = new Vector3();
    private Vector3 _v = new Vector3();
    private Vector3 _w = new Vector3();

    private float _ls;
    public float RadianceFactor { get => _ls; set { _ls = value; } }
    public Color LightColor { get; set; }

    private AbstractSampler _sampler;
    private Color _minAmount;

    public AmbientOccluder(AbstractSampler sampler, Color minAmount)
    {
        _sampler = sampler;
        _minAmount = minAmount;

        _sampler.MapSamplesToHemisphere(1f);

        _ls = 1f;
        LightColor = Color.white;
    }

    public AmbientOccluder(float ls, Color color, AbstractSampler sampler, Color minAmount)
    {
        _ls = ls;
        LightColor = color;
        _sampler = sampler;
        _minAmount = minAmount;

        _sampler.MapSamplesToHemisphere(1f);
    }


    public override Vector3 GetDirection(RaycastHit hit)
    {
        Vector3 sp = _sampler.SampleHemisphere();
        return sp.x * _u + sp.y * _v + sp.z * _w;
    }

    public override bool InShadow(Ray ray, RaycastHit hit)
    {
        if (Physics.Raycast(ray, out RaycastHit tmpHit, RayTraceUtility.Raycast_Distance , RayTraceUtility.LayerMask))
        {            
            return true;
        }

        return false;
    }

    public override Color L(RaycastHit hit)
    {
        _w = hit.normal;

        Vector3 wCrossUp = Vector3.Cross(_w, Vector3.up);
        _v = wCrossUp / Vector3.Magnitude(wCrossUp);

        _u = Vector3.Cross(_v, _w);

        // Jitter up vector in case normal is vertical
        _v.x = Mathf.Pow(_w.x, 0.0072f);
        _v.y = Mathf.Pow(_w.y, 1f);
        _v.z = Mathf.Pow(_w.z, 0.0034f);
        _v.Normalize();

        _u.x = Mathf.Pow(_v.x, _w.x);
        _u.y = Mathf.Pow(_v.y, _w.y);
        _u.z = Mathf.Pow(_v.z, _w.z);

        Ray shadowRay = new Ray(hit.point, GetDirection(hit));

        if(InShadow(shadowRay, hit))
        {
            return _minAmount * _ls * LightColor;
        }
        else
        {
            return _ls * LightColor;
        }
    }
}
