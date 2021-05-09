
using HTC.UnityPlugin.Vive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Unity raycaster implementation based on the "Raytracing in a weekend" introductory course by Peter Shirley.
/// A lot of the functionality of the C++ implementation in the course is handled by Unity itself (camera position, casting of rays, etc.)
/// so the unity engine is used to replace these functionalities. The actual raytracing and color calculation
/// of pixels on the viewport was implemented as necessary to reproduce the image output of the course. Additional
/// functionality (second screens, textures, gameobjects) have been added.
/// 
/// This script should be attached to the gameobject the rays shall originate from. 
/// </summary>
//public class RayTracerUnity : MonoBehaviour
//{
//    #region Variables

//    #region ViewPlane

//    /// <summary>
//    /// ViewPort information containing information that is calculated on start up <see cref="ViewPortPlaneInformation"/>
//    /// /// </summary>
//    private ViewPortPlaneInformation _viewPortInfo;


//    // ToDo: Temporary, calculate dynamically later

//    /// <summary>
//    /// Ingame height of the plane the ray is shot through to set the corresponding pixel on plane
//    /// </summary>
//    private float planeHeight = 0.28f; 

//    /// <summary>
//    /// Ingame width of the plane the ray is shot throug h to set the corresponding pixel on plane
//    /// </summary>
//    private float planeWidth = 0.56f;

//    /// <summary>
//    /// The ingame plane that is used to represent the viewport in the scene.
//    /// Rays are shot through the viewport to set the pixels in the viewport with the calculated colors.
//    /// </summary>
//    public GameObject viewPortPlane;

//    /// <summary>
//    /// Collection of secondary screens the calculated texture is streamed to.
//    /// This can be used to easily apply the texture that is calculated by the raytracer 
//    /// to other gameobjects.
//    /// </summary>
//    public List<GameObject> secondScreens;

//    /// <summary>
//    /// Line renderer used to visualize the rays that are being shot during runtime.
//    /// </summary>
//    private LineRenderer visualRayLine;

//    /// <summary>
//    /// Wrapper object to handle rotation of the eye based on chaning ray trace direction vectors.
//    /// <see cref="EyeRotation"/>
//    /// </summary>
//    private EyeRotation _eyeRotation;


//    #endregion ViewPlane

//    #region Texture

//    /// <summary>
//    /// Scale of the texture containing the pixel that are being colored with the raytracer.
//    /// Each unit of scaling increases the dimension of the texutre, i. e. a scale factor of 1
//    /// corresponds to a texture made up of 100x100 pixels
//    /// </summary>
//    public int TextureScaleFactor = 2;

//    /// <summary>
//    /// Texture coordinates represented by a raw array of size 2.
//    /// The first int value is the horizontal coordinate (x), the second value
//    /// is the vertical coordinate (y).
//    /// During raytracing, these values are incremented for each iteration.    /// 
//    /// </summary>
//    private int[] CurrentPixel = new int[] { 0, 0 };

//    public int[] GetCurrentPixel()
//    {
//        return CurrentPixel;
//    }

//    /// <summary>
//    /// Information object containing texture information that is calculated on start up based on
//    /// set values <see cref="TextureInformation"/>
//    /// </summary>
//    private TextureInformation _textureInfo;

//    #endregion Texture

//    #region RayTracer

//    /// <summary>
//    /// Boolean signaling if raytracer is currently actively calculation. 
//    /// Since this raytracer is either raytracing or lying dormant, a simple boolean is enough (no FSM).
//    /// </summary>
//    private bool isRaytracing = false;

//    public bool IsRaytracing()
//    {
//        return isRaytracing;
//    }

//    /// <summary>
//    /// Bitfield mask to control which objects can be hit by a ray.
//    /// Currently, all objects that should be ignored by the raytracer were moved to a
//    /// custom layer (IgnoreRayCast). This layer corresponds to the value 9, so we set
//    /// all bits in the Int32 to 1, except for the ninth bit. This means all layers except for layer 9
//    /// can be hit by rays.
//    /// </summary>
//    private static readonly int layerMask = ~(1 << 9);


//    /// <summary>
//    /// Enum to define all possible iteration modes the raytracer can be in.
//    /// 
//    /// Automatic: Raytracer automatically moves on to the next pixel coordinate
//    /// 
//    /// Single: Raytracer waits for user signal before moving on to the next pixel coordinate
//    /// 
//    /// </summary>
//    public enum RT_IterationMode { Automatic = 0, Single = 1 };

//    /// <summary>
//    /// Current iteration mode of the raytracer
//    /// </summary>
//    public RT_IterationMode IterationMode = RT_IterationMode.Automatic;

//    /// <summary>
//    /// Setter for the iteration mode
//    /// </summary>
//    /// <param name="mode">New iteration mode</param>
//    public void SetIterationMode(RT_IterationMode mode)
//    {
//        IterationMode = mode;
//    }

//    public bool VisualizeCompleteRTPath = false;

//    public void SetCompleteRTPath(bool showPath)
//    {
//        VisualizeCompleteRTPath = showPath;
//    }

//    /// <summary>
//    /// Determines how far rays are shot into the scene.
//    /// Objects that are farther away from the ray origin point than this range will not be hit,
//    /// even if they are on the path of the ray
//    /// </summary>
//    public float RayTrace_Range = 20f;

