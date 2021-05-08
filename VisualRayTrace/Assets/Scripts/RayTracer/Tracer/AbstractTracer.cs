using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTracer
{
    public abstract Color TraceRay(Ray ray);

    public abstract Color TraceRay(Ray ray, int depth);

    public abstract Color TraceRay(Ray ray, float tmin, int depth);
}
