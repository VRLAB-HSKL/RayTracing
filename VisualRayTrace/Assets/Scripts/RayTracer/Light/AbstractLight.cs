using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractLight 
{
    public bool CastShadows { get; set; } = true;

    public abstract Vector3 GetDirection(RaycastHit hit);
    public abstract Color L(RaycastHit hit);
    public abstract bool InShadow(Ray ray, RaycastHit hit);
}
