using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper class to encapsulate ViewPort information and calculation functions
/// </summary>
public class ViewPortPlaneInformation
{
    /// <summary>
    /// Renderer object of the viewport plane
    /// </summary>
    public Renderer PlaneRenderer { get; set; }

    public Vector3[] PlaneBorderPoints { get; set; }

    public Vector3 PlaneXAxis { get; set; }
    public Vector3 PlaneYAxis { get; set; }

    /// <summary>
    /// Direction vector from ray origin to ViewPort
    /// </summary>
    public Vector3 DirectionVector { get; set; }

    /// <summary>
    /// Distance between two pixels on the vertical axis
    /// </summary>
    public float VerticalIterationStep { get; set; }

    /// <summary>
    /// Distance between two pixels on the horizontal axis
    /// </summary>
    public float HorizontalIterationStep { get; set; }

    /// <summary>
    /// Argument constructor.
    /// Initializes object based on parameter values
    /// </summary>
    /// <param name="viewPortPlane">Plane gameobject</param>
    /// <param name="rayOrigin">Origin point of rays</param>
    /// <param name="width">Width of the viewport plane (ingame unity distance)</param>
    /// <param name="height">Height of the viewport plane (ingame unity distance)</param>
    public ViewPortPlaneInformation(GameObject viewPortPlane, Transform rayOrigin, float width, float height)
    {
        // Set rendere of associated plane
        PlaneRenderer = viewPortPlane.GetComponent<MeshRenderer>();



        //float maxRotation = 90f;

        //float rotationStep = 1f / maxRotation; 
        //float planeYrotation = viewPortPlane.transform.rotation.y;
        //float zPart = (planeYrotation % maxRotation) * rotationStep;
        //float xPart = (maxRotation - zPart) * rotationStep;


        float xDirection = (viewPortPlane.transform.position.x - rayOrigin.position.x);
        float yDirection = (viewPortPlane.transform.position.y - rayOrigin.position.y) - height * 0.5f;
        float zDirection = (viewPortPlane.transform.position.z - rayOrigin.position.z) + width * 0.5f;

        // Initialize default direction vector from rayorigin to the center of the viewport plane
        //DirectionVector = new Vector3(
        //        rayOrigin.right.x,
        //        rayOrigin.right.y - height * 0.5f,
        //        rayOrigin.right.z + width * 0.5f 
        //);
        // Calculate direction vector to lower left corner of viewport
        DirectionVector = new Vector3(
            xDirection, // * xPart,
            yDirection,
            zDirection // * zPart
        );

        //Debug.Log("ViewPortPlane: " + viewPortPlane.transform.position.ToString());
        //Debug.Log("RayOrigin: " + rayOrigin.position.ToString());
        //Debug.Log("DirectionVector: " + DirectionVector.ToString());

        Vector3[] planeFilter = viewPortPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
        PlaneBorderPoints = new Vector3[4];

        //Debug.Log("PlanePointsRaw: " + planeFilter.Length);


        // Lower left
        PlaneBorderPoints[0] = viewPortPlane.transform.TransformPoint(planeFilter[0]);
        // Lower right
        PlaneBorderPoints[1] = viewPortPlane.transform.TransformPoint(planeFilter[1]);
        // Upper left
        PlaneBorderPoints[2] = viewPortPlane.transform.TransformPoint(planeFilter[2]);
        // Upper right
        PlaneBorderPoints[3] = viewPortPlane.transform.TransformPoint(planeFilter[3]);
        //for(int i = 0; i < PlaneBorderPoints.Length; ++i)
        //{
        //    Debug.Log("PlaneBorderPoints[" + i + "]: " + PlaneBorderPoints[i].ToString());
        //}

        PlaneXAxis = (PlaneBorderPoints[1] - PlaneBorderPoints[0]);
        PlaneYAxis = (PlaneBorderPoints[2] - PlaneBorderPoints[0]);
    }

    /// <summary>
    /// Initialize the distances between two pixels in horizontal and vertical direction.
    /// This is used to make sure the LineRenderer always points to the current pixel that
    /// is being raytraced
    /// </summary>
    /// <param name="textureDimension"></param>
    /// <param name="planeWidth"></param>
    /// <param name="planeHeight"></param>
    public void SetIterationSteps(int textureDimension, float planeWidth, float planeHeight)
    {
        VerticalIterationStep = planeHeight / textureDimension;
        HorizontalIterationStep = planeWidth / textureDimension;

        //Debug.Log("VerticalIterationStep: " + VerticalIterationStep);
        //Debug.Log("HorizontalIterationStep: " + HorizontalIterationStep);
    }
}


