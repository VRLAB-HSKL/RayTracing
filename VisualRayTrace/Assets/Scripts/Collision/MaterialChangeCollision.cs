using System.Collections.Generic;
using UnityEngine;

namespace Collision
{
    /// <summary>
    /// Generic collision handler that overrides the material of the object that collided
    /// </summary>
    public class MaterialChangeCollision : MonoBehaviour
    {
        /// <summary>
        /// New material that replaces the material on the object
        /// </summary>
        public Material UpdateMaterial;

        /// <summary>
        /// 
        /// </summary>
        private ParticleSystem pSystem;
        private List<ParticleCollisionEvent> collisionEvents;

        // Start is called before the first frame update
        void Start()
        {
            pSystem = GetComponent<ParticleSystem>();
            collisionEvents = new List<ParticleCollisionEvent>();
        }

        void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = pSystem.GetCollisionEvents(other, collisionEvents);   
            bool isSphereCollider = other.GetComponent<Collider>().GetType() == typeof(SphereCollider);

            int i = 0;
            while (i < numCollisionEvents)
            {
                if (isSphereCollider)
                {
                    other.GetComponent<MeshRenderer>().material = UpdateMaterial;
                }
                ++i;
            }         

        }

        void OnCollisionEnter(UnityEngine.Collision colInfo)
        {
            GameObject obj = colInfo.gameObject;

            if(obj.GetComponent<Collider>().GetType() == typeof(SphereCollider))
            {
                obj.GetComponent<MeshRenderer>().material = UpdateMaterial;
            }

        }
    }
}