//    /// <summary>
//    /// Point in the scene that rays are shot from.
//    /// Currently this point is represented by a floating eye.
//    /// </summary>
//    private Vector3 rayOrigin;


//    /// <summary>
//    /// Toggles wether anti aliasing is used during raytracing. If set to true,
//    /// color calulation for a single pixel is performed multiple times and the 
//    /// results are averaged into a color. This prevents jagged edges on object borders
//    /// by sampling the colors outside of an object to blend the borders together more smoothly.
//    /// </summary>
//    public bool AnitAliasing = true;

//    /// <summary>
//    /// The amount of samples being taken during anti aliasing for a single pixel
//    /// </summary>
//    public int AA_SampleSize = 100;

//    #endregion RayTracer
    

//    #endregion Variables

        
//    /// <summary>
//    /// Start method called once on scene instantiation
//    /// </summary>
//    void Start()
//    {
//        // Line Renderer
//        visualRayLine = GetComponent<LineRenderer>();
//        visualRayLine.enabled = true;

//        // Initialize plane and texture information
//        InitPlane();
//        InitTexture();

//        // Calculate iteration steps based on texture dimension and target viewport plane
//        _viewPortInfo.SetIterationSteps(_textureInfo.TextureDimension, planeWidth, planeHeight);

//        // Setup eye rotation using parameters
//        _eyeRotation = new EyeRotation(
//            transform.parent.transform.position,
//            planeWidth, planeHeight, _textureInfo.TextureDimension);

//        // Set origin of rays (object this script was assgined to)
//        rayOrigin = transform.position;        
//    }

//    /// <summary>
//    /// Update function called once per frame
//    /// </summary>
//    void Update()
//    {
//        // Check for user input
//        if(ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Menu)) // && !isRaytracing)
//        {
//            // If the raytracer is inactive initialize texture
//            if(!isRaytracing && CurrentPixel[0] == 0 && CurrentPixel[1] == 0)
//            {
//                InitTexture();
//            }
                
//            // Toggle raytracing activity
//            isRaytracing = !isRaytracing;
//        }

//        // If raytracer is raytracing, i.e. has texure coordinates left, move to next iteration
//        if (isRaytracing)
//        {   
//            // Start current raytracing iteration using next texture coordinate
//            StartCoroutine(DoRayTraceAA(CurrentPixel[0], CurrentPixel[1]));                   
                            
//            visualRayLine.enabled = true;

//            // Increment texture coordinate for next iteration
//            IncrementTexturePixelCoordinates();
            
//            // Stay on current iteration if raytracer is in single iteration mode
//            if (IterationMode == RT_IterationMode.Single) isRaytracing = false;
//        }
//    }

    
//    /// <summary>
//    /// Initialize viewport plane information
//    /// </summary>
//    private void InitPlane()
//    {
//        // Initialize information object
//        _viewPortInfo = new ViewPortPlaneInformation(viewPortPlane, transform, planeWidth, planeHeight);

//        // Direction vector from the ray origin point to the center of the viewport plane
//        _viewPortInfo.DirectionVector = new Vector3(
//                transform.right.x,
//                transform.right.y - planeHeight * 0.5f, //+ x * verticalIterationStep, //((verticalIterationStep * 2) / TexWidth) );
//                transform.right.z + planeWidth * 0.5f // - y * horizontalIterationStep //((horizontalIterationStep * 2) / TexHeight));
//                );
 
//    }

   
//    /// <summary>
//    /// Initialize texture 
//    /// </summary>
//    private void InitTexture()
//    {
//        // Initialize information object
//        _textureInfo = new TextureInformation(TextureScaleFactor, planeWidth, planeHeight);

//        // Set visual ray line width
//        visualRayLine.startWidth = _textureInfo.VisualLineWidth;
//        visualRayLine.endWidth = _textureInfo.VisualLineWidth;

//        // Connect texture to target objects
//        _viewPortInfo.PlaneRenderer.material.mainTexture = _textureInfo.StreamTexture2D;

//        foreach (var obj in secondScreens)
//        {
//            MeshRenderer rnd = obj.GetComponent<MeshRenderer>();
//            rnd.material.mainTexture = _textureInfo.StreamTexture2D;
//        }

//    }


//    /// <summary>
//    /// Increment texture coordinate for next raytracing iteration.
//    /// Traverses the texture from lower left corner to the upper right corner
//    /// Note: Texture is rotated on game object by 90 degrees
//    /// </summary>
//    private void IncrementTexturePixelCoordinates()
//    {        
//        // On last pixel, reset coordinates and set raytracer to inactive
//        if (CurrentPixel[1] == _textureInfo.TextureDimension)
//        {
//            CurrentPixel[0] = 0;
//            CurrentPixel[1] = 0;

//            isRaytracing = false;
//            return;
//        }

//        // On reaching the highest pixel on the vertical axis, move to the next pixel column
//        // and begin at the bottom
//        if (CurrentPixel[0] == _textureInfo.TextureDimension)
//        {
//            ++CurrentPixel[1];
//            CurrentPixel[0] = 0;
//        }
//        // If top hasn't been reached, move one pixel above the last pixel
//        else
//        {
//            ++CurrentPixel[0];
//        }
//    }

