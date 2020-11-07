using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collision handler 
/// </summary>
public class QuitGameCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        // Make sure application doesn't exit when a sphere rolls into exit door...
        bool isSphereCollider = col.collider.GetType() == typeof(SphereCollider);
        if (isSphereCollider) return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

}
