using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointLight : AbstractLight
{
    private float _ls;
    private Color _color;
    private Vector3 _location;

    public PointLight() : base()
    {

    }

    public PointLight(float ls, Color color, Vector3 location) : base()
    {
        _ls = ls;
        _color = color;
        _location = location;
    }

    public override Vector3 GetDirection(RaycastHit hit)
    {
        Vector3 tmp = (_location - hit.point);

        // ToDo: Decouple "hat" function
        float length = Mathf.Sqrt(tmp.x * tmp.x + tmp.y * tmp.y + tmp.z * tmp.z);
        tmp.x /= length; 
        tmp.y /= length; 
        tmp.z /= length;

        return tmp; 
    }

    public override Color L(RaycastHit hit)
    {
        return _ls * _color;
    }
}
