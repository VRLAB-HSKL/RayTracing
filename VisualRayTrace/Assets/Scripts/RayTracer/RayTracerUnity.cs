﻿
using HTC.UnityPlugin.Vive;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


/// <summary>
/// Unity raycaster implementation based on the "Raytracing in a weekend" introductory course by Peter Shirley.
/// A lot of the functionality of the C++ implementation in the course is handled by Unity itself (camera position, casting of rays, etc.)
/// so the unity engine is used to replace these functionalities. The actual raytracing and color calculation
/// of pixels on the viewport was implemented as necessary to reproduce the image output of the course. Additional
/// functionality (second screens, textures, gameobjects) have been added.
/// 
/// This script should be attached to the gameobject the rays shall originate from. 
/// </summary>
public class RayTracerUnity : MonoBehaviour
{
    #region Variables

    #region RayTracer


    /// <summary>
    /// Boolean signaling if raytracer is currently actively calculation. 
    /// Since this raytracer is either raytracing or lying dormant, a simple boolean is enough (no FSM).
    /// </summary>
    private bool isRaytracing = false;


    public bool IsRaytracing()
    {
        return isRaytracing;
    }

    /// <summary>
    /// Bitfield mask to control which objects can be hit by a ray.
    /// Currently, all objects that should be ignored by the raytracer were moved to a
    /// custom layer (IgnoreRayCast). This layer corresponds to the value 9, so we set
    /// all bits in the Int32 to 1, except for the ninth bit. This means all layers except for layer 9
    /// can be hit by rays.
    /// </summary>
    private static readonly int layerMask = ~(1 << 9);


    /// <summary>
    /// Enum to define all possible iteration modes the raytracer can be in.
    /// 
    /// Automatic: Raytracer automatically moves on to the next pixel coordinate
    /// 
    /// Single: Raytracer waits for user signal before moving on to the next pixel coordinate
    /// 
    /// </summary>
    public enum RT_IterationMode { Automatic = 0, Single = 1 };

    /// <summary>
    /// Current iteration mode of the raytracer
    /// </summary>
    [Header("Ray Tracer")]
    public RT_IterationMode IterationMode = RT_IterationMode.Automatic;

    /// <summary>
    /// Setter for the iteration mode
    /// </summary>
    /// <param name="mode">New iteration mode</param>
    public void SetIterationMode(RT_IterationMode mode)
    {
        IterationMode = mode;
    }

    public bool VisualizePath = false;

    public void SetCompleteRTPath(bool showPath)
    {
        VisualizePath = showPath;
    }

    /// <summary>
    /// Determines how far rays are shot into the scene.
    /// Objects that are farther away from the ray origin point than this range will not be hit,
    /// even if they are on the path of the ray
    /// </summary>
    [Range(20f, 100f)]
    public float RayTrace_Range = 20f;

    /// <summary>
    /// Point in the scene that rays are shot from.
    /// Currently this point is represented by a floating eye.
    /// </summary>
    private Vector3 rayOrigin;


    /// <summary>
    /// Toggles wether anti aliasing is used during raytracing. If set to true,
    /// color calulation for a single pixel is performed multiple times and the 
    /// results are averaged into a color. This prevents jagged edges on object borders
    /// by sampling the colors outside of an object to blend the borders together more smoothly.
    /// </summary>
    [Header("Anti-Aliasing")]
    public bool AnitAliasing = true;

    /// <summary>
    /// The amount of samples being taken during anti aliasing for a single pixel
    /// </summary>
    [Range(10, 200)]
    public int SampleSize = 25;

    public AASamplingStrategy SamplingMethod = AASamplingStrategy.Random;

    private AntiAliasingStrategy _aaStrategy;

    public void SetSamplingMethod(AASamplingStrategy strat)
    {
        SamplingMethod = strat;

        float hStep = _viewPortInfo.HorizontalIterationStep;
        float vStep = _viewPortInfo.VerticalIterationStep;
        _aaStrategy = new AntiAliasingStrategy(SamplingMethod, SampleSize, SampleSetCount, hStep, vStep);
    }

    /// <summary>
    /// The amount of sample sets generated
    /// </summary>
    [Range(10, 200)]
    public int SampleSetCount = 83;


    #endregion RayTracer

    #region ViewPlane    

    /// <summary>
    /// ViewPort information containing information that is calculated on start up <see cref="ViewPortPlaneInformation"/>
    /// /// </summary>
    private ViewPortPlaneInformation _viewPortInfo;


    [Header("ViewPort")]
    public ViewPortStartPoint ViewPortStart;

    private StartPointInformation _startPointSettings;

