using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractState
{
    protected GameObject _uiCanvas;
    protected string _subMenuName;

    public AbstractState(GameObject canv, string subMenuName)
    {
        _uiCanvas = canv;
        _subMenuName = subMenuName;
    }

    public virtual void OnStateEnter() 
    {
        for (int i = 0; i < _uiCanvas.transform.childCount; ++i)
        {
            GameObject child = _uiCanvas.transform.GetChild(i).gameObject;
            if (child.name.Equals("MenuButtons")) continue;
            child.SetActive(child.name.Equals(_subMenuName));
        }
    }

    public virtual void OnStateExit() { }
}
