using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSamplerState : AbstractState
{
    //protected string _samplerSubMenuName;
    public AbstractSamplerState(GameObject headerObj, string subMenuName) : base(headerObj, subMenuName) 
    {
        //_samplerSubMenuName = name;
    }

    public override void OnStateEntered()
    {
        base.OnStateEntered();

        // Hide all children except current selection and selection buttons
        //for (int i = 0; i < _uiHeaderElement.transform.childCount; ++i)
        //{
        //    GameObject child = _uiHeaderElement.transform.GetChild(i)?.gameObject;
        //    if (child is null) continue;
        //    if (child.name.Equals("MenuButtons")) continue;
        //    if (child.name.Equals("SamplersMenu"))
        //    {
        //        if(child.transform.childCount > 0)
        //        {
        //            for (int j = 0; i < child.transform.childCount; ++j)
        //            {
        //                GameObject grandchild = child.transform.GetChild(j)?.gameObject;
        //                if (grandchild is null) continue;
        //                if (grandchild.name.Equals("MenuButtons")) continue;
        //                grandchild.SetActive(grandchild.name.Equals(_samplerSubMenuName));
        //            }
        //        }               
        //    }

        //    //child.SetActive(child.name.Equals(_subMenuName));
        //}

        SetSamplerContentIndex(0);
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
