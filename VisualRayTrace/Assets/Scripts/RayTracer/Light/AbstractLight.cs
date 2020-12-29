using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractLight 
{
    protected bool _shadows;

    public abstract Vector3 GetDirection(RaycastHit hit);
    public abstract Color L(RaycastHit hit);    
}
