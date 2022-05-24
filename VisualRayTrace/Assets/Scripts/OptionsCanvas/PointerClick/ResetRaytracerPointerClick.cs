using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OptionsCanvas
{
    public class ResetRaytracerPointerClick : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Custom vive activation button
        /// </summary>
        public ControllerButton ActivationButton;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.IsViveButton(ActivationButton))
            {
                var rt = FindObjectOfType<RayTracerUnity>();
                rt.ResetRaytracer();
            }
        }
    }
}