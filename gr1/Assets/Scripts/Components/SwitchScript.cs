using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScript : MonoBehaviour
{
    public GameObject Cable1;
    public GameObject Cable2;
    public GameObject Cable3;
    public GameObject Cable4;
    public GameObject Light;

    public GameObject ButtonON;
    public GameObject ButtonOFF;
    public bool isTurnedOn = false;

    public void SetCorrectCablesOnSwitch(bool[] directions)
    {
        Cable1.SetActive(false);
        Cable2.SetActive(false);
        Cable3.SetActive(false);
        Cable4.SetActive(false);

        if (directions[0])
        {
            Cable1.SetActive(true);
        }
        if (directions[1])
        {
            Cable2.SetActive(true);
        }
        if (directions[2])
        {
            Cable3.SetActive(true);
        }
        if (directions[3])
        {
            Cable4.SetActive(true);
        }
    }
    public bool ButtonIsOn(bool isOn)
    {
        if (isOn)
        {
            isTurnedOn = true;
            ButtonON.SetActive(true);
            ButtonOFF.SetActive(false);
        }
        else
        {
            isTurnedOn = false;
            ButtonON.SetActive(false);
            ButtonOFF.SetActive(true);
        }
        return isOn;
    }
    public bool HasFlowingElectricity(bool hasElectricity)
    {
        if (hasElectricity && isTurnedOn)
        {
            Light.SetActive(true);
        }
        else
        {
            Light.SetActive(false);
        }
        return hasElectricity;
    }
}
