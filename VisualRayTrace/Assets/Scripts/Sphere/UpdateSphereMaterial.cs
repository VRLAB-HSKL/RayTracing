using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSphereMaterial : MonoBehaviour
{
    private Color newColor;
    
    // Start is called before the first frame update
    void Start()
    {
        newColor = new Color(255.0f, 0.0f, 0.0f);
        
    }

    // Update is called once per frame
    void Update()
    {
        // ToDo: Set new color on collision
        newColor = Random.ColorHSV();

        if (Input.GetKeyUp("t"))
        {
            gameObject.GetComponent<Renderer>().material.color = newColor;
        }        
    }
}
