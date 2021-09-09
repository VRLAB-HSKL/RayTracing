using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewPortStartPoint { UpperLeft, UpperRight, LowerLeft, LowerRight }

// ToDo: Doc this
public class StartPointInformation
{
    public int ResetXValue = -1;
    public int ResetYValue = -1;
    public int InitXValue = -1;
    public int InitYValue = -1;
    public int IncrementGreaterValue = 0;
    public int IncrementLesserValue = 0;
    public int LesserCordIdx = -1;
    public int GreaterCordIdx = -1;

    public StartPointInformation(ViewPortStartPoint sp, int textureDimension)
    {
        switch (sp)
        {
            case ViewPortStartPoint.UpperLeft:
                InitXValue = 0;
                InitYValue = textureDimension;
                ResetXValue = textureDimension;
                ResetYValue = 0;
                IncrementGreaterValue = -1;
                IncrementLesserValue = 1;
                GreaterCordIdx = 0;
                LesserCordIdx = 1;
                break;

            case ViewPortStartPoint.UpperRight:
                break;

            case ViewPortStartPoint.LowerLeft:
                InitXValue = 0;
                InitYValue = 0;
                ResetXValue = textureDimension;
                ResetYValue = textureDimension;
                IncrementGreaterValue = 1;
                IncrementLesserValue = 1;
                GreaterCordIdx = 0;
                LesserCordIdx = 1;
                break;

            case ViewPortStartPoint.LowerRight:
                break;
        }

    }
}

