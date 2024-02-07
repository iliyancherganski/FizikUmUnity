using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    public ItemType ItemType;

    public ConnectionNode[] AllConnections = new ConnectionNode[4];
    public ConnectionNode[] PreviousConnection = new ConnectionNode[4];
    public ConnectionNode[] NextConnection = new ConnectionNode[4];

    public GameObject CurrentObject;

    [Header("Cable Prefabs")]
    public GameObject cableUpRightPrefab_2;
    public GameObject cableLeftRightPrefab_2;
    public GameObject cableUpLeftRightPrefab_3;
    public GameObject cableUpLeftDownRightPrefab_4;

    [Header("Battery Prefabs")]
    public GameObject Battery1;
    public ConnectionNode batteryNode;
    public bool isPositive = false;

    [Header("Lightbulb Prefabs")]
    public GameObject Lightbulb_Off;
    public bool isLit = false;

    [Header("Switch Prefab")]
    public GameObject SwitchPrefab;

    public int Rotation;
    // Start is called before the first frame update
    void Start()
    {
        isPositive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ItemType == ItemType.Battery && batteryNode == null)
        {
            ItemType = ItemType.None;
        }
    }

    public void InstantiateCableNode(bool up, bool left, bool down, bool right)
    {
        if (up)
        {
            if (left)
            {
                if (down)
                {
                    if (right)
                    {
                        // up left down right
                        CurrentObject = Instantiate(cableUpLeftDownRightPrefab_4, this.transform.position, Quaternion.Euler(0, 0, 0));
                    }
                    else
                    {
                        // up left down
                        CurrentObject = Instantiate(cableUpLeftRightPrefab_3, this.transform.position, Quaternion.Euler(0, -90, 0));
                    }
                }
                else if (right)
                {
                    // up left right
                    CurrentObject = Instantiate(cableUpLeftRightPrefab_3, this.transform.position, Quaternion.Euler(0, 0, 0));
                }
                else
                {
                    // up left
                    CurrentObject = Instantiate(cableUpRightPrefab_2, this.transform.position, Quaternion.Euler(0, -90, 0));
                }
            }
            else if (down)
            {
                if (right)
                {
                    // up down right 
                    CurrentObject = Instantiate(cableUpLeftRightPrefab_3, this.transform.position, Quaternion.Euler(0, 90, 0));
                }
                else
                {
                    // up down 
                    CurrentObject = Instantiate(cableLeftRightPrefab_2, this.transform.position, Quaternion.Euler(0, 90, 0));
                }
            }
            else if (right)
            {
                // up right 
                CurrentObject = Instantiate(cableUpRightPrefab_2, this.transform.position, Quaternion.Euler(0, 0, 0));
            }
            else
            {
                // up 
                CurrentObject = Instantiate(cableLeftRightPrefab_2, this.transform.position, Quaternion.Euler(0, 90, 0));
            }
        }
        else if (left)
        {
            if (down)
            {
                if (right)
                {
                    // left down right
                    CurrentObject = Instantiate(cableUpLeftRightPrefab_3, this.transform.position, Quaternion.Euler(0, 180, 0));
                }
                else
                {
                    // left down
                    CurrentObject = Instantiate(cableUpRightPrefab_2, this.transform.position, Quaternion.Euler(0, 180, 0));
                }
            }
            else if (right)
            {
                // left right
                CurrentObject = Instantiate(cableLeftRightPrefab_2, this.transform.position, Quaternion.Euler(0, 0, 0));
            }
            else
            {
                // left
                CurrentObject = Instantiate(cableLeftRightPrefab_2, this.transform.position, Quaternion.Euler(0, 0, 0));
            }
        }
        else if (down)
        {
            if (right)
            {
                // down right
                CurrentObject = Instantiate(cableUpRightPrefab_2, this.transform.position, Quaternion.Euler(0, 90, 0));
            }
            else
            {
                // down
                CurrentObject = Instantiate(cableLeftRightPrefab_2, this.transform.position, Quaternion.Euler(0, 90, 0));
            }
        }
        else
        {
            // none
            CurrentObject = Instantiate(cableLeftRightPrefab_2, this.transform.position, Quaternion.identity);
        }
        ItemType = ItemType.Cable;
    }

    public void InstantiateBatteryNode(int rotation)
    {
        /*if (currObjectIfExists == null)
        {
            CurrentObject = Instantiate(Battery1, this.transform.position, Quaternion.Euler(0, 0, 0));
        }*/
        ItemType = ItemType.Battery;
        CurrentObject = Instantiate(Battery1, this.transform.position, Quaternion.Euler(0, rotation * -90, 0));
        batteryNode = this;
    }

    public void InstantiateLightbulbNode(bool up, bool left, bool down, bool right)
    {
        ItemType = ItemType.Lightbulb;
        CurrentObject = Instantiate(Lightbulb_Off, this.transform.position, Quaternion.Euler(0, 0, 0));

        LightbulbScript lightbulb = CurrentObject.GetComponent<LightbulbScript>();
        if (lightbulb != null)
        {
            lightbulb.SetCorrectCablesOnLightbulb(up, left, down, right);
        }
    }

    public void InstantiateLightbulb_ON_or_OFF(bool isLit)
    {
        if (ItemType == ItemType.Lightbulb && CurrentObject != null)
        {
            LightbulbScript lightbulb = CurrentObject.GetComponent<LightbulbScript>();
            if (lightbulb != null)
            {
                lightbulb.LightIsLit(isLit);
            }
        }
    }

    public void DestroyNode()
    {
        Destroy(CurrentObject);
        if (CurrentObject != null)
        {
            CurrentObject = null;
        }
        //print("reaches here");
        ItemType = ItemType.None;
    }
}
