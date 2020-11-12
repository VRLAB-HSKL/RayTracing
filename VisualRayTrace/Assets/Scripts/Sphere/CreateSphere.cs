using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSphere : MonoBehaviour
{
    public GameObject preFab;

    private float[] arr = new float[3] { -0.125f, 0.0f, 0.125f };

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(Input.GetKeyUp("x"))
        {
            SpawnSphere();
        }
        */
    }

    private void SpawnSphereInBasket()
    {
        // Instantiate bound prefab instance
        
        Instantiate(preFab, new Vector3(8 + arr[Random.Range(0, arr.Length)], 1, 8 + arr[Random.Range(0, arr.Length)]), Quaternion.identity);
    }

    private void SpawnSphere()
    {
        // Spawn spheres below the game object and add small random offset for 
        // x and z dimensions to prevent vertical sphere stacking        
        float x = transform.position.x + arr[Random.Range(0, arr.Length)];
        float y = transform.position.y - 1.0f;
        float z = transform.position.z + arr[Random.Range(0, arr.Length)];        
        Instantiate(preFab, new Vector3(x, y, z), Quaternion.identity);        
    }


}
