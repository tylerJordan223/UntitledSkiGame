using UnityEngine;
using System.Collections.Generic;
using System;

public class SlalomCourse : MonoBehaviour
{
    [SerializeField] public List<GameObject> slalomFlags;

    [SerializeField] public Material daRed;
    [SerializeField] public Material daBlue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < slalomFlags.Count; i++)
        {
            if (slalomFlags[i] == null)
            {
                Debug.LogError("Slalom flag at index " + i + " is not assigned in the inspector.");
            }
            else // Set all object children of SlalomFlag Object to alternating blue and red
            {
                Renderer[] renderers = slalomFlags[i].GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (i % 2 == 0) 
                        renderer.material = daBlue;
                    else
                        renderer.material = daRed;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
