using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public GridConnectionNode gridScr;
    public GameObject cursorPoint;
    public CameraMovement cameraMovement;

    public bool importMenuIsOpened;
    public GameObject menuButton;
    public GameObject importMenu;
    public GameObject controlsMenu;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
        {
            ImportSimulationMenu();
            importMenuIsOpened = true;
        }

        if (!importMenuIsOpened)
        {
            Pause(false);
            importMenu.SetActive(false);
            
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
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                importMenuIsOpened = false;
            }
        }
    }

    public void ImportSimulationMenu()
    {
        Pause(true);
        importMenu.SetActive(true);
    }

    public void Pause(bool isPaused)
    {
        gridScr.enabled = !isPaused;
        cursorPoint.SetActive(!isPaused);
        cameraMovement.enabled = !isPaused;
    }
}
