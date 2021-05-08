using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuState : AbstractState
{
    public MainMenuState(GameObject canv, string subMenuName) : base(canv, subMenuName) { }

    public override void OnStateEntered()
    {
        base.OnStateEntered();        
    }
}
