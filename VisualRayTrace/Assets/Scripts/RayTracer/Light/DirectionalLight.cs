using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLight : AbstractLight
{

    private float _ls;
    public float RadianceFactor { get => _ls; set { _ls = value; } }

    private Color _color;
    private Vector3 _dir;		// direction the light comes from
    private Vector3 _location;

    public DirectionalLight(Vector3 direction, Vector3 location)
    {
        _ls = 1f;
        _color = Color.white;
        _dir = direction;
    }

    public override Vector3 GetDirection(RaycastHit hit)
    {
        return _dir;
    }

    public override bool InShadow(Ray ray, RaycastHit hit)
    {
        if (Physics.Raycast(ray, out RaycastHit tmpHit, 30, ~(1 << 9)))
        {
            float t = tmpHit.distance;
            float d = Vector3.Distance(_location, ray.origin);
            return t < d;
        }

        return false;
    }

    public override Color L(RaycastHit hit)
    {
        return _ls * _color;
    }
}
