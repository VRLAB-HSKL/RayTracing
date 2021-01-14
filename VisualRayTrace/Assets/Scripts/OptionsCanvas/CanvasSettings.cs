using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles visibility and position updates of the settings canvas ui
/// </summary>
public class CanvasSettings : MonoBehaviour
{
    /// <summary>
    /// Origin point the settings windows is displayed for
    /// </summary>
    public GameObject origin;

    /// <summary>
    /// Settings canvas used to encapsulated controls
    /// </summary>
    private Canvas canvas;

    private RectTransform canvasRectTransform;



    /// <summary>
    /// Truth value that signals wether the settings canvas is visible or not
    /// </summary>
    private bool _settingsVisible;

    /// <summary>
    /// Distance between view origin and canvas position
    /// </summary>
    private float zAxisOffset = 3f;


    private AbstractState _currentMainState;

    private AbstractState _currentSamplerState;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasRectTransform = GetComponent<RectTransform>();
        canvas.enabled = true;

        SetMainState(new MainMenuState(gameObject, "MainMenu"));

    }

    // Update is called once per frame
    void Update()
    {
        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Menu)) // && !isRaytracing)
        {
            //_settingsVisible = !_settingsVisible;
        }
        
        //canvas.enabled = _settingsVisible;

        if(_settingsVisible)
        {
            //Vector3 originPosition = new Vector3(origin.transform.position.x, origin.transform.position.y, origin.transform.position.z + zAxisOffset);
            //canvasRectTransform.SetPositionAndRotation(originPosition, origin.transform.rotation);
        }
    }   
    
    private void SetMainState(AbstractState state)
    {
        if (_currentMainState != null)
            _currentMainState.OnStateExit();

        _currentMainState = state; //new SamplerMenuState(canvas.gameObject, name);

        _currentMainState.OnStateEnter();
    }

    private void SetSamplersSubMenuState(AbstractSamplerState state)
    {

    }

    public void SwitchToMainMenu(string name)
    {
        SetMainState(new MainMenuState(canvas.gameObject, name));
    }

    public void SwitchToSamplersMenu(string name)
    {
        SetMainState(new SamplerMenuState(canvas.gameObject, name));
    }

    public void SwitchSamplersSubMenuState(int index)
    {
        switch(index)
        {
            case 0:
                _currentSamplerState = new RandomSamplerState(canvas.gameObject, "RandomSamplerInfo");
                break;

            case 1:

                break;

            case 2:

                break;

            case 3:

                break;

            case 4:

                break;

            case 5:

                break;
        }
    }
}
