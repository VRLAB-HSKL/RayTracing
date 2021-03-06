using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Source: Unity 5.x Cookbook - Matt Smith, Chico Queiroz 
/// </summary>
public class SliderValueToText : MonoBehaviour
{
    public Slider sliderUI;
    private TMPro.TextMeshProUGUI TextSliderValue;
    
    void Start()
    {
        TextSliderValue = GetComponent<TMPro.TextMeshProUGUI>();    
        ShowSliderValue();
    }

    public void ShowSliderValue()
    {
        if(TextSliderValue != null)
        {
            float value = sliderUI.value;
            string msg = Mathf.Floor(value) == value ? sliderUI.value.ToString() : sliderUI.value.ToString("0.000");
            TextSliderValue.SetText(msg);
        }
    }
}