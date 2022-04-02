using HTC.UnityPlugin.ColliderEvent;
using UnityEngine;

namespace Collision
{
    public class ControlRTButtonHitCollision : MonoBehaviour, IColliderEventHoverEnterHandler
    {
        public GameObject RayTracer;

        private RayTracerUnity _rt;

        public enum RTOperation { Stop, Pause, Play }

        public RTOperation Operation;

        public void Start()
        {
            _rt = RayTracer.GetComponent<RayTracerUnity>();
        }

        public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
        {
            switch(Operation)
            {
                case RTOperation.Play:
                    _rt.SetIsRaytracing(true);
                    break;

                case RTOperation.Pause:
                    _rt.SetIsRaytracing(false);
                    break;

                case RTOperation.Stop:
                    _rt.ResetRaytracer();//StopRaytracer();
                    break;            
            }
        }
    }
}
