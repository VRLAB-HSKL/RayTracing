using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhittedTracer : AbstractTracer
{
    private float _maxDist;
    private int _layerMask;
    private Color _bgColor;

    private int _maxDepth;


    public WhittedTracer(int maxDepth)
    {
        _maxDepth = maxDepth;
        _maxDist = 30f;
        _layerMask = ~(1 << 9);
        _bgColor = Color.black;
    }

    public override Color TraceRay(Ray ray)
    {
        throw new System.NotImplementedException();
    }

    public override Color TraceRay(Ray ray, int depth)
    {
        if(depth > _maxDepth)
        {
            return Color.black;
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, _maxDist, _layerMask))
            {
                // ToDo: Start shading


                return TraceRay(ray, depth + 1);
            }
            else
            {
                return _bgColor;
            }
        }
    }

    public override Color TraceRay(Ray ray, float tmin, int depth)
    {
        throw new System.NotImplementedException();
    }
}
