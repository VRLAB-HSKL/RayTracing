using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add custom script functions to a <see cref="Toggle"/> UI control ValueChanged event listener collection
/// </summary>
public class ToggleIterationMode : MonoBehaviour
{
    /// <summary>
    /// Raycaster component
    /// </summary>
    private RayTracerUnity _rayCaster;


    /// <summary>
    /// Toggle UI control on options canvas
    /// </summary>
    private Toggle _toggle;


    /// <summary>
    /// Start function called once on setup
    /// </summary>
    void Start()
    {
        _rayCaster = GameObject.Find("RayCaster").GetComponent<RayTracerUnity>();

        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(_toggle); });
    }

    /// <summary>
    /// Modify iteration mode on 
    /// </summary>
    /// <param name="change"></param>
    void ToggleValueChanged(Toggle change)
    {
        _rayCaster.SetIterationMode(change.isOn ? RayTracerUnity.RT_IterationMode.Single : RayTracerUnity.RT_IterationMode.Automatic);
    }
}
