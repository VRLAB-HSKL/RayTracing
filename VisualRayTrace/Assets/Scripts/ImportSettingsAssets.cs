using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ImportSettingsAssets : MonoBehaviour
{
    public string SettingsCanvasName;

    private void Awake()
    {
        //var setCanv = GameObject.Find("SettingsCanvas");
        //for (int i = 0; i < setCanv.transform.childCount; ++i)
        //{
        //    setCanv.transform.GetChild(i).gameObject.SetActive(true);
        //}


        //SetInfoText("RandomSamplerText", "Samplers/RandomSamplerInfo");
    }

    private void SetInfoText(string gameObjectName, string textAssetRessourcePath)
    {
        TextMeshPro TMesh = GameObject.Find(gameObjectName).GetComponent<TextMeshPro>();
        if (TMesh is null) Debug.Log("GameObject or TextMeshPro Component not found");

        TextAsset textasset = (TextAsset)Resources.Load(textAssetRessourcePath);
        if (textasset is null) Debug.Log("TextAsset Ressource Load failed");

        TMesh.text = textasset.text;
    }

}