//    /// <summary>
//    /// Sets the color of a pixel in the texture
//    /// </summary>
//    /// <param name="x">Horizontal texture coordinate</param>
//    /// <param name="y">Vertical texture coordinate</param>
//    /// <param name="color">Calculated pixel color</param>
//    private void SetTexturePixel(int x, int y, Color color)
//    {
//        // Catch invalid coordinate values
//        if (x < 0 || x > _textureInfo.TextureDimension - 1) return;
//        if (y < 0 || y > _textureInfo.TextureDimension - 1) return;

//        // Update color array with new value,
//        int index = x * _textureInfo.TextureDimension + y;
//        _textureInfo.SetPixelColor(index, color);
        
//    }
    

//    /// <summary>
//    /// Completely cancels the current raytracer calculation and resets the texture
//    /// </summary>
//    public void ResetRaytracer()
//    {
//        isRaytracing = false;
//        CurrentPixel = new int[2] { 0, 0 };
//        InitTexture();
//    }

//    /// <summary>
//    /// Updates the source texture with the current pixel color values
//    /// </summary>
//    private void UpdateRenderTexture()
//    {
//        // Set active rendering texture
//        RenderTexture.active = _textureInfo.Texture;
        
//        _textureInfo.StreamTexture2D.ReadPixels(new Rect(0, 0, _textureInfo.TextureDimension, _textureInfo.TextureDimension), 0, 0);

//        // Update colors array in texture by setting all pixel colors at once (faster than single SetPixel() call)
//        // If this is too slow, consider using GetRawTextureData() instead
//        _textureInfo.StreamTexture2D.SetPixels(_textureInfo.PixelColorData);

//        // Apply changes and clear active texture
//        _textureInfo.StreamTexture2D.Apply();
//        RenderTexture.active = null;
//    }


//    private IEnumerator DoRayTraceAA(int xCord, int yCord)
//    {        
//        // Create job collections
//        NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(AA_SampleSize, Allocator.TempJob);
//        NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(AA_SampleSize, Allocator.TempJob);

//        List<Vector3> directionVectors = new List<Vector3>();
//        // Calculate direction vectors
//        for (int i = 0; i < raycastCommands.Length; ++i)
//        {
//            var hRandomVal = UnityEngine.Random.Range(0f, _viewPortInfo.HorizontalIterationStep - 1e-5f);
//            var vRandomVal = UnityEngine.Random.Range(0f, _viewPortInfo.VerticalIterationStep - 1e-5f);

//            Vector3 rayDir = _viewPortInfo.DirectionVector;
//            rayDir.y += xCord * _viewPortInfo.VerticalIterationStep + vRandomVal;   //rayDir.y += x * verticalIterationStep;
//            rayDir.z -= yCord * _viewPortInfo.HorizontalIterationStep + hRandomVal; //rayDir.z -= y * horizontalIterationStep;

//            directionVectors.Add(rayDir);

//            raycastCommands[i] = new RaycastCommand(rayOrigin, rayDir, RayTrace_Range, layerMask);
//        }

//        // Add raycasts to job queue and wait for them to finish
//        JobHandle raycastHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, AA_SampleSize, default(JobHandle));
//        raycastHandle.Complete();

//        List<RaycastHit> hitList = raycastHits.ToList();

//        // Deallocate job collections
//        raycastHits.Dispose();
//        raycastCommands.Dispose();
//        yield return null;
             
        
//        Vector3 colorSummation = Vector3.zero;
//        int validHitCounter = 0;
//        for(int i = 0; i < hitList.Count(); ++i)
//        {
//            if (hitList[i].distance > 1e-3)
//            {
//                ++validHitCounter;
//                Color tmpColor = DetermineHitColor(hitList[i], directionVectors[i]);
//                colorSummation += new Vector3(tmpColor.r, tmpColor.g, tmpColor.b);
//            }
//            else
//            {   
//                Color c = CreateNonHitColor(directionVectors[i]);
//                colorSummation += new Vector3(c.r, c.g, c.b);                
//            }
//            yield return null;
//        }

//        // Gamma correction
//        Vector3 finalColVector = colorSummation / AA_SampleSize; // validHitCounter;
//        finalColVector.x = Mathf.Sqrt(finalColVector.x);
//        finalColVector.y = Mathf.Sqrt(finalColVector.y);
//        finalColVector.z = Mathf.Sqrt(finalColVector.z);
//        Color finalColor = new Color(finalColVector.x, finalColVector.y, finalColVector.z);

//        // Set pixels 
//        SetTexturePixel(xCord, yCord, finalColor);

//        // Apply changes to texture
//        UpdateRenderTexture();

//        visualRayLine.positionCount = VisualizeCompleteRTPath ? 2 + rt_rec_points.Count() : 2;

//        // Begin visual line at the origin
//        visualRayLine.SetPosition(0, rayOrigin);

//        // Use first ray as visual representation
//        Vector3 initDir = _viewPortInfo.DirectionVector;
//        initDir.y += xCord * _viewPortInfo.VerticalIterationStep;   //rayDir.y += x * verticalIterationStep;
//        initDir.z -= yCord * _viewPortInfo.HorizontalIterationStep;

