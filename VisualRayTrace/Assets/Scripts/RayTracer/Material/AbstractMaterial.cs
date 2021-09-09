using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMaterial
{
    public abstract Color Shade(RaycastHit hit, int depth);

    public abstract Color AreaLightShade(RaycastHit hit);

    public abstract Color PathShade(RaycastHit hit);
}
