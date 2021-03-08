
using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// version 1:
/// Unity raycaster implementation based on the "Raytracing in a weekend" introductory course by Peter Shirley.
/// A lot of the functionality of the C++ implementation in the course is handled by Unity itself (camera position, casting of rays, etc.)
/// so the unity engine is used to replace these functionalities. The actual raytracing and color calculation
/// of pixels on the viewport was implemented as necessary to reproduce the image output of the course. Additional
/// functionality (second screens, textures, gameobjects) have been added.
/// 
/// version 2:
/// The software component was stripped of its ray tracing functionality and now uses an external architecture
/// based on the book "Ray Tracing from the Ground Up" by Kevin Suffern. Using this new paradigm allows for
/// the ray tracing functionality to be mangaged / expanded upon in a much simpler way, without having to refactor
/// this static script.
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

    /// <summary>
    /// Returns true if object is currently ray tracing
    /// </summary>
    /// <returns>Is the game object currently ray tracing</returns>
    public bool IsRaytracing()
    {
        return isRaytracing;
    }

    /// <summary>
    /// Sets state of ray tracer (is ray tracing / is not ray tracing)
    /// </summary>
    /// <param name="isRT">New raytracing state</param>
    public void SetIsRaytracing(bool isRT)
    {
        isRaytracing = isRT;
    }

    /// <summary>
    /// Stops the ray tracing process and resets the texture
    /// </summary>
    public void StopRaytracer()
    {
        isRaytracing = false;
        CurrentPixel[0] = 0;
        CurrentPixel[1] = 0;
        _textureInfo.ResetTexture();
    }    


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

    /// <summary>
    /// Controls wether every created ray is shown (reflected rays)
    /// </summary>
    public bool VisualizePath = false;

    /// <summary>
    /// Sets path visualization
    /// </summary>
    /// <param name="showPath">parameter</param>
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
    public float RayTrace_Range; 
    //{ 
    //    get => RayTraceUtility.Raycast_Distance; 
    //    set { RayTraceUtility.Raycast_Distance = value; } 
    //}

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

    /// <summary>
    /// Update sampling method
    /// </summary>
    /// <param name="strat"></param>
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

    /// <summary>
    /// Start point related information
    /// </summary>
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

    /// <summary>
    /// Returns the current pixel 2D array
    /// </summary>
    /// <returns></returns>
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

        // Initialize static members
        RayTraceUtility.H_STEP = hStep;
        RayTraceUtility.V_STEP = vStep;
        
        RayTraceUtility.GlobalWorld = new RayTraceUtility.WorldInformation();
        RayTraceUtility.GlobalWorld.Tracer = 
            new WhittedTracer(RayTraceUtility.GlobalWorld.MaxDepth, 
                              RayTrace_Range, 
                              RayTraceUtility.LayerMask, 
                              RayTraceUtility.GlobalWorld.BackgroundColor);

        RayTraceUtility.Raycast_Distance = RayTrace_Range;
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

                StartCoroutine(DoRayTraceArchitecture(CurrentPixel[0], CurrentPixel[1]));
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
    
    
    /// <summary>
    /// Process color argument to match display
    /// </summary>
    /// <param name="rgb"></param>
    /// <returns></returns>
    public static Color ProcessColor(Vector3 rgb)
    {
        Color col = new Color(rgb.x, rgb.y, rgb.z);

        // Tone mapping
        // ToDo: add this to settings
        bool showOutOfGamut = false;        
        if (showOutOfGamut)
        {
            col = RayTraceUtility.ClampToColor(col);
        }
        else
        {
            col = RayTraceUtility.MaxToOne(col);
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


    private IEnumerator DoRayTraceArchitecture(int hCord, int vCord)
    {
        Vector3 rayDir = CalculateRayDirectionVector(hCord, vCord);

        //Debug.Log("(" + hCord + "," + vCord + "): " + rayDir);

        // Calculate direction vectors
        Vector3[] aa_DirectionVectors = _aaStrategy.CreateAARays(rayDir);
        int directionRayCount = aa_DirectionVectors.Length;

        //Debug.Log("DirectionRayCount: " + directionRayCount);

        
        Vector3 colorSummation = Vector3.zero;
        int validHitCounter = 0;
        foreach(var x in aa_DirectionVectors)
        {
            if(Physics.Raycast(new Ray(rayOrigin, x), RayTrace_Range, RayTraceUtility.LayerMask))
            {
                var tmpCol = RayTraceUtility.GlobalWorld.Tracer.TraceRay(new Ray(rayOrigin, x), 0);
                colorSummation += new Vector3(tmpCol.r, tmpCol.g, tmpCol.b);
                ++validHitCounter;
            }
            yield return null;
        }

        // Average out anti-aliasing results
        Vector3 finalColVector = colorSummation / directionRayCount;
        UpdateScene(rayDir, finalColVector, hCord, vCord);

        yield return null;

    }

   
    /// <summary>
    /// Update scene related objects
    /// </summary>
    /// <param name="rayDir">Ray direction</param>
    /// <param name="finalColVector">Calculated color vector</param>
    /// <param name="hCord">Horizontal texture coordinate</param>
    /// <param name="vCord">Vertical texture coordinate</param>
    private void UpdateScene(Vector3 rayDir, Vector3 finalColVector, int hCord, int vCord)
    {
        Color finalColor = ProcessColor(finalColVector);

        //Debug.Log("FinalColor: " + finalColor.ToString());

        // Set pixels on texture
        SetTexturePixel(hCord, vCord, finalColor);

        // Apply changes to texture
        UpdateRenderTexture();

        // Set line renderer point count based on settings value
        visualRayLine.positionCount = VisualizePath ? 2 + RayTraceUtility.RT_rec_points.Count() : 2;

        // Begin visual line at the origin
        visualRayLine.SetPosition(0, rayOrigin);

        // Use first ray as visual representation
        Vector3 initDir = new Vector3(rayDir.x, rayDir.y, rayDir.z);

        //_viewPortInfo.DirectionVector;
        //initDir.y += hCord * _viewPortInfo.VerticalIterationStep;   //rayDir.y += x * verticalIterationStep;
        //initDir.z -= vCord * _viewPortInfo.HorizontalIterationStep;

        //Debug.Log("InitDir:" + initDir.ToString());

        Vector3 endpoint;
        if (Physics.Raycast(new Ray(rayOrigin, initDir), out RaycastHit hit, RayTrace_Range, RayTraceUtility.LayerMask))
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
            foreach (Vector3 point in RayTraceUtility.RT_rec_points)
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

}
