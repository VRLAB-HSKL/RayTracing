using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public string InitMenuName = "MainMenu";
    public string InitSamplerMenuName = "RandomSamplerInfo";
    public string InitMaterialMenuName = "SolidColorSubmenu";

    private RectTransform canvasRectTransform;


    public GameObject SamplersSubMenu;
    
    public TMP_Dropdown SamplerDropdown;

    public UnityEngine.UI.Slider SamplersContentSlider;


    public GameObject MaterialsSubMenu;


    /// <summary>
    /// Truth value that signals wether the settings canvas is visible or not
    /// </summary>
    private bool _settingsVisible;

    /// <summary>
    /// Distance between view origin and canvas position
    /// </summary>
    private float zAxisOffset = 3f;

    private AbstractState _currentMainState;

    private AbstractSamplerState _currentSamplerState;

    private AbstractMaterialState _currentMaterialState;


    public List<KeyValuePair<string, string>> StringAssetImportPairs = new List<KeyValuePair<string, string>>()
    {
        new KeyValuePair<string, string>("RandomSamplerText", "Samplers/Textfiles/RandomSamplerInfo"),
        //new KeyValuePair<string, string>("RegularSamplerText", "Samplers/Textfiles/RegularSamplerInfo"),
        new KeyValuePair<string, string>("JitteredSamplerText", "Samplers/Textfiles/JitteredSamplerInfo"),
        new KeyValuePair<string, string>("NRooksSamplerText", "Samplers/Textfiles/NRooksSamplerInfo"),
        new KeyValuePair<string, string>("MultiJitteredSamplerText", "Samplers/Textfiles/MultiJitteredSamplerInfo"),
        new KeyValuePair<string, string>("HamersleySamplerText", "Samplers/Textfiles/HamersleySamplerInfo")
    };

    public List<KeyValuePair<string, string>> ImageAssetImportPairs = new List<KeyValuePair<string, string>>()
    {
        new KeyValuePair<string, string>("RandomSamplerImage01", "Samplers/Images/RandomSampler01"),
        new KeyValuePair<string, string>("RandomSamplerImage02", "Samplers/Images/RandomSampler02"),
        //new KeyValuePair<string, string>("RegularSamplerImage01", "Samplers/Images/RegularSampler01"),
        //new KeyValuePair<string, string>("RegularSamplerImage02", "Samplers/Images/RegularSampler02"),
        new KeyValuePair<string, string>("JitteredSamplerImage01", "Samplers/Images/JitteredSampler01"),
        new KeyValuePair<string, string>("JitteredSamplerImage02", "Samplers/Images/JitteredSampler02"),
        new KeyValuePair<string, string>("NRooksSamplerImage01", "Samplers/Images/NRooksSampler01"),
        new KeyValuePair<string, string>("NRooksSamplerImage02", "Samplers/Images/NRooksSampler02"),
        new KeyValuePair<string, string>("MultiJitteredSamplerImage01", "Samplers/Images/MultiJitteredSampler01"),
        new KeyValuePair<string, string>("MultiJitteredSamplerImage02", "Samplers/Images/MultiJitteredSampler02"),
        new KeyValuePair<string, string>("HamersleySamplerImage01", "Samplers/Images/HamersleySampler01"),
        new KeyValuePair<string, string>("HamersleySamplerImage02", "Samplers/Images/HamersleySampler02"),
    };

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasRectTransform = GetComponent<RectTransform>();
        canvas.enabled = true;

        /*
        // Import string assets
        foreach(KeyValuePair<string, string> pair in StringAssetImportPairs)
        {
            //SetInfoText("RandomSamplerText", "Samplers/RandomSamplerInfo");
            SetInfoText(pair.Key, pair.Value);
        }

        // Import image assets
        foreach(KeyValuePair<string, string> pair in ImageAssetImportPairs)
        {
            SetImage(pair.Key, pair.Value);
        }
        */

        // Set menu states
        SetMainState(new MainMenuState(gameObject, InitMenuName));
        SetSamplersSubMenuState(new RandomSamplerState(SamplersSubMenu, InitSamplerMenuName));
        SetMaterialsSubMenuState(new SolidColorMaterialState(MaterialsSubMenu, InitMaterialMenuName));

        // Set sliders
        MetalKASlider.value = RayTraceUtility.Metal_KA;
        MetalKDSlider.value = RayTraceUtility.Metal_KD;
        MetalKSSlider.value = RayTraceUtility.Metal_KS;
        MetalExpSlider.value = RayTraceUtility.Metal_EXP;
        MetalKRSlider.value = RayTraceUtility.Metal_KR;

        DielectricKSSlider.value = RayTraceUtility.Dielectric_KS;
        DielectricExpSlider.value = RayTraceUtility.Dielectric_EXP;
        DielectricEtaInSlider.value = RayTraceUtility.Dielectric_EtaIN;
        DielectricEtaOutSlider.value = RayTraceUtility.Dielectric_EtaOUT;
    }

    private void SetInfoText(string gameObjectName, string textAssetRessourcePath)
    {
        GameObject gameObj = GameObject.Find(gameObjectName);
        if (gameObj is null) Debug.Log("TextImport - " + gameObjectName + " not found");

        TextMeshProUGUI TMesh = gameObj.GetComponent<TextMeshProUGUI>();
        if (TMesh is null) Debug.Log("TextImport - TextMeshPro Component not found on " + gameObjectName);

        TextAsset textasset = (TextAsset)Resources.Load(textAssetRessourcePath);
        if (textasset is null) Debug.Log("TextImport - " + textAssetRessourcePath + ": Ressource Load failed");

        TMesh.text = textasset.text;
    }

    private void SetImage(string gameObjectName, string imageAssetResourcePath)
    {
        GameObject gameObj = GameObject.Find(gameObjectName);
        if (gameObj is null) Debug.Log("ImageImport - " + gameObjectName + " not found");

        Image ImgComp = gameObj.GetComponent<Image>();
        if (ImgComp is null) Debug.Log("ImageImport - Image Component not found on " + gameObjectName);

        Sprite spriteAsset = Resources.Load<Sprite>(imageAssetResourcePath);
        if (spriteAsset is null) Debug.Log("ImageImport - " + imageAssetResourcePath + ": Ressource Load failed");

        ImgComp.sprite = spriteAsset;
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
            _currentMainState.OnStateQuit();

        _currentMainState = state; //new SamplerMenuState(canvas.gameObject, name);

        _currentMainState.OnStateEntered();
    }

    private void SetSamplersSubMenuState(AbstractSamplerState state)
    {
        if (_currentSamplerState != null)
            _currentSamplerState.OnStateQuit();

        _currentSamplerState = state;

        _currentSamplerState.OnStateEntered();
    }

    private void SetMaterialsSubMenuState(AbstractMaterialState state)
    {
        if (_currentMaterialState != null)
            _currentMaterialState.OnStateQuit();

        _currentMaterialState = state;

        _currentMaterialState.OnStateEntered();
    }

    public void SwitchToMainMenu(string name)
    {
        SetMainState(new MainMenuState(canvas.gameObject, name));
    }

    public void SwitchToSamplersMenu(string name)
    {
        SetMainState(new SamplerMenuState(canvas.gameObject, name));
    }

    public void SwitchToMaterialsMenu(string name)
    {
        SetMainState(new MaterialMenuState(canvas.gameObject, name));
    }

    public void SwitchSamplersSubMenuState(int index)
    {
        //Debug.Log("ValueChanged: " + SamplerDropdown.value);
        //int index = SamplerDropdown.value;

        switch(index)
        {
            case 0:
                SetSamplersSubMenuState(new RandomSamplerState(SamplersSubMenu, "RandomSamplerInfo"));                
                break;

            //case 1:
            //    SetSamplersSubMenuState(new RegularSamplerState(SamplersSubMenu, "RegularSamplerInfo"));
            //    break;

            case 1:
                SetSamplersSubMenuState(new JitteredSamplerState(SamplersSubMenu, "JitteredSamplerInfo"));
                break;

            case 2:
                SetSamplersSubMenuState(new NRooksSamplerState(SamplersSubMenu, "NRooksSamplerInfo"));
                break;

            case 3:
                SetSamplersSubMenuState(new MultiJitteredSamplerState(SamplersSubMenu, "MultiJitteredSamplerInfo"));
                break;

            case 4:
                SetSamplersSubMenuState(new HamersleySamplerState(SamplersSubMenu, "HamersleySamplerInfo"));
                break;
        }
    }


    public enum MaterialSubMenuEntry { SolidColor, Metal, Dielectric};


    public void SwitchMaterialsSubMenuState(int index)
    {
        switch(index)
        {
            case 0: //MaterialSubMenuEntry.SolidColor:
                SetMaterialsSubMenuState(new SolidColorMaterialState(MaterialsSubMenu, "SolidColorSubmenu"));
                break;

            case 1: // MaterialSubMenuEntry.Metal:
                SetMaterialsSubMenuState(new MetalMaterialState(MaterialsSubMenu, "MetalSubmenu"));
                break;

            case 2: // MaterialSubMenuEntry.Dielectric:
                SetMaterialsSubMenuState(new DielectricMaterialState(MaterialsSubMenu, "DielectricSubmenu"));
                break;
        }
    }

    #region Material Values

    [Header("Metal Parameters")]
    public Slider MetalKASlider;
    public Slider MetalKDSlider;
    public Slider MetalKSSlider;
    public Slider MetalExpSlider;
    public Slider MetalKRSlider;

    public void UpdateMetalKA()
    {
        RayTraceUtility.Metal_KA = MetalKASlider.value;
    }

    public void UpdateMetalKD()
    {
        RayTraceUtility.Metal_KD = MetalKDSlider.value;
    }

    public void UpdateMetalKS()
    {
        RayTraceUtility.Metal_KS = MetalKSSlider.value;
    }

    public void UpdateMetalExp()
    {
        RayTraceUtility.Metal_EXP = (int)MetalExpSlider.value;
    }

    public void UpdateMetalKR()
    {
        RayTraceUtility.Metal_KR = MetalKRSlider.value;
    }

    [Header("Dielectric Parameters")]
    public Slider DielectricKSSlider;
    public Slider DielectricExpSlider;
    public Slider DielectricEtaInSlider;
    public Slider DielectricEtaOutSlider;

    public void UpdateDielectricKS()
    {
        RayTraceUtility.Dielectric_KS = DielectricKSSlider.value;
    }

    public void UpdateDielectricExp()
    {
        RayTraceUtility.Dielectric_EXP = DielectricExpSlider.value;
    }

    public void UpdateDielectricEtaIn()
    {
        RayTraceUtility.Dielectric_EtaIN = DielectricEtaInSlider.value;
    }

    public void UpdateDielectricEtaOut()
    {
        RayTraceUtility.Dielectric_EtaOUT = DielectricEtaOutSlider.value;
    }

    #endregion Material Values


    public void ExitApplication()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
        
    }
}
