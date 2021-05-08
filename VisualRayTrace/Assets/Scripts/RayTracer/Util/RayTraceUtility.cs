using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RayTraceUtility
{
    /// <summary>
    /// Global world object the unity scene is parsed into
    /// </summary>
    public static WorldInformation GlobalWorld;

    /// <summary>
    /// Static instance of solid color material
    /// </summary>
    public static PhongMaterial SolidColorMaterial = new PhongMaterial(Vector3.zero);        


    private static ReflectiveMaterial _metalMat;

    /// <summary>
    /// Static instance of metal material
    /// </summary>
    public static ReflectiveMaterial MetalMaterial
    { 
        get
        {
            if(_metalMat is null)
            {
                _metalMat = new ReflectiveMaterial(Vector3.zero);
                _metalMat.SetKA(Metal_KA);
                _metalMat.SetKD(Metal_KD);
                _metalMat.SetKS(Metal_KS);
                _metalMat.SetExp(Metal_EXP);
                _metalMat.SetKR(Metal_KR);
                _metalMat.SetCR(Color.white);
            }

            return _metalMat;
        }    
    }

    // Metal parameters
    public static float Metal_KA = 0.25f;
    public static float Metal_KD = 0.5f;
    public static float Metal_KS = 0.15f;
    public static int Metal_EXP = 100;
    public static float Metal_KR = 0.75f;


    private static DielectricMaterial _glassMat;

    /// <summary>
    /// Static instance of dielectric material
    /// </summary>
    public static DielectricMaterial GlassMaterial
    {
        get
        {
            if(_glassMat is null)
            {
                _glassMat = new DielectricMaterial(Vector3.zero, Color.white, Color.white);
                _glassMat.SetKS(Dielectric_KS);
                _glassMat.SetExp(Dielectric_EXP);
                _glassMat.SetEtaIn(Dielectric_EtaIN);
                _glassMat.SetEtaOut(Dielectric_EtaOUT);
            }

            return _glassMat;
        }
    }

    // Dielectric parameters
    public static float Dielectric_KS = 0.2f;
    public static float Dielectric_EXP = 100f;
    public static float Dielectric_EtaIN = 1.5f;
    public static float Dielectric_EtaOUT = 1f;

    /// <summary>
    /// Maximum distance that rays are cast
    /// </summary>
    public static float Raycast_Distance = 30f;

    /// <summary>
    /// Bitfield mask to control which objects can be hit by a ray.
    /// Currently, all objects that should be ignored by the raytracer were moved to a
    /// custom layer (IgnoreRayCast). This layer corresponds to the value 9, so we set
    /// all bits in the Int32 to 1, except for the ninth bit. This means all layers except for layer 9
    /// can be hit by rays.
    /// </summary>
    public static int LayerMask = ~(1 << 9);

    /// <summary>
    /// Horizontal width between centers of individual pixels on viewport
    /// </summary>
    public static float H_STEP;

    /// <summary>
    /// Vertical height between centers of individual pixels on viewport
    /// </summary>
    public static float V_STEP;


    /// <summary>
    /// Enum type containing all material types that the raytracer can differentiate between
    /// </summary>
    public enum MaterialType { SolidColor = 1, Metal = 2, Dielectric = 3 };


    /// <summary>
    /// Static metal fuzz factor
    /// </summary>
    private static float metalFuzz = 0.3f;


    /// <summary>
    /// Create a color for the case that a ray does not hit an object.
    /// This function creates a color between white and RGB(0.5, 0.7, 1.0)
    /// based on the direction vector.
    /// Based on "Raytracing in a weekend" C++ function
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Color CreateNonHitColor(Vector3 direction)
    {
        Vector3 unit_direction = Vector3.Normalize(direction);
        float t = 0.5f * (unit_direction.y + 1f);
        var colVec = (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
        return new Color(colVec.x, colVec.y, colVec.z);

        //return RayTraceUtility.GlobalWorld.BackgroundColor;
    }

    /// <summary>
    /// Static function to translate an ingame material into a <see cref="MaterialType"/> enum value
    /// based on ingame material identifier.
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static MaterialType DetermineMaterialType(Material mat)
    {
        string matName = mat.name;

        // Remove ' (Instance)' postfix
        matName = matName.Split(' ')[0];

        switch (matName)
        {
            case "Metal": return MaterialType.Metal;
            case "Dielectric": return MaterialType.Dielectric;
            default: return MaterialType.SolidColor;
        };
    }

    public static List<Vector3> RT_points = new List<Vector3>();
    public static List<Vector3> RT_rec_points = new List<Vector3>();

    //// Partial Source: https://forum.unity.com/threads/trying-to-get-color-of-a-pixel-on-texture-with-raycasting.608431/
    ///// <summary>
    ///// Determine final pixel color based on hit object
    ///// </summary>
    ///// <param name="hit">Information about the raycast hit</param>
    ///// <param name="direction">Direction the ray was shot in</param>
    ///// <returns>Calculated pixel color</returns>
    //public static Color DetermineHitColor(RaycastHit hit, Vector3 direction)
    //{
    //    // Check if the ray hit anything
    //    if (!(hit.collider is null))
    //    {
    //        // Material of the object that was hit
    //        Material mat = hit.transform.gameObject.GetComponent<MeshRenderer>().material;

    //        // On empty material, return error color
    //        if (mat is null) return Color.red; // new Color(1, 0, 0);

    //        // If material contains a texture, use that texture
    //        if (!(mat.mainTexture is null))
    //        {
    //            // Determine u,v coordinates on texture and return texture pixel color
    //            var texture = mat.mainTexture as Texture2D;
    //            Vector2 pixelUVCoords = hit.textureCoord;
    //            pixelUVCoords.x *= texture.width;
    //            pixelUVCoords.y *= texture.height;
    //            return texture.GetPixel(Mathf.FloorToInt(pixelUVCoords.x), Mathf.FloorToInt(pixelUVCoords.y));
    //        }
    //        else
    //        {
    //            // On raw material hit, check for type
    //            switch (RayTraceUtility.DetermineMaterialType(mat))
    //            {
    //                case RayTraceUtility.MaterialType.Metal:
    //                    //Debug.Log("Metal Hit!");

    //                    ReflectiveMaterial metalMat = new ReflectiveMaterial(direction);
    //                    metalMat.SetCD(mat.color);
    //                    return metalMat.Shade(hit, 0);

    //                    //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Metal, mat.color);

    //                case RayTraceUtility.MaterialType.Dielectric:
    //                    TransparentMaterial dielectricMat =
    //                        new TransparentMaterial(
    //                            direction, Dielectric_KS, Dielectric_EXP, 1.5f, 0.1f, 0.9f
    //                        );
    //                    dielectricMat.SetCD(mat.color);
    //                    return dielectricMat.Shade(hit, 0);

    //                    //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Dielectric, mat.color);

    //                default:
    //                case RayTraceUtility.MaterialType.SolidColor:
                        
    //                    var tmpMat = new PhongMaterial(direction);
    //                    //Debug.Log("SolidColor - InitMatColor: " + mat.color);
    //                    tmpMat.SetCD(mat.color);
    //                    Color tmpColor = tmpMat.Shade(hit, 0);
    //                    //Debug.Log("SolidColor - ShadedColor: " + tmpColor);
    //                    return tmpColor;

    //                    //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.SolidColor, mat.color);
    //            }
    //        }

    //    }
    //    else
    //    {
    //        // On non-hit, return non hit color
    //        return RayTraceUtility.CreateNonHitColor(direction);
    //    }

    //}


    ///// <summary>
    ///// Handles additional ray calculations based on initial material hit
    ///// </summary>
    ///// <param name="hit">Initial raycast hit information</param>
    ///// <param name="direction">Initial raycast direction vector</param>
    ///// <param name="matType">Type of material hit <see cref="MaterialType"/></param>
    ///// <param name="matColor">Color of the hit material</param>
    ///// <returns></returns>
    //public static Color HandleMaterial(RaycastHit hit, Vector3 direction, RayTraceUtility.MaterialType matType, Color matColor)
    //{
    //    RT_points.Clear();

    //    // Determine if ray hit anything and discard all hits below a fixed distance
    //    // to prevent dark spots in final image
    //    if (!(hit.collider is null) && hit.distance > 1e-3f)
    //    {
    //        // Direction ray of next ray that scatters from the initial material point 
    //        Ray scatterRay = new Ray();

    //        // Attenuation 
    //        Vector3 attenuation = new Vector3();

    //        // Color values of the hit material
    //        Vector3 matColorVec = new Vector3(matColor.r, matColor.g, matColor.b);

    //        // Reconstructed initial ray
    //        Ray mainRay = new Ray(hit.point, direction);

    //        // Checks if next ray hits another object
    //        bool rayHit = false;

    //        // Determine values based on hit material type
    //        switch (matType)
    //        {
    //            case RayTraceUtility.MaterialType.SolidColor:
    //                rayHit = RayTraceUtility.ScatterDiffuse(mainRay, hit, out attenuation, out scatterRay, matColorVec);
    //                break;

    //            case RayTraceUtility.MaterialType.Metal:
    //                rayHit = RayTraceUtility.ScatterMetal(mainRay, hit, out attenuation, out scatterRay, matColorVec);
    //                break;

    //            case RayTraceUtility.MaterialType.Dielectric:
    //                float ref_idx = 1.5f;
    //                rayHit = RayTraceUtility.ScatterDielectric(mainRay, hit, out attenuation, out scatterRay, ref_idx, matColorVec);
    //                break;
    //        }

    //        // Second ray hit
    //        if (rayHit)
    //        {
    //            // Enter recursive ray tracing
    //            RT_rec_points.Clear();
    //            Vector3 tmpColorVec = RayTrace_Recursive(scatterRay, 0);

    //            // Attenuate color vector
    //            tmpColorVec.x *= attenuation.x;
    //            tmpColorVec.y *= attenuation.y;
    //            tmpColorVec.z *= attenuation.z;

    //            // Initialize and return color based on vector values
    //            Color retColor = new Color(tmpColorVec.x, tmpColorVec.y, tmpColorVec.z);
    //            return retColor;
    //        }
    //        else
    //        {
    //            // On non-hit, return black color
    //            return new Color(0f, 0f, 0f);
    //        }

    //    }
    //    else
    //    {
    //        // On non-hit, return non-hit color
    //        return RayTraceUtility.CreateNonHitColor(direction);
    //    }
    //}

    ///// <summary>
    ///// Recursive raytracing used for special materials, i.e. reflection or refraction
    ///// of materials
    ///// </summary>
    ///// <param name="ray">Next ray</param>
    ///// <param name="depth">Current recursion depth</param>
    ///// <returns></returns>
    //private static Vector3 RayTrace_Recursive(Ray ray, int depth)
    //{
    //    // If ray hit another object and distance is above the threshold
    //    if (Physics.Raycast(ray, out RaycastHit hit) && hit.distance > 1e-3)
    //    {
    //        RT_rec_points.Add(hit.point);

    //        // Scatter & attenuation
    //        Ray scatter = new Ray();
    //        Vector3 attenuation = Vector3.zero;

    //        // Get mesh renderer
    //        var mesh = hit.transform.gameObject.GetComponent<MeshRenderer>();
    //        if (mesh == null) return Vector3.zero;

    //        // Get material color
    //        Material mat = mesh.material;
    //        Vector3 matColVec = new Vector3(mat.color.r, mat.color.g, mat.color.b);
    //        bool rayHit = false;

    //        // Stop recursion above max depth
    //        if (depth < RayTraceUtility.GlobalWorld.MaxDepth)
    //        {
    //            switch (RayTraceUtility.DetermineMaterialType(mat))
    //            {
    //                case RayTraceUtility.MaterialType.Metal:
    //                    rayHit = RayTraceUtility.ScatterMetal(ray, hit, out attenuation, out scatter, matColVec);
    //                    break;

    //                case RayTraceUtility.MaterialType.Dielectric:
    //                    float ref_idx = 1.5f;
    //                    rayHit = RayTraceUtility.ScatterDielectric(ray, hit, out attenuation, out scatter, ref_idx, matColVec);
    //                    break;

    //                case RayTraceUtility.MaterialType.SolidColor:
    //                    rayHit = RayTraceUtility.ScatterDiffuse(ray, hit, out attenuation, out scatter, matColVec);
    //                    break;
    //            }

    //            if (rayHit)
    //            {
    //                // Next recursion level

    //                var tmpVec = RayTrace_Recursive(scatter, depth + 1);

    //                // Apply attenuation
    //                tmpVec.x *= attenuation.x;
    //                tmpVec.y *= attenuation.y;
    //                tmpVec.z *= attenuation.z;

    //                return tmpVec;
    //            }
    //            else
    //            {
    //                Vector3 unit_direction = Vector3.Normalize(ray.direction);
    //                float t = 0.5f * (unit_direction.y + 1f);
    //                return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
    //            }
    //        }
    //        else
    //        {
    //            Color c = RayTraceUtility.CreateNonHitColor(ray.direction);
    //            return new Vector3(c.r, c.g, c.b);

    //            //Vector3 unit_direction = Vector3.Normalize(ray.direction);
    //            //float t = 0.5f * (unit_direction.y + 1f);
    //            //return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);
    //        }


    //    }
    //    else
    //    {
    //        Color c = RayTraceUtility.CreateNonHitColor(ray.direction);
    //        return new Vector3(c.r, c.g, c.b);

    //        //Vector3 unit_direction = Vector3.Normalize(ray.direction);
    //        //float t = 0.5f * (unit_direction.y + 1f);
    //        //return (1f - t) * new Vector3(1f, 1f, 1f) + t * new Vector3(.5f, .7f, 1f);

    //    }


    //}


    /// <summary>
    /// Global tone mapping operator to map out-of-gamut colors into displayable range
    /// </summary>
    /// <param name="c">Color in initial range</param>
    /// <returns>Range corrected color</returns>
    public static Color MaxToOne(Color c)
    {
        // Determine max value in color
        float maxValue = Mathf.Max(c.r, Mathf.Max(c.g, c.b));

        if (maxValue > 1f)
        {
            return c / maxValue;
        }
        else
        {
            return c;
        }
    }

    /// <summary>
    /// Corrects color values if any value is above 1
    /// </summary>
    /// <param name="rawColor">Color to be checked</param>
    /// <returns>Corrected color</returns>
    public static Color ClampToColor(Color rawColor)
    {
        Color c = new Color(rawColor.r, rawColor.g, rawColor.g);

        // If any value is out of range, map color to red
        if (rawColor.r > 1f || rawColor.g > 1f || rawColor.b > 1f)
        {
            c.r = 1f; c.g = 0f; c.b = 0f;
        }

        return c;
    }


    /// <summary>
    /// World encapsulation to parse unity scene into a simple construct that can be used in raytracing
    /// </summary>
    public class WorldInformation
    {
        /// <summary>
        /// Information about the viewport in the scene
        /// </summary>
        //public ViewPortPlaneInformation VP { get; set; }

        /// <summary>
        /// Tracer instance to be used to shoot rays during raytracing
        /// </summary>
        public AbstractTracer Tracer { get; set; }

        /// <summary>
        /// Maximum amount of reflections that are calculated into final color
        /// </summary>
        public int MaxDepth { get; set; } = 10;
        
        /// <summary>
        /// Background color of the raytraced scene
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.black;

        /// <summary>
        /// Global ambient light instance that illuminates the whole scene with a constant light amount
        /// </summary>
        public AbstractLight GlobalAmbientLight { get; set; }

        /// <summary>
        /// Lights in unity scene that influence the ray traced scene
        /// </summary>
        public List<AbstractLight> GlobalLights { get; set; } = new List<AbstractLight>();
                
        public WorldInformation()
        {
            // Create sampler for ambient occluder
            MultiJitteredSampler ambOccSampler = new MultiJitteredSampler(50, 50, H_STEP, V_STEP);

            // Use ambient occluder as global ambient light
            Color ambientMinColor = new Color(0.05f, 0.05f, 0.05f, 1);
            AmbientOccluder occluder = new AmbientOccluder(ambOccSampler, ambientMinColor)
            {
                RadianceFactor = 1f,
                LightColor = Color.white
            };
            GlobalAmbientLight = occluder;

            // Parse scene lights
            foreach (Light l in Resources.FindObjectsOfTypeAll(typeof(Light)) as Light[])
            {
                switch (l.type)
                {
                    case LightType.Directional:
                        
                            // Direction vector of default direction light path
                            Vector3 dirVector = new Vector3(0, 0, 1);
                            Vector3 lightRotationVec = l.transform.rotation.eulerAngles;
                            //Debug.Log("GlobalLights - DirectionalLight: EulerAngles: " + lightRotationVec);

                            // Parse light object rotation 
                            if (lightRotationVec.x != 0f)
                                dirVector = Quaternion.AngleAxis(lightRotationVec.x, Vector3.right) * dirVector;

                            if (lightRotationVec.y != 0f)
                                dirVector = Quaternion.AngleAxis(lightRotationVec.y, Vector3.up) * dirVector;

                            if (lightRotationVec.z != 0f)
                                dirVector = Quaternion.AngleAxis(lightRotationVec.z, Vector3.forward) * dirVector;

                            DirectionalLight tmpLight = new DirectionalLight(dirVector, l.transform.position);
                            tmpLight.CastShadows = false;
                            tmpLight.RadianceFactor = l.intensity;

                            GlobalLights.Add(tmpLight);
                            //Debug.Log("GlobalLights - DirectionalLight Added: " + dirVector);
                        
                        break;

                    case LightType.Point:
                        GlobalLights.Add(new PointLight(l.intensity, l.color, l.transform.position));
                        break;
                }
            }
        }

    }

}

