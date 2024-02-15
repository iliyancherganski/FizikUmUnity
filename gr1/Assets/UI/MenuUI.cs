using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class MenuUI : MonoBehaviour
{
    public GameObject menuButton;
    public GameObject controlsMenu;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            menuButton.SetActive(false);
            controlsMenu.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            menuButton.SetActive(true);
            controlsMenu.SetActive(false);
        }
    }
}