//        Vector3 endpoint;
//        if(Physics.Raycast(new Ray(rayOrigin, initDir), out RaycastHit hit, RayTrace_Range, layerMask))
//        {
//            endpoint = hit.point;
//        }
//        else
//        {
//            endpoint = rayOrigin + (initDir * RayTrace_Range);
//        }

//        visualRayLine.SetPosition(1, endpoint);

//        if(VisualizeCompleteRTPath)
//        {
//            byte counter = 2;
//            foreach (Vector3 point in rt_rec_points)
//            {
//                visualRayLine.SetPosition(counter++, point);
//            }
//        }        

//        transform.parent.rotation =
//            Quaternion.Euler(
//                new Vector3(
//                    0f,
//                    _eyeRotation.HorizontalEyeRotation(yCord),
//                    _eyeRotation.VerticalEyeRotation(xCord)
//                    )
//         );
//         yield return null;

//    }

  

//    // Partial Source: https://forum.unity.com/threads/trying-to-get-color-of-a-pixel-on-texture-with-raycasting.608431/
//    /// <summary>
//    /// Determine final pixel color based on hit object
//    /// </summary>
//    /// <param name="hit">Information about the raycast hit</param>
//    /// <param name="direction">Direction the ray was shot in</param>
//    /// <returns>Calculated pixel color</returns>
//    private Color DetermineHitColor(RaycastHit hit, Vector3 direction)
//    {
//        // Check if the ray hit anything
//        if(!(hit.collider is null))
//        {
//            // Material of the object that was hit
//            Material mat = hit.transform.gameObject.GetComponent<MeshRenderer>().material;

//            // On empty material, return error color
//            if (mat is null) return new Color(1, 0, 0);

//            // If material contains a texture, use that texture
//            if (!(mat.mainTexture is null))
//            {
//                // Determine u,v coordinates on texture and return texture pixel color
//                var texture = mat.mainTexture as Texture2D;
//                Vector2 pixelUVCoords = hit.textureCoord;
//                pixelUVCoords.x *= texture.width;
//                pixelUVCoords.y *= texture.height;
//                return texture.GetPixel(Mathf.FloorToInt(pixelUVCoords.x), Mathf.FloorToInt(pixelUVCoords.y));                
//            }
//            else
//            {
//                // On raw material hit, check for type
//                switch (DetermineMaterialType(mat))
//                {
//                    case MaterialType.Metal:
//                        return HandleMaterial(hit, direction, MaterialType.Metal, mat.color);

//                    case MaterialType.Dielectric:
//                        return HandleMaterial(hit, direction, MaterialType.Dielectric, mat.color);                        

//                    default:
//                    case MaterialType.SolidColor:
//                        return HandleMaterial(hit, direction, MaterialType.SolidColor, mat.color);
//                }
//            }
                            
//        }
//        else
//        {
//            // On non-hit, return non hit color
//            return CreateNonHitColor(direction);
//        }

//    }

//    /// <summary>
//    /// Enum type containing all material types that the raytracer can differentiate between
//    /// </summary>
//    public enum MaterialType { SolidColor = 1, Metal = 2, Dielectric = 3};
//    private List<Vector3> rt_points = new List<Vector3>();

//    /// <summary>
//    /// Handles additional ray calculations based on initial material hit
//    /// </summary>
//    /// <param name="hit">Initial raycast hit information</param>
//    /// <param name="direction">Initial raycast direction vector</param>
//    /// <param name="matType">Type of material hit <see cref="MaterialType"/></param>
//    /// <param name="matColor">Color of the hit material</param>
//    /// <returns></returns>
//    private Color HandleMaterial(RaycastHit hit, Vector3 direction, MaterialType matType, Color matColor)
//    {
//        rt_points.Clear();

//        // Determine if ray hit anything and discard all hits below a fixed distance
//        // to prevent dark spots in final image
//        if (!(hit.collider is null) && hit.distance > 1e-3f)
//        {
//            // Direction ray of next ray that scatters from the initial material point 
//            Ray scatterRay = new Ray();

//            // Attenuation 
//            Vector3 attenuation = new Vector3();

//            // Color values of the hit material
//            Vector3 matColorVec = new Vector3(matColor.r, matColor.g, matColor.b);

//            // Reconstructed initial ray
//            Ray mainRay = new Ray(hit.point, direction);

//            // Checks if next ray hits another object
//            bool rayHit = false;

//            // Determine values based on hit material type
//            switch (matType)
//            {
                
//                case MaterialType.SolidColor:
//                    rayHit = ScatterDiffuse(mainRay, hit, out attenuation, out scatterRay, matColorVec);
//                    break;

//                case MaterialType.Metal:
//                    rayHit = ScatterMetal(mainRay, hit, out attenuation, out scatterRay, matColorVec);
//                    break;

//                case MaterialType.Dielectric:
//                    float ref_idx = 1.5f;
//                    rayHit = ScatterDielectric(mainRay, hit, out attenuation, out scatterRay, ref_idx, matColorVec);
//                    break;
//            }

//            // Second ray hit
//            if (rayHit)
//            {
//                // Enter recursive ray tracing
//                rt_rec_points.Clear();
//                Vector3 tmpColorVec = RayTrace_Recursive(scatterRay, 0);
                
