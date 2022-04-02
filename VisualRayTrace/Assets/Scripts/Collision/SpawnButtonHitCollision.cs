using HTC.UnityPlugin.ColliderEvent;
using UnityEngine;

namespace Collision
{
    /// <summary>
    /// Script that creates a new prefab instance when an object collision occurs
    /// </summary>
    public class SpawnButtonHitCollision : MonoBehaviour, IColliderEventHoverEnterHandler
    { 
        /// <summary>
        /// Prefab to be instantiated
        /// </summary>
        public GameObject preFab;

        /// <summary>
        /// Spawn point of prefab instances
        /// </summary>
        private Transform SphereSpawn;    

        /// <summary>
        /// Tag string to locate spawn point in scene
        /// </summary>
        private readonly string SpawnTag = "SphereSpawn";

        private float[] arr = new float[3] { -0.125f, 0.0f, 0.125f };

        void Start()
        {
            SphereSpawn = GameObject.FindGameObjectWithTag(SpawnTag).transform;
        }

        void OnCollisionEnter(UnityEngine.Collision colInfo)
        {        
            SpawnSphere();        
        }


        public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
        {
            SpawnSphere();
        }

        private void SpawnSphere()
        {
            // Spawn spheres below the game object and add small random offset for 
            // x and z dimensions to prevent vertical sphere stacking        
            var position = SphereSpawn.position;
            var x = position.x + arr[Random.Range(0, arr.Length)];
            var y = position.y - 1.0f;
            var z = position.z + arr[Random.Range(0, arr.Length)];
            Instantiate(preFab, new Vector3(x, y, z), Quaternion.identity);
        }


    }
}
