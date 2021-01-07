using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastTracer : AbstractTracer
{
    private float _maxDist;
    private int _layerMask;
    private Color _bgColor;

    public RayCastTracer()
    {
        _maxDist = 30f;
        _layerMask = ~(1 << 9);
        _bgColor = Color.black;
    }

    public RayCastTracer(float maxDist, int layerMask, Color bgColor)
    {
        _maxDist = maxDist;
        _layerMask = layerMask;
        _bgColor = bgColor;
    }


    public override Color TraceRay(Ray ray)
    {
        return TraceRay(ray, 0);

        //throw new System.NotImplementedException();       
    }

    public override Color TraceRay(Ray ray, int depth)
    {
        // ToDo: Add recursion using depth parameter

        if(Physics.Raycast(ray, out RaycastHit hitInfo, _maxDist, _layerMask))
        {
            // ToDo: Start shading

            return Color.white;
        }
        else
        {
            return _bgColor;
        }
    }

    public override Color TraceRay(Ray ray, float tmin, int depth)
    {
        throw new System.NotImplementedException();
    }
}
