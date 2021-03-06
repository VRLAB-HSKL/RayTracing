using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientLight : AbstractLight
{
    /// <summary>
    /// Radiance scaling factor (brightness)
    /// </summary>
    public float _ls;

    /// <summary>
    /// Light color
    /// </summary>
    public Color LightColor { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public AmbientLight() : base()
    {
        _ls = 1f;
        LightColor = Color.white;
    }

    /// <summary>
    /// Argument construcotr
    /// </summary>
    /// <param name="ls">Radiance scaling factor</param>
    /// <param name="color">Light color</param>
    public AmbientLight(float ls, Color color) : base()
    {
        _ls = ls;
        LightColor = color;
    }

    public override Vector3 GetDirection(RaycastHit hit)
    {
        return Vector3.zero;
    }

    public override bool InShadow(Ray ray, RaycastHit hit)
    {
        return false;
    }

    public override Color L(RaycastHit hit)
    {
        return _ls * LightColor;
    }
}
