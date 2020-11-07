using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;

/// <summary>
/// Click event handler that exits the application on click
/// </summary>
public class ExitPointerClick : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Custom vive activation button
    /// </summary>
    public ControllerButton ActivationButton;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.IsViveButton(ActivationButton))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
        }
    }
}
