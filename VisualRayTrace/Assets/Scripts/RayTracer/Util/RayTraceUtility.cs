using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RayTraceUtility
{
    /// <summary>
    /// Enum type containing all material types that the raytracer can differentiate between
    /// </summary>
    public enum MaterialType { SolidColor = 1, Metal = 2, Dielectric = 3 };

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


}
