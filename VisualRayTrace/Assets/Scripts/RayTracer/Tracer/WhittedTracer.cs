using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhittedTracer : AbstractTracer
{
    private float _maxDist;
    private int _layerMask;
    private Color _bgColor;

    //private int _maxDepth;


    public WhittedTracer(int maxDepth)
    {
        _maxDist = RayTraceUtility.Raycast_Distance;
        _layerMask = RayTraceUtility.LayerMask;
        _bgColor = RayTraceUtility.GlobalWorld.BackgroundColor;
    }

    public WhittedTracer(int maxDepth, float maxDist, int layerMask, Color bgColor)
    {
        _maxDist = maxDist;
        _layerMask = layerMask;
        _bgColor = bgColor;
    }

    public override Color TraceRay(Ray ray)
    {
        return TraceRay(ray, RayTraceUtility.GlobalWorld.MaxDepth);
    }

    public override Color TraceRay(Ray ray, int depth)
    {
        //Debug.Log("WhittedTracer - TraceRay - Depth: " + depth);

        if(depth > RayTraceUtility.GlobalWorld.MaxDepth)
        {
            return Color.black;
        }
        else
        {
            if(Physics.Raycast(ray, out RaycastHit hit, _maxDist, _layerMask))
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
                                RayTraceUtility.MetalMaterial.SetDir(ray.direction);
                                RayTraceUtility.MetalMaterial.SetCD(mat.color);
                                return RayTraceUtility.MetalMaterial.Shade(hit, depth);

                            case RayTraceUtility.MaterialType.Dielectric:
                                //Debug.Log("Dielectric Hit!");
                                RayTraceUtility.GlassMaterial.SetDir(ray.direction);
                                RayTraceUtility.GlassMaterial.SetCD(mat.color);
                                return RayTraceUtility.GlassMaterial.Shade(hit, depth);

                            default:
                            case RayTraceUtility.MaterialType.SolidColor:
                                //Debug.Log("SolidColor - InitMatColor: " + mat.color);
                                RayTraceUtility.SolidColorMaterial.SetDir(ray.direction);                                
                                RayTraceUtility.SolidColorMaterial.SetCD(mat.color);
                                return RayTraceUtility.SolidColorMaterial.Shade(hit, depth);

                        }
                    }

                }
                else
                {
                    // On non-hit, return non hit color
                    return RayTraceUtility.CreateNonHitColor(ray.direction);
                }

            }
            else
            {
                return RayTraceUtility.CreateNonHitColor(ray.direction);
            }
        }
    }

    public override Color TraceRay(Ray ray, float tmin, int depth)
    {
        return TraceRay(ray, depth);
    }
}
