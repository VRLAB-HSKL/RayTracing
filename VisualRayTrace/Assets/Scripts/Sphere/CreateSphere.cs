using UnityEngine;

namespace Sphere
{
    public class CreateSphere : MonoBehaviour
    {
        public GameObject preFab;

        private readonly float[] _arr = new float[3] { -0.125f, 0.0f, 0.125f };

        // private void SpawnSphereInBasket()
        // {
        //     // Instantiate bound prefab instance
        //     Instantiate(preFab, 
        //         new Vector3(8 + arr[Random.Range(0, arr.Length)], 1, 
        //             8 + arr[Random.Range(0, arr.Length)]), Quaternion.identity);
        // }

        private void SpawnSphere()
        {
            // Spawn spheres below the game object and add small random offset for 
            // x and z dimensions to prevent vertical sphere stacking        
            var position = transform.position;
            var x = position.x + _arr[Random.Range(0, _arr.Length)];
            var y = position.y - 1.0f;
            var z = position.z + _arr[Random.Range(0, _arr.Length)];        
            Instantiate(preFab, new Vector3(x, y, z), Quaternion.identity);        
        }

    }
}
