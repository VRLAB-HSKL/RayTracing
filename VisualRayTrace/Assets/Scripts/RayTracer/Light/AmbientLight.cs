using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientLight : AbstractLight
{
    private float _ls;
    private Color _color;

    public AmbientLight() : base()
    {
        _ls = 1f;
        _color = Color.white;

    }

    public AmbientLight(float ls, Color color) : base()
    {
        _ls = ls;
        _color = color;
    }

    public override Vector3 GetDirection(RaycastHit hit)
    {
        return Vector3.zero;
    }

    public override Color L(RaycastHit hit)
    {
        return _ls * _color;
    }
}
