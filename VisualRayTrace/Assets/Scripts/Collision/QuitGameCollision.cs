using UnityEngine;

namespace Collision
{
    /// <summary>
    /// Collision handler 
    /// </summary>
    public class QuitGameCollision : MonoBehaviour
    {
        void OnCollisionEnter(UnityEngine.Collision col)
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
}
