using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastTracer : AbstractTracer
{
    private float _maxDist;
    private int _layerMask;
    private Color _bgColor;

    public RayCastTracer()
    {
        _maxDist = RayTraceUtility.Raycast_Distance;
        _layerMask = RayTraceUtility.LayerMask;
        _bgColor = RayTraceUtility.GlobalWorld.BackgroundColor;
    }

    public RayCastTracer(float maxDist, int layerMask, Color bgColor)
    {
        _maxDist = maxDist;
        _layerMask = layerMask;
        _bgColor = bgColor;
    }


    public override Color TraceRay(Ray ray)
    {
        return TraceRay(ray, 0);

        //throw new System.NotImplementedException();       
    }

    public override Color TraceRay(Ray ray, int depth)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDist, _layerMask))
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
                            RayTraceUtility.MetalMaterial.SetCD(mat.color); 
                            RayTraceUtility.MetalMaterial.SetDir(ray.direction);
                            RayTraceUtility.MetalMaterial.SetCR(Color.white);
                            return RayTraceUtility.MetalMaterial.Shade(hit, depth);
                                                    
                        case RayTraceUtility.MaterialType.Dielectric:
                            RayTraceUtility.GlassMaterial.SetDir(ray.direction);
                            RayTraceUtility.GlassMaterial.SetCD(mat.color);
                            return RayTraceUtility.GlassMaterial.Shade(hit, depth);
                                                    
                        default:
                        case RayTraceUtility.MaterialType.SolidColor:

                            var tmpMat = new PhongMaterial(ray.direction);
                            //Debug.Log("SolidColor - InitMatColor: " + mat.color);
                            tmpMat.SetCD(mat.color);
                            Color tmpColor = tmpMat.Shade(hit, depth);
                            //Debug.Log("SolidColor - ShadedColor: " + tmpColor);
                            return tmpColor;

                            //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.SolidColor, mat.color);
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

    public override Color TraceRay(Ray ray, float tmin, int depth)
    {
        throw new System.NotImplementedException();
    }


}