//                // Attenuate color vector
//                tmpColorVec.x *= attenuation.x;
//                tmpColorVec.y *= attenuation.y;
//                tmpColorVec.z *= attenuation.z;

//                // Initialize and return color based on vector values
//                Color retColor = new Color(tmpColorVec.x, tmpColorVec.y, tmpColorVec.z);
//                return retColor;
//            }
//            else
//            {
//                // On non-hit, return black color
//                return new Color(0f, 0f, 0f);
//            }

//        }
//        else
//        {
//            // On non-hit, return non-hit color
//            return CreateNonHitColor(direction);
//        }
//    }

//    /// <summary>
//    /// Create a color for the case that a ray does not hit an object.
//    /// This function creates a color between white and RGB(0.5, 0.7, 1.0)
//    /// based on the direction vector.
//    /// Based on "Raytracing in a weekend" C++ function
//    /// </summary>
//    /// <param name="direction"></param>
//    /// <returns></returns>
//    public static Color CreateNonHitColor(Vector3 direction)
//    {
//        Vector3 unit_direction = Vector3.Normalize(direction);
//        float t = 0.5f * (unit_direction.y + 1f);
//        var colVec = (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
//        return new Color(colVec.x, colVec.y, colVec.z);
//    }

//    /// <summary>
//    /// Static function to translate an ingame material into a <see cref="MaterialType"/> enum value
//    /// based on ingame material identifier.
//    /// </summary>
//    /// <param name="mat"></param>
//    /// <returns></returns>
//    private static MaterialType DetermineMaterialType(Material mat)
//    {
//        string matName = mat.name;

//        // Remove ' (Instance)' postfix
//        matName = matName.Split(' ')[0];

//        switch (matName)
//        {
//            case "Metal":
//                return MaterialType.Metal;

//            case "Dielectric":
//                return MaterialType.Dielectric;

//            default:
//                return MaterialType.SolidColor;
//        }

//    }

//    private List<Vector3> rt_rec_points = new List<Vector3>();

//    /// <summary>
//    /// Recursive raytracing used for special materials, i.e. reflection or refraction
//    /// of materials
//    /// </summary>
//    /// <param name="ray">Next ray</param>
//    /// <param name="depth">Current recursion depth</param>
//    /// <returns></returns>
//    private Vector3 RayTrace_Recursive(Ray ray, int depth)
//    {
//        // If ray hit another object and distance is above the threshold
//        if(Physics.Raycast(ray, out RaycastHit hit) && hit.distance > 1e-3 )
//        {
//            rt_rec_points.Add(hit.point);

//            // Scatter & attenuation
//            Ray scatter = new Ray();
//            Vector3 attenuation = Vector3.zero;

//            // Get mesh renderer
//            var mesh = hit.transform.gameObject.GetComponent<MeshRenderer>();
//            if (mesh == null) return Vector3.zero;

//            // Get material color
//            Material mat = mesh.material;
//            Vector3 matColVec = new Vector3(mat.color.r, mat.color.g, mat.color.b);
//            bool rayHit = false;

//            // Stop recursion above max depth
//            if (depth < 50)
//            {
//                switch(DetermineMaterialType(mat))
//                {
//                    case MaterialType.Metal:
//                        rayHit = ScatterMetal(ray, hit, out attenuation, out scatter, matColVec);
//                        break;

//                    case MaterialType.Dielectric:
//                        float ref_idx = 1.5f;
//                        rayHit = ScatterDielectric(ray, hit, out attenuation, out scatter, ref_idx, matColVec);
//                        break;

//                    case MaterialType.SolidColor:
//                        rayHit = ScatterDiffuse(ray, hit, out attenuation, out scatter, matColVec);
//                        break;
//                }

//                if(rayHit)
//                {
//                    // Next recursion level
                    
//                    var tmpVec = RayTrace_Recursive(scatter, depth + 1);

//                    // Apply attenuation
//                    tmpVec.x *= attenuation.x;
//                    tmpVec.y *= attenuation.y;
//                    tmpVec.z *= attenuation.z;

//                    return tmpVec;
//                }
//                else
//                {
//                    Vector3 unit_direction = Vector3.Normalize(ray.direction);
//                    float t = 0.5f * (unit_direction.y + 1f);
//                    return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
//                }
//            }
//            else
//            {
//                Color c = CreateNonHitColor(ray.direction);
//                return new Vector3(c.r, c.g, c.b);

//                //Vector3 unit_direction = Vector3.Normalize(ray.direction);
//                //float t = 0.5f * (unit_direction.y + 1f);
//                //return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
//            }

            
//        }
//        else
//        {
//            Color c = CreateNonHitColor(ray.direction);
//            return new Vector3(c.r, c.g, c.b);

//            //Vector3 unit_direction = Vector3.Normalize(ray.direction);
//            //float t = 0.5f * (unit_direction.y + 1f);
//            //return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);

//        }
//    }

