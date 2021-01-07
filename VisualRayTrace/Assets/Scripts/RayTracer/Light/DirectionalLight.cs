using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLight : AbstractLight
{
    private float _ls;
    private Color _color;
    private Vector3 _dir;		// direction the light comes from

    public DirectionalLight(Vector3 direction)
    {
        _ls = 1f;
        _color = Color.white;
        _dir = direction;
    }

    public override Vector3 GetDirection(RaycastHit hit)
    {
        return _dir;
    }

    public override Color L(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }
}
