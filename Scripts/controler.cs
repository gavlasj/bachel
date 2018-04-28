using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controler : MonoBehaviour {

    public plant plant;
    public float maxX;
    public float y;
    public float maxZ;

    public float treeDensity;
    private float baseDensity = 12.0f;



    public bool ShouldPlaceTree(float chance)
    {
        if (Random.Range(0.0f, 1.0f) <= chance)
        {
            return true;
        }
        return false;
    }
    // Use this for initialization
    void Start () {

        for (int i = 2; i < maxX; i++)
        {
            for (int j = 2; j < maxZ; j++)
            {
                float chance = Mathf.PerlinNoise(i, j) / (baseDensity / treeDensity);

                if (ShouldPlaceTree(chance))
                {
                    plant.BuildNew(i, y, j);
                }
            }
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            plant.waveOn = !plant.waveOn;
        }
    }

    
}
