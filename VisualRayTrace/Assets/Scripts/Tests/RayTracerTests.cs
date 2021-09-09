using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[TestFixture]
public class RayTracerTests : MonoBehaviour
{

    private RayTracerUnity _rayTracer;

    //[TestCase]
    public void Iteration_Mode_Setter()
    {
        _rayTracer.SetIterationMode(RayTracerUnity.RT_IterationMode.Single);
        //Assert.AreEqual(_rayTracer.IterationMode, RT_IterationMode.Single);

        _rayTracer.SetIterationMode(RayTracerUnity.RT_IterationMode.Automatic);
        //Assert.AreEqual(_rayTracer.IterationMode, RT_IterationMode.Automatic);
    }

    //[TestCase]
    public void Path_Visualization_Setter()
    {
        _rayTracer.SetCompleteRTPath(true);
        //Assert.AreEqual(_rayTracer.VisualizeCompleteRTPath, true);

        _rayTracer.SetCompleteRTPath(false);
        //Assert.AreEqual(_rayTracer.VisualizeCompleteRTPath, false);
    }

    //[TestCase]
    public void Check_Setup_Routine()
    {
        
    }

    //[TestCase]
    public void Reset_Raytracer()
    {
        _rayTracer.ResetRaytracer();
        //Assert.AreEqual(_rayTracer.IsRaytracing(), false);
        int[] cp = _rayTracer.GetCurrentPixel();
        //Assert.AreEqual(cp.Length, 2);
        //Assert.AreEqual(cp[0], 0);
        //Assert.AreEqual(cp[1], 0);
    }

    //[TestCase]
    public void NonHit_Color_Range()
    {
        Color actualColor = RayTraceUtility.CreateNonHitColor(Vector3.zero);
        //Assert.True(actualColor.r > .5f);
        //Assert.True(actualColor.g > .7f);
        //Assert.True(Mathf.Abs(actualColor.b - 1f) < 1e-5);
    }

    //[TestCase]
    public void Check_Reflection_Vector()
    {
        Vector3 testV = new Vector3(2f, 2f, 2f);
        Vector3 testN = new Vector3(2f, 2f, 2f);
        //Vector3 actualVector = RayTraceUtility.Reflect(testV, testN);

        //Assert.True(Mathf.Abs(actualVector.x - 46f) < 1e-5);
        //Assert.True(Mathf.Abs(actualVector.y - 46f) < 1e-5);
        //Assert.True(Mathf.Abs(actualVector.z - 46f) < 1e-5);

    }

    //[TestCase]
    public void Check_Refraction_Vector()
    {
        //Vector3 testV = new Vector3(2f, 2f, 2f);
        //Vector3 testN = new Vector3(2f, 2f, 2f);
        //float ni_nt = 1.5f;
        //Vector3 actualRefraction;
        //RayTraceUtility.Refract(testV, testN, ni_nt, out actualRefraction);

        //Assert.True(Mathf.Abs(actualVector.x - 32f) < 1e-5);
        //Assert.True(Mathf.Abs(actualVector.y - 32f) < 1e-5);
        //Assert.True(Mathf.Abs(actualVector.z - 32f) < 1e-5);
    }

    //[TestCase]
    public void Check_Schlick()
    {
        //float cosine = 1f;
        //float ref_idx = 1.5f;
        //float actualResult = RayTraceUtility.Schlick(cosine, ref_idx);

        //Assert.True(Mathf.Abs(actualResult - 1f) < 1e-5);
    }

}
