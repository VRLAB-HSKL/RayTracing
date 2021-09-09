using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper class to encapsulate the eye rotation functionality
/// </summary>
public class EyeRotationInformation
{
    #region Properties

    /// <summary>
    /// Position of the eye
    /// </summary>
    private Vector3 _eyePosition;

    /// <summary>
    /// Width of the viewport plane
    /// </summary>
    private float _planeWidth;

    /// <summary>
    /// Height of the viewport plane
    /// </summary>
    private float _planeHeight;

    /// <summary>
    /// Texture dimension
    /// </summary>
    private int _textureDimension;

    /// <summary>
    /// Private float used for custom getter of property
    /// </summary>
    private float? _hMaxRotation;

    /// <summary>
    /// Public readonly property representing the maximum horizontal rotation
    /// of the eye, i.e. the angle between the left and right borders of the 
    /// viewport plane
    /// </summary>
    public float HorizontalMaxRotation
    {
        get
        {
            if (_hMaxRotation is null)
            {
                float halfWidth = (_planeWidth / 2);
                Vector3 leftPlanePoint = _eyePosition;
                leftPlanePoint.x -= halfWidth;
                Vector3 rightPlanePoint = _eyePosition;
                rightPlanePoint.x += halfWidth;
                _hMaxRotation = Vector3.Angle(leftPlanePoint, rightPlanePoint);
            }

            return _hMaxRotation.Value;
        }
    }

    /// <summary>
    /// Private float used for custom getter of property
    /// </summary>
    private float? _vMaxRotation;

    /// <summary>
    /// Public readonly property representing the maximum vertical rotation
    /// of the eye, i.e. the angle between the top and bottom borders of the 
    /// viewport plane
    /// </summary>
    public float VerticalMaxRotation
    {
        get
        {
            if (_vMaxRotation is null)
            {
                float halfHeight = (_planeHeight / 2);
                var bottomPlanePoint = _eyePosition;
                bottomPlanePoint.y -= halfHeight;
                var topPlanePoint = _eyePosition;
                topPlanePoint.y += halfHeight;
                _vMaxRotation = Vector3.Angle(bottomPlanePoint, topPlanePoint);
            }

            return _vMaxRotation.Value;
        }
    }

    /// <summary>
    /// Private float used for custom getter of property
    /// </summary>
    private float? _hRotationStep;

    /// <summary>
    /// Angle between two pixels on the horizontal axis
    /// </summary>
    public float HorizontalRotationStep
    {
        get
        {
            if (_hRotationStep is null)
            {
                _hRotationStep = HorizontalMaxRotation / (float)_textureDimension;
            }

            return _hRotationStep.Value;
        }
    }

    /// <summary>
    /// Private float used for custom getter of property
    /// </summary>
    private float? _vRotationStep;

    /// <summary>
    /// Angle between two pixels on the vertical axis
    /// </summary>
    public float VerticalRotationStep
    {
        get
        {
            if (_vRotationStep is null)
            {
                _vRotationStep = VerticalMaxRotation / _textureDimension;
            }

            return _vRotationStep.Value;
        }
    }

    #endregion Properties

    /// <summary>
    /// Argument constructor
    /// </summary>
    /// <param name="eyePosition">Position of eye object</param>
    /// <param name="planeWidth">Width of viewport plane</param>
    /// <param name="planeHeight">Height of viewport plane</param>
    /// <param name="textureDimension">Texture dimension</param>
    public EyeRotationInformation(
        Vector3 eyePosition, float planeWidth,
        float planeHeight, int textureDimension)
    {
        _eyePosition = eyePosition;
        _planeWidth = planeWidth;
        _planeHeight = planeHeight;
        _textureDimension = textureDimension;
    }

    /// <summary>
    /// Query the horizontal eye rotation based on the given color array coordinate
    /// </summary>
    /// <param name="colorArrayCoordinate">Array coordinate</param>
    /// <returns>Horizontal eye rotation</returns>
    public float HorizontalEyeRotation(int colorArrayCoordinate)
    {
        return -(HorizontalMaxRotation / 2f) + colorArrayCoordinate * HorizontalRotationStep;
    }

    /// <summary>
    /// Query the vertical eye rotation based on the given color array coordinate
    /// </summary>
    /// <param name="colorArrayCoordinate">Array coordinate</param>
    /// <returns>Vertical eye rotation</returns>
    public float VerticalEyeRotation(int colorArrayCoordinate)
    {
        return -(VerticalMaxRotation / 2f) + colorArrayCoordinate * VerticalRotationStep;
    }


}
