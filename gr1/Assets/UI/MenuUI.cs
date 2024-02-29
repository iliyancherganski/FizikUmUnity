using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUI : MonoBehaviour
{
    public GridConnectionNode gridScr;
    public ImportExportGridScript importExportScript;
    public GameObject cursorPoint;
    public CameraMovement cameraMovement;

    public bool importMenuIsOpened;
    public GameObject menuButton;
    public GameObject importMenu;
    public GameObject controlsMenu;

    public TMP_InputField jsonInputField;

    public TextMeshProUGUI messageBox;



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.V) && !importMenuIsOpened)
        {
            ImportSimulationMenu();
            importMenuIsOpened = true;
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
        {
            messageBox.text = "Симуацията е копирана.";
            Invoke(nameof(MessageBoxClearInvoke), 4);
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

    public void CancelButton()
    {
        Pause(false);
        importMenuIsOpened = false;
    }

    public void SubmitJsonButton()
    {
        if (!string.IsNullOrWhiteSpace(jsonInputField.text))
        {
            messageBox.text = importExportScript.GridImport(jsonInputField.text);
            Invoke(nameof(MessageBoxClearInvoke), 4);
            importMenuIsOpened = false;
        }
    }

    public void MessageBoxClearInvoke()
    {
        messageBox.text = "";
    }
}
