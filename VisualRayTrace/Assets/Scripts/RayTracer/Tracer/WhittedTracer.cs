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
        //_maxDepth = maxDepth;
        _maxDist = 30f;
        _layerMask = ~(1 << 9);
        _bgColor = Color.black;
    }

    public WhittedTracer(int maxDepth, float maxDist, int layerMask, Color bgColor)
    {
        //_maxDepth = maxDepth;
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

                                ReflectiveMaterial metalMat = new ReflectiveMaterial(ray.direction, RayTraceUtility.GlobalWorld);
                                metalMat.SetKA(RayTraceUtility.Metal_KA);
                                metalMat.SetKD(RayTraceUtility.Metal_KD);
                                metalMat.SetCD(mat.color);
                                metalMat.SetKS(RayTraceUtility.Metal_KS);
                                metalMat.SetExp(RayTraceUtility.Metal_EXP);
                                metalMat.SetKR(RayTraceUtility.Metal_KR);
                                metalMat.SetCR(Color.white);
                                return metalMat.Shade(hit, depth);

                            //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Metal, mat.color);

                            case RayTraceUtility.MaterialType.Dielectric:
                                DielectricMaterial dielectricMat =
                                    new DielectricMaterial(
                                        ray.direction, RayTraceUtility.GlobalWorld,
                                        //0.5f, 2000f, 1.55f, 0.1f, 0.9f
                                        Color.white, Color.white
                                    );

                                dielectricMat.SetKS(RayTraceUtility.Dielectric_KS);
                                dielectricMat.SetExp(RayTraceUtility.Dielectric_EXP);
                                dielectricMat.SetEtaIn(RayTraceUtility.Dielectric_EtaIN);
                                dielectricMat.SetEtaOut(RayTraceUtility.Dielectric_EtaOUT);

                                dielectricMat.SetCD(mat.color); //Color.white);//mat.color);
                                return dielectricMat.Shade(hit, depth);

                            //return HandleMaterial(hit, direction, RayTraceUtility.MaterialType.Dielectric, mat.color);

                            default:
                            case RayTraceUtility.MaterialType.SolidColor:

                                var tmpMat = new PhongMaterial(ray.direction, RayTraceUtility.GlobalWorld);
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
    }

    public override Color TraceRay(Ray ray, float tmin, int depth)
    {
        return TraceRay(ray, depth);
    }
}
