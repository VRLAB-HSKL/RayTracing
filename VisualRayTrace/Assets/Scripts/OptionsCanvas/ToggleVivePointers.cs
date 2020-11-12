using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toggle VivePointer visibility. This is currently used to display the pointers
/// for settings window navigation
/// </summary>
public class ToggleVivePointers : MonoBehaviour
{
    /// <summary>
    /// Scene vivepointers prefab instance
    /// </summary>
    GameObject vivePointers;

    void Start()
    {
        vivePointers = GameObject.Find("VivePointers");
        vivePointers.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Menu))
        {
            vivePointers.SetActive(!vivePointers.activeSelf);
        }
    }
}