    /// <summary>
    /// Ingame height of the plane the ray is shot through to set the corresponding pixel on plane
    /// </summary>
    private float planeHeight;

    /// <summary>
    /// Ingame width of the plane the ray is shot throug h to set the corresponding pixel on plane
    /// </summary>
    private float planeWidth;

    /// <summary>
    /// The ingame plane that is used to represent the viewport in the scene.
    /// Rays are shot through the viewport to set the pixels in the viewport with the calculated colors.
    /// </summary>    
    public GameObject viewPortPlane;

    /// <summary>
    /// Collection of secondary screens the calculated texture is streamed to.
    /// This can be used to easily apply the texture that is calculated by the raytracer 
    /// to other gameobjects.
    /// </summary>
    public List<GameObject> secondScreens;

    /// <summary>
    /// Line renderer used to visualize the rays that are being shot during runtime.
    /// </summary>
    private LineRenderer visualRayLine;

    /// <summary>
    /// Wrapper object to handle rotation of the eye based on chaning ray trace direction vectors.
    /// <see cref="EyeRotationInformation"/>
    /// </summary>
    private EyeRotationInformation _eyeRotation;


    #endregion ViewPlane

    #region Texture

    /// <summary>
    /// Scale of the texture containing the pixel that are being colored with the raytracer.
    /// Each unit of scaling increases the dimension of the texutre, i. e. a scale factor of 1
    /// corresponds to a texture made up of 100x100 pixels
    /// </summary>
    public int TextureScaleFactor = 2;

    /// <summary>
    /// Texture coordinates represented by a raw array of size 2.
    /// The first int value is the horizontal coordinate (x), the second value
    /// is the vertical coordinate (y).
    /// During raytracing, these values are incremented for each iteration.    /// 
    /// </summary>
    private int[] CurrentPixel = new int[] { 0, 0 };

    public int[] GetCurrentPixel()
    {
        return CurrentPixel;
    }

    /// <summary>
    /// Information object containing texture information that is calculated on start up based on
    /// set values <see cref="TextureInformation"/>
    /// </summary>
    private TextureInformation _textureInfo;

    #endregion Texture




    #endregion Variables


    /// <summary>
    /// Start method called once on scene instantiation
    /// </summary>
    void Start()
    {
        // Line Renderer
        visualRayLine = GetComponent<LineRenderer>();
        visualRayLine.enabled = true;

        // Initialize plane and texture information
        InitPlane();
        InitTexture();

        // Calculate iteration steps based on texture dimension and target viewport plane
        _viewPortInfo.SetIterationSteps(_textureInfo.TextureDimension, planeWidth, planeHeight);

        // Setup eye rotation using parameters
        _eyeRotation = new EyeRotationInformation(
            transform.parent.transform.position,
            planeWidth, planeHeight, _textureInfo.TextureDimension);

        // Set origin of rays (object this script was assgined to)
        rayOrigin = transform.position;

        _startPointSettings = new StartPointInformation(ViewPortStart, _textureInfo.TextureDimension);
        CurrentPixel = new int[2] { _startPointSettings.InitXValue, _startPointSettings.InitYValue };

        float hStep = _viewPortInfo.HorizontalIterationStep;
        float vStep = _viewPortInfo.VerticalIterationStep;

        _aaStrategy = new AntiAliasingStrategy(SamplingMethod, SampleSize, SampleSetCount, hStep, vStep);

        _world = new WorldInformation(_viewPortInfo);
    }

