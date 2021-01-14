using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSamplerState : AbstractState
{
    protected string _samplerSubMenuName;
    public AbstractSamplerState(GameObject canv, string name) : base(canv, string.Empty) 
    {
        _samplerSubMenuName = name;
    }

    public override void OnStateEnter()
    {
        //base.OnStateEnter();

        for (int i = 0; i < _uiCanvas.transform.childCount; ++i)
        {
            GameObject child = _uiCanvas.transform.GetChild(i).gameObject;
            if (child.name.Equals("MenuButtons")) continue;
            if (child.name.Equals("SamplersMenu"))
            {
                for(int j = 0; i < child.transform.childCount; ++j)
                {
                    GameObject grandchild = child.transform.GetChild(j).gameObject;
                    if (grandchild.name.Equals("MenuButtons")) continue;
                    grandchild.SetActive(grandchild.name.Equals(_samplerSubMenuName));
                }

                break;
            }
            
            //child.SetActive(child.name.Equals(_subMenuName));
        }
    }
}
