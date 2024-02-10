using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightbulbScript : MonoBehaviour
{
    public GameObject Cable1;
    public GameObject Cable2;
    public GameObject Cable3;
    public GameObject Cable4;
    public GameObject Light;
    public GameObject EmissionObject;

    public Material LightMaterialON;
    public Material LightMaterialOFF;

    public void SetCorrectCablesOnLightbulb(bool up, bool left, bool down, bool right)
    {
        Cable1.SetActive(false);
        Cable2.SetActive(false);
        Cable3.SetActive(false);
        Cable4.SetActive(false);

        if (up)
        {
            Cable1.SetActive(true);
        }
        if (left)
        {
            Cable2.SetActive(true);
        }
        if (down)
        {
            Cable3.SetActive(true);
        }
        if (right)
        {
            Cable4.SetActive(true);
        }
    }
    public void LightIsLit(bool isLit)
    {
        Renderer renderer = EmissionObject.GetComponent<Renderer>();
        if (isLit)
        {
            Light.SetActive(true);
            renderer.material = LightMaterialON;
        }
        else
        {
            Light.SetActive(false);
            renderer.material = LightMaterialOFF;
        }
    }
}
