using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to reset camera object position. This is used to place the player
/// in a valid position to edit the values through the settings canvas
/// </summary>
public class ResetPosition : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.System))
        {
            transform.position = new Vector3(0f, 1.75f, 0f);
        }
    }
}