//    /// <summary>
//    /// Scatter function for diffuse / solid color materials.
//    /// Based on "Raytracing in a weekend" C++ function
//    /// </summary>
//    /// <param name="r">Initial ray</param>
//    /// <param name="hit">Raycast hit information</param>
//    /// <param name="attenuation">Attenuation vector (output vector)</param>
//    /// <param name="scatterRay">Scatter ray (output vector)</param>
//    /// <param name="matColor">Material color vector</param>
//    /// <returns></returns>
//    private bool ScatterDiffuse(Ray r, RaycastHit hit, out Vector3 attenuation, out Ray scatterRay, Vector3 matColor)
//    {
//        Vector3 target = hit.point + hit.normal + UnityEngine.Random.insideUnitSphere;
//        scatterRay = new Ray(hit.point, target - hit.point);
//        attenuation = matColor;
//        return true;
//    }

//    /// <summary>
//    /// Static metal fuzz factor
//    /// </summary>
//    private float metalFuzz = 0.3f;

//    /// <summary>
//    /// Scatter function for metal materials.
//    /// Based on "Raytracing in a weekend" C++ function
//    /// </summary>
//    /// <param name="r">Initial ray</param>
//    /// <param name="hit">Raycast hit information</param>
//    /// <param name="attenuation">Attenuation vector (output parameter)</param>
//    /// <param name="scatterRay">Scatter ray (output parameter)</param>
//    /// <param name="matColor">Material color</param>
//    /// <returns></returns>
//    private bool ScatterMetal(Ray r, RaycastHit hit, out Vector3 attenuation, out Ray scatterRay, Vector3 matColor)
//    {
//        // Calculate reflection vector for metal material
//        Vector3 reflected = Reflect(Vector3.Normalize(r.direction), hit.normal);

//        // Scatter in a direction based on reflection, fuzziness of the material and a random point
//        // in the unit sphere above the point
//        scatterRay = new Ray(hit.point, reflected + metalFuzz * UnityEngine.Random.insideUnitSphere);

//        // Calculate new vectors and determine next hit truth value
//        var tmpColor = hit.transform.gameObject.GetComponent<MeshRenderer>().material.color;
//        attenuation = new Vector3(tmpColor.r, tmpColor.g, tmpColor.b);
//        return (Vector3.Dot(scatterRay.direction, hit.normal) > 0f);
//    }

//    /// <summary>
//    /// Static function to calculate reflection vector.
//    /// Based on "Raytracing in a weekend" C++ function
//    /// </summary>
//    /// <param name="v">Initial entry direction vector</param>
//    /// <param name="n">Normal of the hit triangle</param>
//    /// <returns>Reflection direction vector</returns>
//    public static Vector3 Reflect(Vector3 v, Vector3 n)
//    {
//        return v - 2f * Vector3.Dot(v, n) * n;
//    }

//    /// <summary>
//    /// Scatter function for dielectric materials.
//    /// Based on "Raytracing in a weekend" C++ function.
//    /// </summary>
//    /// <param name="r">Initial ray</param>
//    /// <param name="hit">Raycast hit information</param>
//    /// <param name="attenuation">Attenuation vector (output parameter)</param>
//    /// <param name="scatterRay">Scatter ray (output parameter)</param>
//    /// <param name="refIdx">Refraction index of material</param>
//    /// <param name="matColor">Material color</param>
//    /// <returns></returns>
//    private bool ScatterDielectric(Ray r, RaycastHit hit, out Vector3 attenuation, out Ray scatterRay, float refIdx, Vector3 matColor)
//    {
//        // Initialize variables
//        Vector3 outwardNormal;
//        Vector3 reflected = Reflect(r.direction, hit.normal);
//        float ni_over_nt;
//        attenuation = new Vector3(1f, 1f, 1f);
//        Vector3 refracted;
//        float reflect_prob;
//        float cosine;


//        if(Vector3.Dot(r.direction, hit.normal) > 0)
//        {
//            outwardNormal = -hit.normal;
//            ni_over_nt = refIdx;
//            cosine = refIdx * Vector3.Dot(r.direction, hit.normal) / r.direction.magnitude;
//        }
//        else
//        {
//            outwardNormal = hit.normal;
//            ni_over_nt = 1f / refIdx;
//            cosine = -Vector3.Dot(r.direction, hit.normal) / r.direction.magnitude;
//        }

//        if(Refract(r.direction, outwardNormal, ni_over_nt, out refracted))
//        {
//            reflect_prob = Schlick(cosine, refIdx);
//        }
//        else
//        {
//            scatterRay = new Ray(hit.point, reflected);
//            reflect_prob = 1f;
//        }

//        if(UnityEngine.Random.Range(0f, 1f - 1e-5f) < reflect_prob)
//        {
//            scatterRay = new Ray(hit.point, reflected);
//        }
//        else
//        {
//            scatterRay = new Ray(hit.point, refracted);
//        }


//        return true;


//    }

//    /// <summary>
//    /// Static function to calculate a refraction vector based on vector information
//    /// </summary>
//    /// <param name="v">Initial entry direction vector</param>
//    /// <param name="n">Normal vector of hit triangle</param>
//    /// <param name="ni_over_nt"></param>
//    /// <param name="refracted"></param>
//    /// <returns></returns>
//    public static bool Refract(Vector3 v, Vector3 n, float ni_over_nt, out Vector3 refracted)
//    {
//        Vector3 uv = Vector3.Normalize(v);
//        float dt = Vector3.Dot(uv, n);
//        float discriminant = 1f - ni_over_nt * ni_over_nt * (1f - dt * dt);
//        if(discriminant > 0)
//        {
//            refracted = ni_over_nt * (uv - n * dt) - n * Mathf.Sqrt(discriminant);
//            return true;
//        }

