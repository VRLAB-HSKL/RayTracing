using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractState
{
    protected GameObject _uiHeaderElement;
    protected string _subMenuName;

    public AbstractState(GameObject headerObject, string subMenuName)
    {
        _uiHeaderElement = headerObject;
        _subMenuName = subMenuName;
    }

    public virtual void OnStateEntered() 
    {
        // Hide all children except current selection and selection buttons
        for (int i = 0; i < _uiHeaderElement.transform.childCount; ++i)
        {
            GameObject child = _uiHeaderElement.transform.GetChild(i).gameObject;
            if (child.name.Equals("MenuButtons")) continue;
            child.SetActive(child.name.Equals(_subMenuName));
        }
    }

    public virtual void OnStateUpdate() { }

    public virtual void OnStateQuit() { }
}