    /// <summary>
    /// Update function called once per frame
    /// </summary>
    void Update()
    {
        // Check for user input
        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Menu)) // && !isRaytracing)
        {
            // If the raytracer is inactive initialize texture
            if (!isRaytracing && CurrentPixel[0] == 0 && CurrentPixel[1] == 0)
            {
                InitTexture();
            }

            // Toggle raytracing activity
            isRaytracing = !isRaytracing;
        }

        // If raytracer is raytracing, i.e. has texure coordinates left, move to next iteration
        if (isRaytracing)
        {
            for (int i = 0; i < 1; ++i)
            {
                // Start current raytracing iteration using next texture coordinate

                StartCoroutine(DoRayTraceVersion02(CurrentPixel[0], CurrentPixel[1]));
                //StartCoroutine(DoRayTraceAA(CurrentPixel[0], CurrentPixel[1]));

                visualRayLine.enabled = true;

                //Debug.Log("InitXas");

                // Increment texture coordinate for next iteration
                IncrementTexturePixelCoordinates();

                // Stay on current iteration if raytracer is in single iteration mode
                if (IterationMode == RT_IterationMode.Single) isRaytracing = false;
            }

        }
    }


    /// <summary>
    /// Initialize viewport plane information
    /// </summary>
    private void InitPlane()
    {
        //ToDo: Calculate viewport width and height dynamically
        MeshRenderer viewportRender = viewPortPlane.GetComponent<MeshRenderer>();
        planeWidth = viewportRender.bounds.size.z; //0.56f;
        planeHeight = viewportRender.bounds.size.y; //0.28f;
        //Debug.Log("Bounds.Size: " + viewportRender.bounds.size);
        //Debug.Log("planeWidth: " + planeWidth);
        //Debug.Log("planeHeight: " + planeHeight);

        // Initialize information object
        _viewPortInfo = new ViewPortPlaneInformation(viewPortPlane, transform, planeWidth, planeHeight);


        
        // Direction vector from the ray origin point to the center of the viewport plane
        //_viewPortInfo.DirectionVector = new Vector3(
        //        transform.right.x,
        //        transform.right.y - planeHeight * 0.5f, //+ x * verticalIterationStep, //((verticalIterationStep * 2) / TexWidth) );
        //        transform.right.z + planeWidth * 0.5f // - y * horizontalIterationStep //((horizontalIterationStep * 2) / TexHeight));
        //        );

    }


    /// <summary>
    /// Initialize texture 
    /// </summary>
    private void InitTexture()
    {
        // Initialize information object
        _textureInfo = new TextureInformation(TextureScaleFactor, planeWidth, planeHeight);

        // Set visual ray line width
        visualRayLine.startWidth = _textureInfo.VisualLineWidth;
        visualRayLine.endWidth = _textureInfo.VisualLineWidth;

        // Connect texture to target objects
        _viewPortInfo.PlaneRenderer.material.mainTexture = _textureInfo.StreamTexture2D;

        foreach (var obj in secondScreens)
        {
            MeshRenderer rnd = obj.GetComponent<MeshRenderer>();
            rnd.material.mainTexture = _textureInfo.StreamTexture2D;
        }

        //CurrentPixel = new int[2] {0, _textureInfo.TextureDimension };
    }




    /// <summary>
    /// Increment texture coordinate for next raytracing iteration.
    /// Traverses the texture from lower left corner to the upper right corner
    /// Note: Texture is rotated on game object by 90 degrees
    /// </summary>
    private void IncrementTexturePixelCoordinates()
    {
        // On last pixel, reset coordinates and set raytracer to inactive

        //Debug.Log("CurrPixelGreater[" + _startPointSettings.GreaterCordIdx + "] = " + CurrentPixel[_startPointSettings.GreaterCordIdx]);
        //Debug.Log("XResetValue = " + _startPointSettings.ResetXValue);
        //Debug.Log("YResetValue = " + _startPointSettings.ResetYValue);

        if (CurrentPixel[_startPointSettings.GreaterCordIdx] == (_startPointSettings.GreaterCordIdx == 0 ? _startPointSettings.ResetXValue : _startPointSettings.ResetYValue))
        {
            CurrentPixel[_startPointSettings.GreaterCordIdx] = _startPointSettings.InitXValue;
            CurrentPixel[_startPointSettings.LesserCordIdx] = _startPointSettings.InitYValue;

            isRaytracing = false;
            return;
        }

        // On reaching the highest pixel on the vertical axis, move to the next pixel column
        // and begin at the bottom
        if (CurrentPixel[_startPointSettings.LesserCordIdx] == (_startPointSettings.LesserCordIdx == 0 ? _startPointSettings.ResetXValue : _startPointSettings.ResetYValue))
        {
            CurrentPixel[_startPointSettings.GreaterCordIdx] += _startPointSettings.IncrementGreaterValue;
            CurrentPixel[_startPointSettings.LesserCordIdx] = 0;
        }
        // If top hasn't been reached, move one pixel above the last pixel
        else
        {
            CurrentPixel[_startPointSettings.LesserCordIdx] += _startPointSettings.IncrementLesserValue;
        }
    }

    /// <summary>
    /// Sets the color of a pixel in the texture
    /// </summary>
    /// <param name="x">Horizontal texture coordinate</param>
    /// <param name="y">Vertical texture coordinate</param>
    /// <param name="color">Calculated pixel color</param>
    private void SetTexturePixel(int x, int y, Color color)
    {
        // Catch invalid coordinate values
        if (x < 0 || x > _textureInfo.TextureDimension - 1) return;
        if (y < 0 || y > _textureInfo.TextureDimension - 1) return;

        // Update color array with new value,
        int index = x * _textureInfo.TextureDimension + y;
        _textureInfo.SetPixelColor(index, color);

    }


    /// <summary>
    /// Completely cancels the current raytracer calculation and resets the texture
    /// </summary>
    public void ResetRaytracer()
    {
        isRaytracing = false;
        CurrentPixel = new int[2] { 0, 0 };
        InitTexture();
    }

    /// <summary>
    /// Updates the source texture with the current pixel color values
    /// </summary>
    private void UpdateRenderTexture()
    {
        // Set active rendering texture
        RenderTexture.active = _textureInfo.Texture;

        _textureInfo.StreamTexture2D.ReadPixels(new Rect(0, 0, _textureInfo.TextureDimension, _textureInfo.TextureDimension), 0, 0);

        // Update colors array in texture by setting all pixel colors at once (faster than single SetPixel() call)
        // If this is too slow, consider using GetRawTextureData() instead
        _textureInfo.StreamTexture2D.SetPixels(_textureInfo.PixelColorData);

        // Apply changes and clear active texture
        _textureInfo.StreamTexture2D.Apply();
        RenderTexture.active = null;
    }

    /// <summary>
    /// Calculates the direction vector originating in the <see cref="rayOrigin"/> and pointing
    /// to the current texture pixel being ray traced
    /// </summary>
    /// <param name="hCord">Horizontal texture coordinate</param>
    /// <param name="vCord">Vertical texture coordinate</param>
    /// <returns>Ray direction vector</returns>
    private Vector3 CalculateRayDirectionVector(int hCord, int vCord)
    {
        // Get plane axis vectors
        Vector3 xAxisVec = _viewPortInfo.PlaneXAxis;
        Vector3 yAxisVec = _viewPortInfo.PlaneYAxis;

        //Debug.Log("XDirVec: " + xAxisVec);
        //Debug.Log("YDirVec: " + yAxisVec);

        // ToDo: Cache fraction values for most texture dimensions to prevent expensive float division
        // Calculate scale factor for plane direction vectors 
        float hScale = (float)vCord / (float)_textureInfo.TextureDimension;
        float vScale = (float)hCord / (float)_textureInfo.TextureDimension;

        //Debug.Log("TextureDimension: " + _textureInfo.TextureDimension);
        //Debug.Log("hScale: " + hScale);
        //Debug.Log("vScale: " + vScale);

        // Save scaled direction vectors
        Vector3 horizontalOffset = (xAxisVec * hScale);
        Vector3 verticalOffset = (yAxisVec * vScale);

        //Debug.Log("HOffset: " + horizontalOffset);
        //Debug.Log("VOffset: " + verticalOffset);

        // Calculate direcion vector
        Vector3 rayDir = (_viewPortInfo.PlaneBorderPoints[0] - rayOrigin) + horizontalOffset + verticalOffset;

        //Debug.Log("RayDir: " + rayDir02);

        return rayDir;
    }

    private IEnumerator DoRayTraceAA(int hCord, int vCord)
    {
        Vector3 rayDir = CalculateRayDirectionVector(hCord, vCord);

        //Debug.Log("(" + hCord + "," + vCord + "): " + rayDir);

        // Calculate direction vectors
        Vector3[] directionVectors = _aaStrategy.CreateAARays(rayDir);
        int directionRayCount = directionVectors.Length;

        //Debug.Log("DirectionRayCount: " + directionRayCount);

        // Create job collections
        NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(directionRayCount, Allocator.TempJob);
        NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(directionRayCount, Allocator.TempJob);

        for (int i = 0; i < raycastCommands.Length; ++i)
        {
            raycastCommands[i] = new RaycastCommand(rayOrigin, directionVectors[i], RayTrace_Range, layerMask);
        }

        // Add raycasts to job queue and wait for them to finish
        JobHandle raycastHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, directionRayCount, default(JobHandle));
        raycastHandle.Complete();

        List<RaycastHit> hitList = raycastHits.ToList();

        // Deallocate job collections
        raycastHits.Dispose();
        raycastCommands.Dispose();
        yield return null;

        Vector3 colorSummation = Vector3.zero;
        int validHitCounter = 0;
        for (int i = 0; i < hitList.Count(); ++i)
        {
            if (hitList[i].distance > 1e-3)
            {
                ++validHitCounter;
                Color tmpColor = DetermineHitColor(hitList[i], directionVectors[i]);
                colorSummation += new Vector3(tmpColor.r, tmpColor.g, tmpColor.b);
            }
            else
            {
                Color c = RayTraceUtility.CreateNonHitColor(directionVectors[i]);
                colorSummation += new Vector3(c.r, c.g, c.b);
            }
            yield return null;
        }

        //Debug.Log("RawColorSummation: " + colorSummation.ToString());


        // Average anti-aliasing results
        Vector3 finalColVector = colorSummation / directionRayCount; // validHitCounter;



        //Debug.Log("FinalColorVector (before gamma correction): " + finalColVector.ToString());

        //finalColVector.x = Mathf.Sqrt(finalColVector.x);
        //finalColVector.y = Mathf.Sqrt(finalColVector.y);
        //finalColVector.z = Mathf.Sqrt(finalColVector.z);
        //Color finalColor = new Color(finalColVector.x, finalColVector.y, finalColVector.z);

        Color finalColor = DisplayPixel(finalColVector);

        //Debug.Log("FinalColor: " + finalColor.ToString());

        // Set pixels on texture
        SetTexturePixel(hCord, vCord, finalColor);

        // Apply changes to texture
        UpdateRenderTexture();

        // Set line renderer point count based on settings value
        visualRayLine.positionCount = VisualizePath ? 2 + rt_rec_points.Count() : 2;

        // Begin visual line at the origin
        visualRayLine.SetPosition(0, rayOrigin);

        // Use first ray as visual representation
        Vector3 initDir = new Vector3(rayDir.x, rayDir.y, rayDir.z);

        //_viewPortInfo.DirectionVector;
        //initDir.y += hCord * _viewPortInfo.VerticalIterationStep;   //rayDir.y += x * verticalIterationStep;
        //initDir.z -= vCord * _viewPortInfo.HorizontalIterationStep;

        //Debug.Log("InitDir:" + initDir.ToString());

        Vector3 endpoint;
        if (Physics.Raycast(new Ray(rayOrigin, initDir), out RaycastHit hit, RayTrace_Range, layerMask))
        {
            endpoint = hit.point;
        }
        else
        {
            endpoint = rayOrigin + (initDir * RayTrace_Range);
        }

        visualRayLine.SetPosition(1, endpoint);

        // On full path visualization, add all points to line renderer
        if (VisualizePath)
        {
            byte counter = 2;
            foreach (Vector3 point in rt_rec_points)
            {
                visualRayLine.SetPosition(counter++, point);
            }
        }

        // ToDo: Refactor this
        // Rotate eye to face current ray target 
        transform.parent.rotation =
            Quaternion.Euler(
                new Vector3(
                    0.0f,
                    _eyeRotation.HorizontalEyeRotation(vCord),
                    _eyeRotation.VerticalEyeRotation(hCord)
                    )
         );

        //Vector3 rotationDirection = Vector3.RotateTowards(transform.parent.position, initDir, Time.deltaTime, 0.0f);
        //transform.parent.rotation = Quaternion.LookRotation(rotationDirection);

        yield return null;

    }



    // Partial Source: https://forum.unity.com/threads/trying-to-get-color-of-a-pixel-on-texture-with-raycasting.608431/
    /// <summary>
    /// Determine final pixel color based on hit object
    /// </summary>
    /// <param name="hit">Information about the raycast hit</param>
    /// <param name="direction">Direction the ray was shot in</param>
    /// <returns>Calculated pixel color</returns>
    private Color DetermineHitColor(RaycastHit hit, Vector3 direction)
    {
        // Check if the ray hit anything
        if (!(hit.collider is null))
        {
            // Material of the object that was hit
            Material mat = hit.transform.gameObject.GetComponent<MeshRenderer>().material;

            // On empty material, return error color
            if (mat is null) return Color.red; // new Color(1, 0, 0);

            // If material contains a texture, use that texture
            if (!(mat.mainTexture is null))
            {
                // Determine u,v coordinates on texture and return texture pixel color
                var texture = mat.mainTexture as Texture2D;
                Vector2 pixelUVCoords = hit.textureCoord;
                pixelUVCoords.x *= texture.width;
                pixelUVCoords.y *= texture.height;
                return texture.GetPixel(Mathf.FloorToInt(pixelUVCoords.x), Mathf.FloorToInt(pixelUVCoords.y));
            }
            else
            {
                // On raw material hit, check for type
                switch (RayTraceUtility.DetermineMaterialType(mat))
                {
                    case RayTraceUtility.MaterialType.Metal:
                        return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Metal, mat.color);

                    case RayTraceUtility.MaterialType.Dielectric:
                        return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Dielectric, mat.color);

                    default:
                    case RayTraceUtility.MaterialType.SolidColor:
                        return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.SolidColor, mat.color);
                }
            }

        }
        else
        {
            // On non-hit, return non hit color
            return RayTraceUtility.CreateNonHitColor(direction);
        }

    }

    
    private List<Vector3> rt_points = new List<Vector3>();

    /// <summary>
    /// Handles additional ray calculations based on initial material hit
    /// </summary>
    /// <param name="hit">Initial raycast hit information</param>
    /// <param name="direction">Initial raycast direction vector</param>
    /// <param name="matType">Type of material hit <see cref="MaterialType"/></param>
    /// <param name="matColor">Color of the hit material</param>
    /// <returns></returns>
    private Color HandleMaterial(RaycastHit hit, Vector3 direction, RayTraceUtility.MaterialType matType, Color matColor)
    {
        rt_points.Clear();

        // Determine if ray hit anything and discard all hits below a fixed distance
        // to prevent dark spots in final image
        if (!(hit.collider is null) && hit.distance > 1e-3f)
        {
            // Direction ray of next ray that scatters from the initial material point 
            Ray scatterRay = new Ray();

            // Attenuation 
            Vector3 attenuation = new Vector3();

            // Color values of the hit material
            Vector3 matColorVec = new Vector3(matColor.r, matColor.g, matColor.b);

            // Reconstructed initial ray
            Ray mainRay = new Ray(hit.point, direction);

            // Checks if next ray hits another object
            bool rayHit = false;

            // Determine values based on hit material type
            switch (matType)
            {

                case RayTraceUtility.MaterialType.SolidColor:
                    rayHit = RayTraceUtility.ScatterDiffuse(mainRay, hit, out attenuation, out scatterRay, matColorVec);
                    break;

                case RayTraceUtility.MaterialType.Metal:
                    rayHit = RayTraceUtility.ScatterMetal(mainRay, hit, out attenuation, out scatterRay, matColorVec);
                    break;

                case RayTraceUtility.MaterialType.Dielectric:
                    float ref_idx = 1.5f;
                    rayHit = RayTraceUtility.ScatterDielectric(mainRay, hit, out attenuation, out scatterRay, ref_idx, matColorVec);
                    break;
            }

            // Second ray hit
            if (rayHit)
            {
                // Enter recursive ray tracing
                rt_rec_points.Clear();
                Vector3 tmpColorVec = RayTrace_Recursive(scatterRay, 0);

                // Attenuate color vector
                tmpColorVec.x *= attenuation.x;
                tmpColorVec.y *= attenuation.y;
                tmpColorVec.z *= attenuation.z;

                // Initialize and return color based on vector values
                Color retColor = new Color(tmpColorVec.x, tmpColorVec.y, tmpColorVec.z);
                return retColor;
            }
            else
            {
                // On non-hit, return black color
                return new Color(0f, 0f, 0f);
            }

        }
        else
        {
            // On non-hit, return non-hit color
            return RayTraceUtility.CreateNonHitColor(direction);
        }
    }


    private List<Vector3> rt_rec_points = new List<Vector3>();

    /// <summary>
    /// Recursive raytracing used for special materials, i.e. reflection or refraction
    /// of materials
    /// </summary>
    /// <param name="ray">Next ray</param>
    /// <param name="depth">Current recursion depth</param>
    /// <returns></returns>
    private Vector3 RayTrace_Recursive(Ray ray, int depth)
    {
        // If ray hit another object and distance is above the threshold
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.distance > 1e-3)
        {
            rt_rec_points.Add(hit.point);

            // Scatter & attenuation
            Ray scatter = new Ray();
            Vector3 attenuation = Vector3.zero;

            // Get mesh renderer
            var mesh = hit.transform.gameObject.GetComponent<MeshRenderer>();
            if (mesh == null) return Vector3.zero;

            // Get material color
            Material mat = mesh.material;
            Vector3 matColVec = new Vector3(mat.color.r, mat.color.g, mat.color.b);
            bool rayHit = false;

            // Stop recursion above max depth
            if (depth < 50)
            {
                switch (RayTraceUtility.DetermineMaterialType(mat))
                {
                    case RayTraceUtility.MaterialType.Metal:
                        rayHit = RayTraceUtility.ScatterMetal(ray, hit, out attenuation, out scatter, matColVec);
                        break;

                    case RayTraceUtility.MaterialType.Dielectric:
                        float ref_idx = 1.5f;
                        rayHit = RayTraceUtility.ScatterDielectric(ray, hit, out attenuation, out scatter, ref_idx, matColVec);
                        break;

                    case RayTraceUtility.MaterialType.SolidColor:
                        rayHit = RayTraceUtility.ScatterDiffuse(ray, hit, out attenuation, out scatter, matColVec);
                        break;
                }

                if (rayHit)
                {
                    // Next recursion level

                    var tmpVec = RayTrace_Recursive(scatter, depth + 1);

                    // Apply attenuation
                    tmpVec.x *= attenuation.x;
                    tmpVec.y *= attenuation.y;
                    tmpVec.z *= attenuation.z;

                    return tmpVec;
                }
                else
                {
                    Vector3 unit_direction = Vector3.Normalize(ray.direction);
                    float t = 0.5f * (unit_direction.y + 1f);
                    return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
                }
            }
            else
            {
                Color c = RayTraceUtility.CreateNonHitColor(ray.direction);
                return new Vector3(c.r, c.g, c.b);

                //Vector3 unit_direction = Vector3.Normalize(ray.direction);
                //float t = 0.5f * (unit_direction.y + 1f);
                //return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
            }


        }
        else
        {
            Color c = RayTraceUtility.CreateNonHitColor(ray.direction);
            return new Vector3(c.r, c.g, c.b);

            //Vector3 unit_direction = Vector3.Normalize(ray.direction);
            //float t = 0.5f * (unit_direction.y + 1f);
            //return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);

        }
    }

    

    public static Color DisplayPixel(Vector3 rgb)
    {
        Color col = new Color(rgb.x, rgb.y, rgb.z);

        // Tone mapping
        // ToDo: add this to settings
        bool showOutOfGamut = true;        
        if (showOutOfGamut)
        {
            col = WorldInformation.ClampToColor(col);
        }
        else
        {
            col = WorldInformation.MaxToOne(col);
        }

        

        // Gamma correction

        // ToDo: Add this to settings / determine based on current device ?
        float gamma = 2.2f;
        float powVal = 1.0f / gamma;

        //rgb.x = Mathf.Sqrt(rgb.x);
        //rgb.y = Mathf.Sqrt(rgb.y);
        //rgb.z = Mathf.Sqrt(rgb.z);
        //Color finalColor = new Color(rgb.x, rgb.y, rgb.z);

        if (gamma != 1f)
        {
            rgb.x = Mathf.Pow(rgb.x, powVal);
            rgb.y = Mathf.Pow(rgb.y, powVal);
            rgb.z = Mathf.Pow(rgb.z, powVal);
        }

        // Integer mapping


        return col;
    }


    private WorldInformation _world;

    private IEnumerator DoRayTraceVersion02(int hCord, int vCord)
    {
        Vector3 rayDir = CalculateRayDirectionVector(hCord, vCord);

        //Debug.Log("(" + hCord + "," + vCord + "): " + rayDir);

        // Calculate direction vectors
        Vector3[] aa_DirectionVectors = _aaStrategy.CreateAARays(rayDir);
        int directionRayCount = aa_DirectionVectors.Length;

        //Debug.Log("DirectionRayCount: " + directionRayCount);

        List<RaycastHit> hitList = ShootRays(aa_DirectionVectors);

        //Debug.Log("HitList - ColLength: " + hitList.Count);

        yield return null;

        // Caclulcate pixel color
        Vector3 colorSummation = CalculatePixelColor(hitList, aa_DirectionVectors);

        //Debug.Log("RawColorSummation: " + colorSummation.ToString());


        // Average anti-aliasing results
        Vector3 finalColVector = colorSummation / directionRayCount; // validHitCounter;

        //Debug.Log("FinalColorVector (before gamma correction): " + finalColVector.ToString());

        //finalColVector.x = Mathf.Sqrt(finalColVector.x);
        //finalColVector.y = Mathf.Sqrt(finalColVector.y);
        //finalColVector.z = Mathf.Sqrt(finalColVector.z);
        //Color finalColor = new Color(finalColVector.x, finalColVector.y, finalColVector.z);

        UpdateScene(rayDir, finalColVector, hCord, vCord);

        //Vector3 rotationDirection = Vector3.RotateTowards(transform.parent.position, initDir, Time.deltaTime, 0.0f);
        //transform.parent.rotation = Quaternion.LookRotation(rotationDirection);

        yield return null;

    }

    private List<RaycastHit> ShootRays(Vector3[] directionRays)
    {
        // Create job collections
        using (NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(directionRays.Length, Allocator.TempJob))
        {
            NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(directionRays.Length, Allocator.TempJob);

            for (int i = 0; i < raycastCommands.Length; ++i)
            {
                raycastCommands[i] = new RaycastCommand(rayOrigin, directionRays[i], RayTrace_Range, layerMask);
            }

            // Add raycasts to job queue and wait for them to finish
            JobHandle raycastHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, directionRays.Length, default(JobHandle));
            raycastHandle.Complete();

            // Deallocate job collections
            //raycastHits.Dispose();
            raycastCommands.Dispose();

            return raycastHits.ToList();
        }
    }

    private Vector3 CalculatePixelColor(List<RaycastHit> hitList, Vector3[] aa_DirectionVectors)
    {
        Vector3 colorSummation = Vector3.zero;
        int validHitCounter = 0;
        for (int i = 0; i < hitList.Count(); ++i)
        {
            if (hitList[i].distance > 1e-3)
            {
                ++validHitCounter;
                Color tmpColor = DetermineHitColor(hitList[i], aa_DirectionVectors[i]);
                colorSummation += new Vector3(tmpColor.r, tmpColor.g, tmpColor.b);
            }
            else
            {
                Color c = RayTraceUtility.CreateNonHitColor(aa_DirectionVectors[i]);
                colorSummation += new Vector3(c.r, c.g, c.b);
            }

            //Debug.Log("ColorSummation - Iteration " + i + ": " + colorSummation);
            //yield return null;
        }
        return colorSummation;
    }

    private void UpdateScene(Vector3 rayDir, Vector3 finalColVector, int hCord, int vCord)
    {
        Color finalColor = DisplayPixel(finalColVector);

        //Debug.Log("FinalColor: " + finalColor.ToString());

        // Set pixels on texture
        SetTexturePixel(hCord, vCord, finalColor);

        // Apply changes to texture
        UpdateRenderTexture();

        // Set line renderer point count based on settings value
        visualRayLine.positionCount = VisualizePath ? 2 + rt_rec_points.Count() : 2;

        // Begin visual line at the origin
        visualRayLine.SetPosition(0, rayOrigin);

        // Use first ray as visual representation
        Vector3 initDir = new Vector3(rayDir.x, rayDir.y, rayDir.z);

        //_viewPortInfo.DirectionVector;
        //initDir.y += hCord * _viewPortInfo.VerticalIterationStep;   //rayDir.y += x * verticalIterationStep;
        //initDir.z -= vCord * _viewPortInfo.HorizontalIterationStep;

        //Debug.Log("InitDir:" + initDir.ToString());

        Vector3 endpoint;
        if (Physics.Raycast(new Ray(rayOrigin, initDir), out RaycastHit hit, RayTrace_Range, layerMask))
        {
            endpoint = hit.point;
        }
        else
        {
            endpoint = rayOrigin + (initDir * RayTrace_Range);
        }

        visualRayLine.SetPosition(1, endpoint);

        // On full path visualization, add all points to line renderer
        if (VisualizePath)
        {
            byte counter = 2;
            foreach (Vector3 point in rt_rec_points)
            {
                visualRayLine.SetPosition(counter++, point);
            }
        }

        // ToDo: Refactor this
        // Rotate eye to face current ray target 
        transform.parent.rotation =
            Quaternion.Euler(
                new Vector3(
                    0.0f,
                    _eyeRotation.HorizontalEyeRotation(vCord),
                    _eyeRotation.VerticalEyeRotation(hCord)
                    )
         );
    }


    public abstract class Tracer
    {
        public abstract Color TraceRay(Vector3 ray);
    }

    public class WorldInformation
    {
        public ViewPortPlaneInformation VP { get; set; }
        public AbstractTracer Tracer { get; set; } = new RayCastTracer(30f, layerMask, Color.black);

        public Color BackgroundColor { get; set; } = Color.black;

        public AmbientLight GlobalAmbientLight { get; set; } = new AmbientLight(.5f, Color.white);

        public List<AbstractLight> GlobalLights { get; set; } = new List<AbstractLight>();

        public WorldInformation(ViewPortPlaneInformation vp)
        {
            VP = vp;

            // Parse scene lights
            foreach(Light l in Resources.FindObjectsOfTypeAll(typeof(Light)) as Light[])
            {
                switch(l.type)
                {
                    case LightType.Directional:
                        
                        Vector3 lightRotationVec = l.transform.rotation.eulerAngles;
                        float x = (lightRotationVec.x / 360f);
                        float y = (lightRotationVec.y / 360f);
                        float z = (lightRotationVec.z / 360f);

                        Vector3 dirVector = new Vector3(x, y, z);
                        GlobalLights.Add(new DirectionalLight(dirVector));
                        break;

                    case LightType.Point:
                        GlobalLights.Add(new PointLight(l.intensity, l.color, l.transform.position));
                        break;
                }
            }
        }

        public static Color MaxToOne(Color c)
        {
            float maxValue = Mathf.Max(c.r, Mathf.Max(c.g, c.b));

            if(maxValue > 1f)
            {
                return c / maxValue;
            }
            else
            {
                return c;
            }
        }

        public static Color ClampToColor(Color rawColor)
        {
            Color c = new Color(rawColor.r, rawColor.g, rawColor.g);

            if(rawColor.r > 1f || rawColor.g > 1f || rawColor.b > 1f)
            {
                c.r = 1f; c.g = 0f; c.b = 0f;
            }

            return c;
        }

    }
}
