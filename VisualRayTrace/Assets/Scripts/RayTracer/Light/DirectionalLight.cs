using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLight : AbstractLight
{
    private float _ls;
    private Color _color;
    private Vector3 _dir;		// direction the light comes from



    public override Vector3 GetDirection(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    public override Color L(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }
}
