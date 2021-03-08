using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper class to encapsulate Texture information and calculation functions
/// </summary>
public class TextureInformation
{
    /// <summary>
    /// Dimension of texture. Currenty the texture is square
    /// and has the same horizontal and vertical dimension.
    /// </summary>
    public int TextureDimension { get; set; }

    /// <summary>
    /// Render texture that pixel color information is transmitted to
    /// </summary>
    public RenderTexture Texture { get; set; }

    /// <summary>
    /// Raw texture that is drawn in
    /// </summary>
    public Texture2D StreamTexture2D { get; set; }

    /// <summary>
    /// Width of the visual line of the LineRenderer.
    /// The width of the line is determined based on texture dimension (more pixels => smaller ray)
    /// </summary>
    public float VisualLineWidth { get; set; }

    /// <summary>
    /// Raw array containing pixel color information. On change, the complete
    /// array is transmitted as the new texture image data.
    /// </summary>
    public Color[] PixelColorData { get; }

    /// <summary>
    /// Static number of pixels for each texture scaling unit
    /// </summary>
    private int _scalingPixelCount = 100;

    /// <summary>
    /// Argument constructor
    /// </summary>
    /// <param name="scaleFactor">Texture scale factor</param>
    /// <param name="planeWidth">Width of viewport gameobject</param>
    /// <param name="planeHeight">Height of viewport gameobject</param>
    public TextureInformation(int scaleFactor, float planeWidth, float planeHeight)
    {
        TextureDimension = scaleFactor * _scalingPixelCount;
        VisualLineWidth = 0.025f * (1f / scaleFactor);
        Texture = new RenderTexture(TextureDimension, TextureDimension, 1);

        int pixelCount = TextureDimension * TextureDimension;

        // Initialize color array
        PixelColorData = new Color[pixelCount];
        Color initColor = new Color(0, 0, 0);
        for (int i = 0; i < pixelCount; ++i)
        {
            PixelColorData[i] = initColor;
        }

        StreamTexture2D = new Texture2D(TextureDimension, TextureDimension);
    }

    /// <summary>
    /// Setter for color array value
    /// </summary>
    /// <param name="index">Absolute index in array</param>
    /// <param name="col">New color value</param>
    public void SetPixelColor(int index, Color col)
    {
        // Check invalid values
        if (index < 0 || index > PixelColorData.Length - 1) return;

        PixelColorData[index] = col;
    }

    /// <summary>
    /// Resets the texture
    /// </summary>
    public void ResetTexture()
    {
        Texture = new RenderTexture(TextureDimension, TextureDimension, 1);
    }
}
