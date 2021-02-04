using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RayTraceUtility
{
    public static WorldInformation GlobalWorld;

    public static AbstractMaterial SolidColorMaterial;

    public static AbstractMaterial MetalMaterial;

    public static float Metal_KA = 0.25f;
    public static float Metal_KD = 0.5f;
    public static float Metal_KS = 0.15f;
    public static int Metal_EXP = 100;
    public static float Metal_KR = 0.75f;
    
    public static AbstractMaterial DielectricMaterial;

    public static float Dielectric_KS = 0.2f;
    public static float Dielectric_EXP = 2000f;
    public static float Dielectric_EtaIN = 1.5f;
    public static float Dielectric_EtaOUT = 1f;


    /// <summary>
    /// Enum type containing all material types that the raytracer can differentiate between
    /// </summary>
    public enum MaterialType { SolidColor = 1, Metal = 2, Dielectric = 3 };


    //public static bool ShootRay(Ray ray, out RaycastHit hit, float maxDist, int layerMask)
    //{
    //    return Physics.Raycast(ray, out hit, maxDist, layerMask);
    //}

    /// <summary>
    /// Scatter function for diffuse / solid color materials.
    /// Based on "Raytracing in a weekend" C++ function
    /// </summary>
    /// <param name="r">Initial ray</param>
    /// <param name="hit">Raycast hit information</param>
    /// <param name="attenuation">Attenuation vector (output vector)</param>
    /// <param name="scatterRay">Scatter ray (output vector)</param>
    /// <param name="matColor">Material color vector</param>
    /// <returns></returns>
    public static bool ScatterDiffuse(Ray r, RaycastHit hit, out Vector3 attenuation, out Ray scatterRay, Vector3 matColor)
    {
        Vector3 target = hit.point + hit.normal + UnityEngine.Random.insideUnitSphere;
        scatterRay = new Ray(hit.point, target - hit.point);
        attenuation = matColor;
        return true;
    }

    /// <summary>
    /// Static metal fuzz factor
    /// </summary>
    private static float metalFuzz = 0.3f;

    /// <summary>
    /// Scatter function for metal materials.
    /// Based on "Raytracing in a weekend" C++ function
    /// </summary>
    /// <param name="r">Initial ray</param>
    /// <param name="hit">Raycast hit information</param>
    /// <param name="attenuation">Attenuation vector (output parameter)</param>
    /// <param name="scatterRay">Scatter ray (output parameter)</param>
    /// <param name="matColor">Material color</param>
    /// <returns></returns>
    public static bool ScatterMetal(Ray r, RaycastHit hit, out Vector3 attenuation, out Ray scatterRay, Vector3 matColor)
    {
        // Calculate reflection vector for metal material
        Vector3 reflected = Reflect(Vector3.Normalize(r.direction), hit.normal);

        // Scatter in a direction based on reflection, fuzziness of the material and a random point
        // in the unit sphere above the point
        scatterRay = new Ray(hit.point, reflected + metalFuzz * UnityEngine.Random.insideUnitSphere);

        // Calculate new vectors and determine next hit truth value
        var tmpColor = hit.transform.gameObject.GetComponent<MeshRenderer>().material.color;
        attenuation = new Vector3(tmpColor.r, tmpColor.g, tmpColor.b);
        return (Vector3.Dot(scatterRay.direction, hit.normal) > 0f);
    }

    /// <summary>
    /// Static function to calculate reflection vector.
    /// Based on "Raytracing in a weekend" C++ function
    /// </summary>
    /// <param name="v">Initial entry direction vector</param>
    /// <param name="n">Normal of the hit triangle</param>
    /// <returns>Reflection direction vector</returns>
    public static Vector3 Reflect(Vector3 v, Vector3 n)
    {
        return v - 2f * Vector3.Dot(v, n) * n;
    }

    /// <summary>
    /// Scatter function for dielectric materials.
    /// Based on "Raytracing in a weekend" C++ function.
    /// </summary>
    /// <param name="r">Initial ray</param>
    /// <param name="hit">Raycast hit information</param>
    /// <param name="attenuation">Attenuation vector (output parameter)</param>
    /// <param name="scatterRay">Scatter ray (output parameter)</param>
    /// <param name="refIdx">Refraction index of material</param>
    /// <param name="matColor">Material color</param>
    /// <returns></returns>
    public static bool ScatterDielectric(Ray r, RaycastHit hit, out Vector3 attenuation, out Ray scatterRay, float refIdx, Vector3 matColor)
    {
        // Initialize variables
        Vector3 outwardNormal;
        Vector3 reflected = Reflect(r.direction, hit.normal);
        float ni_over_nt;
        attenuation = new Vector3(1f, 1f, 1f);
        Vector3 refracted;
        float reflect_prob;
        float cosine;


        if (Vector3.Dot(r.direction, hit.normal) > 0)
        {
            outwardNormal = -hit.normal;
            ni_over_nt = refIdx;
            cosine = refIdx * Vector3.Dot(r.direction, hit.normal) / r.direction.magnitude;
        }
        else
        {
            outwardNormal = hit.normal;
            ni_over_nt = 1f / refIdx;
            cosine = -Vector3.Dot(r.direction, hit.normal) / r.direction.magnitude;
        }

        if (Refract(r.direction, outwardNormal, ni_over_nt, out refracted))
        {
            reflect_prob = Schlick(cosine, refIdx);
        }
        else
        {
            scatterRay = new Ray(hit.point, reflected);
            reflect_prob = 1f;
        }

        if (UnityEngine.Random.Range(0f, 1f - 1e-5f) < reflect_prob)
        {
            scatterRay = new Ray(hit.point, reflected);
        }
        else
        {
            scatterRay = new Ray(hit.point, refracted);
        }


        return true;


    }

    /// <summary>
    /// Static function to calculate a refraction vector based on vector information
    /// </summary>
    /// <param name="v">Initial entry direction vector</param>
    /// <param name="n">Normal vector of hit triangle</param>
    /// <param name="ni_over_nt"></param>
    /// <param name="refracted"></param>
    /// <returns></returns>
    public static bool Refract(Vector3 v, Vector3 n, float ni_over_nt, out Vector3 refracted)
    {
        Vector3 uv = Vector3.Normalize(v);
        float dt = Vector3.Dot(uv, n);
        float discriminant = 1f - ni_over_nt * ni_over_nt * (1f - dt * dt);
        if (discriminant > 0)
        {
            refracted = ni_over_nt * (uv - n * dt) - n * Mathf.Sqrt(discriminant);
            return true;
        }

        refracted = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Polynomial approximation (Christophe Schlick).
    /// 
    /// </summary>
    /// <param name="cosine"></param>
    /// <param name="ref_idx"></param>
    /// <returns></returns>
    public static float Schlick(float cosine, float ref_idx)
    {
        float r0 = (1f - ref_idx) / (1f + ref_idx);
        r0 *= r0;
        return r0 + (1f - r0) * Mathf.Pow((1f - cosine), 5f);
    }


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

    // Partial Source: https://forum.unity.com/threads/trying-to-get-color-of-a-pixel-on-texture-with-raycasting.608431/
    /// <summary>
    /// Determine final pixel color based on hit object
    /// </summary>
    /// <param name="hit">Information about the raycast hit</param>
    /// <param name="direction">Direction the ray was shot in</param>
    /// <returns>Calculated pixel color</returns>
    public static Color DetermineHitColor(RaycastHit hit, Vector3 direction)
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
                        //Debug.Log("Metal Hit!");

                        ReflectiveMaterial metalMat = new ReflectiveMaterial(direction, RayTraceUtility.GlobalWorld);
                        metalMat.SetCD(mat.color);
                        return metalMat.Shade(hit, 0);

                        //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Metal, mat.color);

                    case RayTraceUtility.MaterialType.Dielectric:
                        TransparentMaterial dielectricMat =
                            new TransparentMaterial(
                                direction, RayTraceUtility.GlobalWorld,
                                0.5f, 2000f, 1.5f, 0.1f, 0.9f
                            );
                        dielectricMat.SetCD(mat.color);
                        return dielectricMat.Shade(hit, 0);

                        //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Dielectric, mat.color);

                    default:
                    case RayTraceUtility.MaterialType.SolidColor:
                        
                        var tmpMat = new PhongMaterial(direction, RayTraceUtility.GlobalWorld);
                        //Debug.Log("SolidColor - InitMatColor: " + mat.color);
                        tmpMat.SetCD(mat.color);
                        Color tmpColor = tmpMat.Shade(hit, 0);
                        //Debug.Log("SolidColor - ShadedColor: " + tmpColor);
                        return tmpColor;

                        //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.SolidColor, mat.color);
                }
            }

        }
        else
        {
            // On non-hit, return non hit color
            return RayTraceUtility.CreateNonHitColor(direction);
        }

    }

    /// <summary>
    /// Handles additional ray calculations based on initial material hit
    /// </summary>
    /// <param name="hit">Initial raycast hit information</param>
    /// <param name="direction">Initial raycast direction vector</param>
    /// <param name="matType">Type of material hit <see cref="MaterialType"/></param>
    /// <param name="matColor">Color of the hit material</param>
    /// <returns></returns>
    public static Color HandleMaterial(RaycastHit hit, Vector3 direction, RayTraceUtility.MaterialType matType, Color matColor)
    {
        RT_points.Clear();

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
                RT_rec_points.Clear();
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

    /// <summary>
    /// Recursive raytracing used for special materials, i.e. reflection or refraction
    /// of materials
    /// </summary>
    /// <param name="ray">Next ray</param>
    /// <param name="depth">Current recursion depth</param>
    /// <returns></returns>
    private static Vector3 RayTrace_Recursive(Ray ray, int depth)
    {
        // If ray hit another object and distance is above the threshold
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.distance > 1e-3)
        {
            RT_rec_points.Add(hit.point);

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
            if (depth < RayTraceUtility.GlobalWorld.MaxDepth)
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



    public static Color MaxToOne(Color c)
    {
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

    public static Color ClampToColor(Color rawColor)
    {
        Color c = new Color(rawColor.r, rawColor.g, rawColor.g);

        if (rawColor.r > 1f || rawColor.g > 1f || rawColor.b > 1f)
        {
            c.r = 1f; c.g = 0f; c.b = 0f;
        }

        return c;
    }


    public class WorldInformation
    {
        public ViewPortPlaneInformation VP { get; set; }
        public AbstractTracer Tracer { get; set; }

        public int MaxDepth { get; set; } = 10;
        public Color BackgroundColor { get; set; } = Color.black;

        public AbstractLight GlobalAmbientLight { get; set; } //new AmbientLight();

        public List<AbstractLight> GlobalLights { get; set; } = new List<AbstractLight>();

        /// <summary>
        /// 1.0 / 360.0, cached to use multiplication instead of float division
        /// </summary>
        private static readonly float OneDiv360 = 0.00277777778f;
        public WorldInformation(ViewPortPlaneInformation vp)
        {
            VP = vp;

            // ToDo: 1f as step size ok ?
            MultiJitteredSampler ambOccSampler = new MultiJitteredSampler(50, 50, 1f, 1f);

            AmbientOccluder occluder = new AmbientOccluder(ambOccSampler, Color.black);
            occluder.RadianceFactor = 1f;
            occluder.LightColor = Color.white;

            GlobalAmbientLight = occluder;

            // Parse scene lights
            foreach (Light l in Resources.FindObjectsOfTypeAll(typeof(Light)) as Light[])
            {
                switch (l.type)
                {
                    case LightType.Directional:
                        if(true) //l.gameObject.name.Equals("RaytraceDirectionalLight"))
                        {
                            // Direction vector of default direction light path
                            Vector3 dirVector = new Vector3(0, 0, 1);
                            Vector3 lightRotationVec = l.transform.rotation.eulerAngles;
                            //Debug.Log("GlobalLights - DirectionalLight: EulerAngles: " + lightRotationVec);

                            if (lightRotationVec.x != 0f)
                                dirVector = Quaternion.AngleAxis(lightRotationVec.x, Vector3.right) * dirVector;

                            if (lightRotationVec.y != 0f)
                                dirVector = Quaternion.AngleAxis(lightRotationVec.y, Vector3.up) * dirVector;

                            if (lightRotationVec.z != 0f)
                                dirVector = Quaternion.AngleAxis(lightRotationVec.z, Vector3.forward) * dirVector;

                            //float x = (lightRotationVec.x * OneMod360StepSize);
                            //float y = (lightRotationVec.y * OneMod360StepSize);
                            //float z = (lightRotationVec.z * OneMod360StepSize);

                            DirectionalLight tmpLight = new DirectionalLight(dirVector);
                            tmpLight.CastShadows = false;
                            tmpLight.RadianceFactor = l.intensity;

                            GlobalLights.Add(tmpLight);
                            //Debug.Log("GlobalLights - DirectionalLight Added: " + dirVector);
                        }
                        break;

                    case LightType.Point:
                        GlobalLights.Add(new PointLight(l.intensity, l.color, l.transform.position));
                        break;
                }
            }
        }


    }

}

