using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OptionsCanvas
{
    public class SaveImagePointerClick : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Custom vive activation button
        /// </summary>
        public ControllerButton ActivationButton;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.IsViveButton(ActivationButton))
            {
                
            }
        }
    }
}