//        refracted = Vector3.zero;
//        return false;
//    }

//    /// <summary>
//    /// Polynomial approximation (Christophe Schlick).
//    /// 
//    /// </summary>
//    /// <param name="cosine"></param>
//    /// <param name="ref_idx"></param>
//    /// <returns></returns>
//    public static float Schlick(float cosine, float ref_idx)
//    {
//        float r0 = (1f - ref_idx) / (1f + ref_idx);
//        r0 *= r0;
//        return r0 + (1f - r0) * Mathf.Pow((1f - cosine), 5f);
//    }
    

//    /// <summary>
//    /// Wrapper class to encapsulate ViewPort information and calculation functions
//    /// </summary>
//    public class ViewPortPlaneInformation
//    {
//        /// <summary>
//        /// Renderer object of the viewport plane
//        /// </summary>
//        public Renderer PlaneRenderer { get; set; }

//        /// <summary>
//        /// Direction vector from ray origin to ViewPort
//        /// </summary>
//        public Vector3 DirectionVector { get; set; }

//        /// <summary>
//        /// Distance between two pixels on the vertical axis
//        /// </summary>
//        public float VerticalIterationStep { get; set; }

//        /// <summary>
//        /// Distance between two pixels on the horizontal axis
//        /// </summary>
//        public float HorizontalIterationStep { get; set; }

//        /// <summary>
//        /// Argument constructor.
//        /// Initializes object based on parameter values
//        /// </summary>
//        /// <param name="viewPortPlane">Plane gameobject</param>
//        /// <param name="rayOrigin">Origin point of rays</param>
//        /// <param name="width">Width of the viewport plane (ingame unity distance)</param>
//        /// <param name="height">Height of the viewport plane (ingame unity distance)</param>
//        public ViewPortPlaneInformation(GameObject viewPortPlane, Transform rayOrigin, float width, float height)
//        {
//            // Set rendere of associated plane
//            PlaneRenderer = viewPortPlane.GetComponent<MeshRenderer>();

//            // Initialize default direction vector from rayorigin to the center of the viewport plane
//            DirectionVector = new Vector3(
//                    rayOrigin.right.x,
//                    rayOrigin.right.y - height * 0.5f,
//                    rayOrigin.right.z + width * 0.5f 
//            );
//        }

//        /// <summary>
//        /// Initialize the distances between two pixels in horizontal and vertical direction.
//        /// This is used to make sure the LineRenderer always points to the current pixel that
//        /// is being raytraced
//        /// </summary>
//        /// <param name="textureDimension"></param>
//        /// <param name="planeWidth"></param>
//        /// <param name="planeHeight"></param>
//        public void SetIterationSteps(int textureDimension, float planeWidth, float planeHeight)
//        {
//            VerticalIterationStep = planeHeight / textureDimension;
//            HorizontalIterationStep = planeWidth / textureDimension;
//        }
//    }

//    /// <summary>
//    /// Wrapper class to encapsulate Texture information and calculation functions
//    /// </summary>
//    public class TextureInformation
//    {
//        /// <summary>
//        /// Dimension of texture. Currenty the texture is square
//        /// and has the same horizontal and vertical dimension.
//        /// </summary>
//        public int TextureDimension { get; set; }

//        /// <summary>
//        /// Render texture that pixel color information is transmitted to
//        /// </summary>
//        public RenderTexture Texture { get; set; }

//        /// <summary>
//        /// Raw texture that is drawn in
//        /// </summary>
//        public Texture2D StreamTexture2D { get; set; }

//        /// <summary>
//        /// Width of the visual line of the LineRenderer.
//        /// The width of the line is determined based on texture dimension (more pixels => smaller ray)
//        /// </summary>
//        public float VisualLineWidth { get; set; }

//        /// <summary>
//        /// Raw array containing pixel color information. On change, the complete
//        /// array is transmitted as the new texture image data.
//        /// </summary>
//        public Color[] PixelColorData { get; }       

//        /// <summary>
//        /// Static number of pixels for each texture scaling unit
//        /// </summary>
//        private int _scalingPixelCount = 100;

//        /// <summary>
//        /// Argument constructor
//        /// </summary>
//        /// <param name="scaleFactor">Texture scale factor</param>
//        /// <param name="planeWidth">Width of viewport gameobject</param>
//        /// <param name="planeHeight">Height of viewport gameobject</param>
//        public TextureInformation(int scaleFactor, float planeWidth, float planeHeight)
//        {
//            TextureDimension = scaleFactor * _scalingPixelCount;
//            VisualLineWidth = 0.025f * (1f / scaleFactor);
//            Texture = new RenderTexture(TextureDimension, TextureDimension, 1);

//            int pixelCount = TextureDimension * TextureDimension;

//            // Initialize color array
//            PixelColorData = new Color[pixelCount];
//            Color initColor = new Color(0, 0, 0);
//            for (int i = 0; i < pixelCount; ++i)
//            {
//                PixelColorData[i] = initColor;
//            }

//            StreamTexture2D = new Texture2D(TextureDimension, TextureDimension);            
//        }
        
