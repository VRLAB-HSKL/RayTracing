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

    ControllerButton ToggleButton = ControllerButton.Menu;
    //float elapsedTime = 0f;

    void Start()
    {
        vivePointers = GameObject.Find("VivePointers");
        vivePointers.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //elapsedTime += Time.deltaTime;
        //if(elapsedTime >= 1f)
        //{
        //    elapsedTime = elapsedTime % 1f;
            
        //}

        CheckPointerUpdate();

        //else if(ViveInput.GetPressUp(HandRole.RightHand, ControllerButton.PadTouch))
        //{
        //    vivePointers.SetActive(false);
        //    Debug.Log("VivePointers deactivated");
        //}


    }

    public void CheckPointerUpdate()
    {
        if (ViveInput.GetPressDown(HandRole.RightHand, ToggleButton))
        {
            //for(int i = 0; i < vivePointers.transform.childCount; i++)
            //{
            //    GameObject child = vivePointers.transform.GetChild(i).gameObject;
            //    child.SetActive(true);
            //}

            vivePointers.SetActive(!vivePointers.activeSelf);
            //Debug.Log("VivePointers activated");
        }
        
        
    }

}
