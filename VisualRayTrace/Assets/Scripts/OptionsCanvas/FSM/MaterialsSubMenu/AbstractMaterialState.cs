using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractMaterialState : AbstractState
{
    public AbstractMaterialState(GameObject headerObj, string subMenuName) : base(headerObj, subMenuName)
    {
        //_samplerSubMenuName = name;
    }

    public override void OnStateEntered()
    {
        base.OnStateEntered();

        //SetSamplerContentIndex(0);
    }

    public void SetSamplerContentIndex(int index)
    {
        GameObject subMenu = GameObject.Find(_subMenuName);



        if (subMenu is null) return;

        //if (index < 0 || index < subMenu.transform.childCount - 1) // -1 due to MenuButtons child
        //    return;

        int childCount = subMenu.transform.childCount;
        List<GameObject> childList = new List<GameObject>();

        for (int i = 0; i < childCount; ++i)
        {
            GameObject childObj = subMenu.transform.GetChild(i).gameObject;
            if (childObj.name.Equals("MenuButtons"))
                continue;
            else
                childList.Add(childObj);
        }

        for (int i = 0; i < childList.Count; ++i)
        {
            //Debug.Log(childList[i].name);
            childList[i].SetActive(i == index);
        }

    }
}
