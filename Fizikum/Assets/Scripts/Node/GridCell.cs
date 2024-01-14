using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public CellStatus cellStatus;
    public bool isHovered = false;
    public bool inPlacementMode = false;
    public GameObject _connectionNodePrefab;
    public ConnectionNode connectionNode;

    [Header("Indexes")]
    public int GridIndex_X;
    public int GridIndex_Y;
    public int ItemRotation;

    [Header("Materials")]
    public Material Unselected;
    public Material Hovered;
    public Material Selected;
    public Material FreeCell;
    public Material OccupiedCell;

    [Header("Keybinds")]
    public KeyCode SelectKey = KeyCode.LeftShift;
    public KeyCode DeselectKey = KeyCode.LeftControl;
    public KeyCode PlaceCableKey = KeyCode.Alpha1;

    public bool isOccupied = false;
    public bool isFree = false;


    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "CursorPoint")
        {
            isHovered = true;
            if (cellStatus != CellStatus.Selected && Cursor.lockState != CursorLockMode.Locked && !isOccupied && !isFree)
            {
                cellStatus = CellStatus.Hovered;
            }
            // Debug.Log("Cursor hits the cell");

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "CursorPoint")
        {
            isHovered = false;
            if (cellStatus != CellStatus.Selected && !isOccupied && !isFree)
            {
                cellStatus = CellStatus.Unselected;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (connectionNode == null)
        {
            InstantiateConnectionNodePrefab();
        }
        SelectSystem();
        //CreateConnectionNode();
        SetCorrectMaterail();
    }
    private void SelectSystem()
    {
        if (isOccupied || isFree)
        {
            return;
        }
        else if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButton(0) && !Input.GetKey(SelectKey) && !Input.GetKey(DeselectKey))
            {
                cellStatus = CellStatus.Unselected;
                if (isHovered)
                {
                    cellStatus = CellStatus.Selected;
                }
            }
            if (isHovered)
            {
                if (Input.GetMouseButton(0))
                {
                    // SELECT
                    if (cellStatus != CellStatus.Selected && Input.GetKey(SelectKey))
                    {
                        cellStatus = CellStatus.Selected;
                    }
                    // DESELECT
                    else if (cellStatus == CellStatus.Selected && Input.GetKey(DeselectKey))
                    {
                        cellStatus = CellStatus.Unselected;
                    }
                }
            }
        }
    }
    private void SetCorrectMaterail()
    {
        Renderer renderer = this.GetComponent<Renderer>();
        switch (cellStatus)
        {
            case CellStatus.Unselected:
                renderer.material = Unselected;
                break;
            case CellStatus.Hovered:
                renderer.material = Hovered;
                break;
            case CellStatus.Selected:
                renderer.material = Selected;
                break;
            case CellStatus.Occupied:
                renderer.material = OccupiedCell;
                break;
            case CellStatus.Free:
                renderer.material = FreeCell;
                break;
            default:
                break;
        }
    }
    private void CreateConnectionNode()
    {
        if (cellStatus == CellStatus.Selected)
        {
            if (Input.GetKeyDown(PlaceCableKey))
            {
                if (connectionNode == null)
                {
                    //connectionNode = Instantiate(_connectionNodePrefab, this.gameObject.transform.position, Quaternion.identity, this.transform).GetComponent<ConnectionNode>();
                    //connectionNode.InstantiateNode();
                }
                else if (connectionNode.CurrentObject == null)
                {
                    //connectionNode.InstantiateNode();
                }
            }
        }
    }

    public void InstantiateConnectionNodePrefab()
    {
        connectionNode = Instantiate(_connectionNodePrefab, this.gameObject.transform.position, Quaternion.identity, this.transform).GetComponent<ConnectionNode>();
    }

    public void SpaceIsOccupied(bool asTemporalWarning)
    {
        if (asTemporalWarning)
        {
            isOccupied = true;
            //print("turned on");
            Invoke(nameof(Invoke_OccupiedWhenPlaced), 1f);
            cellStatus = CellStatus.Occupied;
        }
        else
        {
            isOccupied = true;
            cellStatus = CellStatus.Occupied;
        }
    }
    public void SpaceIsFree()
    {
        isFree = true;
        cellStatus = CellStatus.Free;
    }

    public void DeletePrefab()
    {
        connectionNode.DestroyNode();
        InstantiateConnectionNodePrefab();
    }

    public void InstantiateCorrectPrefab(ItemType itemType, bool canBePlaced, bool up, bool left, bool down, bool right)
    {
        if (itemType == ItemType.Cable)
        {
            if (connectionNode.CurrentObject != null)
            {
                //print("Enters Destroy thing");
                connectionNode.DestroyNode();
            }
            connectionNode.InstantiateCableNode(up, left, down, right);
        }
        if (itemType == ItemType.Lightbulb)
        {
            if (connectionNode.CurrentObject != null)
            {
                //print("Enters Destroy thing");
                connectionNode.DestroyNode();
            }
            connectionNode.InstantiateLightbulbNode();
        }
        /*else if (itemType == ItemType.Battery)
        {
            if (canBePlaced)
            {
                connectionNode.InstantiateBatteryNode();
            }
        }*/

    }

    public void InstantiateBatteryPrefabAndConnectNodes(GridCell[] otherCells, int rotation)
    {
        for (int i = 0; i < otherCells.Length; i++)
        {
            otherCells[i].connectionNode = this.connectionNode;
        }
        connectionNode.InstantiateBatteryNode(rotation);
    }

    public void Invoke_OccupiedWhenPlaced()
    {
        //print("turned off");
        isOccupied = false;
        cellStatus = CellStatus.Unselected;
    }

}
