using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

 
/// <summary> 
/// Translate canvas UI slider values to the corresponding material color
/// </summary>
public class ColorPickerSlider : MonoBehaviour
{
    
    /// <summary>
    /// Preview object in settings canvas
    /// </summary>
    public GameObject PreviewObject;

    /// <summary>
    /// Static material asset to be changed
    /// </summary>
    public Material _solidColorMat;

    /// <summary>
    /// Slider to control red channel of the color
    /// </summary>
    public Slider _redSlider;


    /// <summary>
    /// Slider to control green channel of the color
    /// </summary>
    public Slider _greenSlider;

    /// <summary>
    /// Slider to control blue channel of the color
    /// </summary>
    public Slider _blueSlider;
    

    /// <summary>
    /// Image component of preview object
    /// </summary>
    private Image _img;
    

    // Start is called before the first frame update
    void Start()
    {
        _img = PreviewObject.GetComponent<Image>();
        
    }

    // Update is called once per frame
    void Update()
    {
        var color = new Color(_redSlider.value, _greenSlider.value, _blueSlider.value);
        _img.color = color;
        _solidColorMat.color = color;
    }
}
