using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using UnityEngine.SceneManagement;

/// <summary>
/// Click event handler to reset scene on click
/// </summary>
public class ResetSceneClick : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Custom VIVE activation button
    /// </summary>
    public ControllerButton ActivationButton;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.IsViveButton(ActivationButton))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);            
        }
    }
}