//        /// <summary>
//        /// Setter for color array value
//        /// </summary>
//        /// <param name="index">Absolute index in array</param>
//        /// <param name="col">New color value</param>
//        public void SetPixelColor(int index, Color col)
//        {
//            // Check invalid values
//            if (index < 0 || index > PixelColorData.Length - 1) return;

//            PixelColorData[index] = col;
//        }
//    }

//    /// <summary>
//    /// Wrapper class to encapsulate the eye rotation functionality
//    /// </summary>
//    public class EyeRotation
//    {
//        #region Properties

//        /// <summary>
//        /// Position of the eye
//        /// </summary>
//        private Vector3 _eyePosition;

//        /// <summary>
//        /// Width of the viewport plane
//        /// </summary>
//        private float _planeWidth;

//        /// <summary>
//        /// Height of the viewport plane
//        /// </summary>
//        private float _planeHeight;

//        /// <summary>
//        /// Texture dimension
//        /// </summary>
//        private int _textureDimension;

//        /// <summary>
//        /// Private float used for custom getter of property
//        /// </summary>
//        private float? _hMaxRotation;

//        /// <summary>
//        /// Public readonly property representing the maximum horizontal rotation
//        /// of the eye, i.e. the angle between the left and right borders of the 
//        /// viewport plane
//        /// </summary>
//        public float HorizontalMaxRotation
//        {
//            get
//            {
//                if(_hMaxRotation is null)
//                {
//                    float halfWidth = (_planeWidth / 2);
//                    Vector3 leftPlanePoint = _eyePosition;
//                    leftPlanePoint.x -= halfWidth;
//                    Vector3 rightPlanePoint = _eyePosition;
//                    rightPlanePoint.x += halfWidth;
//                    _hMaxRotation = Vector3.Angle(leftPlanePoint, rightPlanePoint);
//                }

//                return _hMaxRotation.Value;
//            }
//        }

//        /// <summary>
//        /// Private float used for custom getter of property
//        /// </summary>
//        private float? _vMaxRotation;

//        /// <summary>
//        /// Public readonly property representing the maximum vertical rotation
//        /// of the eye, i.e. the angle between the top and bottom borders of the 
//        /// viewport plane
//        /// </summary>
//        public float VerticalMaxRotation
//        {
//            get
//            {
//                if(_vMaxRotation is null)
//                {
//                    float halfHeight = (_planeHeight / 2);
//                    var bottomPlanePoint = _eyePosition;
//                    bottomPlanePoint.y -= halfHeight;
//                    var topPlanePoint = _eyePosition;
//                    topPlanePoint.y += halfHeight;
//                    _vMaxRotation = Vector3.Angle(bottomPlanePoint, topPlanePoint);
//                }

//                return _vMaxRotation.Value;
//            }
//        }

//        /// <summary>
//        /// Private float used for custom getter of property
//        /// </summary>
//        private float? _hRotationStep;

//        /// <summary>
//        /// Angle between two pixels on the horizontal axis
//        /// </summary>
//        public float HorizontalRotationStep
//        {
//            get
//            {
//                if(_hRotationStep is null)
//                {
//                    _hRotationStep = HorizontalMaxRotation / (float)_textureDimension;
//                }

//                return _hRotationStep.Value;
//            }
//        }

//        /// <summary>
//        /// Private float used for custom getter of property
//        /// </summary>
//        private float? _vRotationStep;

//        /// <summary>
//        /// Angle between two pixels on the vertical axis
//        /// </summary>
//        public float VerticalRotationStep
//        {
//            get
//            {
//                if (_vRotationStep is null)
//                {
//                    _vRotationStep = VerticalMaxRotation / _textureDimension;
//                }

//                return _vRotationStep.Value;
//            }
//        }

//        #endregion Properties

//        /// <summary>
//        /// Argument constructor
//        /// </summary>
//        /// <param name="eyePosition">Position of eye object</param>
//        /// <param name="planeWidth">Width of viewport plane</param>
//        /// <param name="planeHeight">Height of viewport plane</param>
//        /// <param name="textureDimension">Texture dimension</param>
//        public EyeRotation(Vector3 eyePosition, float planeWidth, float planeHeight, int textureDimension)
//        {
//            _eyePosition = eyePosition;
//            _planeWidth = planeWidth;
//            _planeHeight = planeHeight;
//            _textureDimension = textureDimension;
//        }      
        
//        /// <summary>
//        /// Query the horizontal eye rotation based on the given color array coordinate
//        /// </summary>
//        /// <param name="colorArrayCoordinate">Array coordinate</param>
//        /// <returns>Horizontal eye rotation</returns>
//        public float HorizontalEyeRotation(int colorArrayCoordinate)
//        {
//            return -(HorizontalMaxRotation / 2f) + colorArrayCoordinate * HorizontalRotationStep;
//        }

//        /// <summary>
//        /// Query the vertical eye rotation based on the given color array coordinate
//        /// </summary>
//        /// <param name="colorArrayCoordinate">Array coordinate</param>
//        /// <returns>Vertical eye rotation</returns>
//        public float VerticalEyeRotation(int colorArrayCoordinate)
//        {
//            return -(VerticalMaxRotation / 2f) + colorArrayCoordinate * VerticalRotationStep;
//        }


//    }

//}
