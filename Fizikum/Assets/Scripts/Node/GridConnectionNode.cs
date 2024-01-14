using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GridConnectionNode : MonoBehaviour
{
    public GridCell[,] cells;

    public int GridSizeX;
    public int GridSizeY;
    public CameraMovement CAMERA;
    public GameObject GridCell;

    public bool inPlacementMode = false;
    public ItemType PlacementModeItemType;
    public int PlacementModeRotation = 0;

    [Header("Keybinds")]
    public KeyCode placeCableKey = KeyCode.Alpha1;
    public KeyCode placeBatteryKey = KeyCode.Alpha2;
    public KeyCode deleteKey = KeyCode.C;
    public KeyCode rotateItemKey = KeyCode.R;
    public KeyCode placeLightbulbKey = KeyCode.Alpha3;

    //public ConnectionNode[,] ConnectionNodeMatrix = new ConnectionNode[6, 6];
    void Start()
    {
        GenerateGrid();

        /*for (int i = 0; i < ConnectionNodeMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < ConnectionNodeMatrix.GetLength(1); j++)
            {
                if (ConnectionNodeMatrix[i, j] != null)
                {

                }
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach (var item in cells)
            {
                Destroy(item);
            }
            GenerateGrid();
        }
        if (Input.GetKeyDown(placeBatteryKey))
        {
            PlacementModeItemType = ItemType.Battery;
            inPlacementMode = true;
        }
        if (!inPlacementMode)
        {
            // PLACE CABLE
            if (Input.GetKeyDown(placeCableKey))
            {
                FindAllSelectedGridCells(ItemType.Cable, ActionType.Create);
            }
            if (Input.GetKeyDown(placeLightbulbKey))
            {
                FindAllSelectedGridCells(ItemType.Lightbulb, ActionType.Create);
            }
            // DELETE
            if (Input.GetKeyDown(deleteKey))
            {
                // Item type here does not matter beacuse the action type is delete.
                FindAllSelectedGridCells(ItemType.Cable, ActionType.Delete);
            }
        }
        else
        {
            if (Input.GetKeyDown(rotateItemKey))
            {
                PlacementModeRotation++;
                if (PlacementModeRotation >= 4)
                {
                    PlacementModeRotation = 0;
                }
            }
            // PLACE BATTERY


            TryCreateAction_GridPrefab();
        }
    }

    public void GenerateGrid()
    {
        CAMERA.camInitialPos = new Vector3((-GridSizeX + 1) * 2, 0, (-GridSizeY + 1) * 2) * -1;
        CAMERA.transform.position = new Vector3((-GridSizeX + 1) * 2, 0, (-GridSizeY + 1) * 2) * -1;

        //ConnectionNodeMatrix = new ConnectionNode[GridSizeX, GridSizeY];

        cells = new GridCell[GridSizeX, GridSizeY];

        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                int x = i * 4;
                int z = j * 4;
                Vector3 transform = new Vector3(x + this.transform.position.x, this.transform.position.y, this.transform.position.z + z);
                var obj = Instantiate(GridCell, transform, Quaternion.identity, this.transform).GetComponent<GridCell>();
                obj.GridIndex_X = i;
                obj.GridIndex_Y = j;
                cells[i, j] = obj;
                cells[i, j].InstantiateConnectionNodePrefab();

            }
        }
    }

    public void FindAllSelectedGridCells(ItemType itemType, ActionType actionType)
    {
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                if (cells[i, j].cellStatus == CellStatus.Selected)
                {
                    if (actionType == ActionType.Create)
                    {
                        CreateAction_GridPrefab(i, j, itemType);
                    }
                    else if (actionType == ActionType.Delete)
                    {
                        DeleteAction_GridPrefab(i, j);
                    }
                }
            }
        }
    }

    public void TryCreateAction_GridPrefab()
    {
        int length = GetCellLength(PlacementModeItemType);
        ClearSelections();
        var cell = FindHoveredCell();

        if (cell != null)
        {
            int xIndex = cell.GridIndex_X;
            int yIndex = cell.GridIndex_Y;

            GridCell[] cellArray = new GridCell[length];
            int count = 0;



            for (int i = 0; i < length; i++)
            {
                if (PlacementModeRotation == 0 && xIndex + i < GridSizeX)
                {
                    var c = cells[xIndex + i, yIndex];

                    cellArray[i] = c;
                    count++;

                    SpaceIsOccupied(c);
                }
                else if (PlacementModeRotation == 2 && xIndex - i >= 0)
                {
                    var c = cells[xIndex - i, yIndex];

                    cellArray[i] = c;
                    count++;

                    SpaceIsOccupied(c);
                }
                else if (PlacementModeRotation == 1 && yIndex + i < GridSizeY)
                {
                    var c = cells[xIndex, yIndex + i];

                    cellArray[i] = c;
                    count++;

                    SpaceIsOccupied(c);
                }
                else if (PlacementModeRotation == 3 && yIndex - i >= 0)
                {
                    var c = cells[xIndex, yIndex - i];

                    cellArray[i] = c;
                    count++;

                    SpaceIsOccupied(c);
                }
            }



            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (count == length && !cellArray.Any(x => x.isOccupied))
                {
                    print("can be placed");
                    Create_Battery_With_2Cables(cellArray);
                }
                else
                {
                    ClearSelections();
                    inPlacementMode = false;
                    print("can NOT be placed");
                }

                inPlacementMode = false;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                ClearSelections();
                inPlacementMode = false;
            }
        }
    }

    public void DeleteAction_GridPrefab(int xIndex, int yIndex)
    {
        var cell = cells[xIndex, yIndex];
        cell.connectionNode.ItemType = ItemType.None;
        if (cell.connectionNode.CurrentObject != null)
        {
            //print("deleting");
            cell.DeletePrefab();
        }
        ClearSelections();
        if (xIndex + 1 < GridSizeX && cells[xIndex + 1, yIndex].connectionNode.ItemType == ItemType.Cable)
        {
            print("g1");
            CreateAction_GridPrefab(xIndex + 1, yIndex, ItemType.Cable, false);
        }
        if (yIndex + 1 < GridSizeY && cells[xIndex, yIndex + 1].connectionNode.ItemType == ItemType.Cable)
        {
            print("g2");
            CreateAction_GridPrefab(xIndex, yIndex + 1, ItemType.Cable, false);
        }
        if (xIndex - 1 >= 0 && cells[xIndex - 1, yIndex].connectionNode.ItemType == ItemType.Cable)
        {
            print("g3");
            CreateAction_GridPrefab(xIndex - 1, yIndex, ItemType.Cable, false);
        }
        if (yIndex - 1 >= 0 && cells[xIndex, yIndex - 1].connectionNode.ItemType == ItemType.Cable)
        {
            print("g4");
            CreateAction_GridPrefab(xIndex, yIndex - 1, ItemType.Cable, false);
        }

        
    }
    public void CreateAction_GridPrefab(int xIndex, int yIndex, ItemType itemType, bool firstLoop = true)
    {
        if (cells[xIndex, yIndex].connectionNode.CurrentObject != null && firstLoop)
        {
            //print("space is occupied");
            cells[xIndex, yIndex].SpaceIsOccupied(true);
            return;
        }

        bool canBePlaced = true;

        bool up = false;
        bool left = false;
        bool down = false;
        bool right = false;

        print(itemType.ToString());
        int lengthN = 1;
        /*if (itemType == ItemType.Battery)
        {
            lengthN = 2;
        }*/
        // up

        if (xIndex > 0 &&
            (cells[xIndex - 1, yIndex].connectionNode.CurrentObject != null ||
            cells[xIndex - 1, yIndex].cellStatus == CellStatus.Selected))
        {
            //if (cells[xIndex - 1, yIndex].connectionNode.ItemType != ItemType.Battery)
            //{
            up = true;
            //}

            if (firstLoop && cells[xIndex - 1, yIndex].cellStatus != CellStatus.Selected && cells[xIndex - 1, yIndex].connectionNode.ItemType == ItemType.Cable)
            {
                CreateAction_GridPrefab(xIndex - 1, yIndex, ItemType.Cable, false);
            }

            /* if (firstLoop && cells[xIndex - 1, yIndex].cellStatus != CellStatus.Selected
                 && cells[xIndex - 1, yIndex].connectionNode.ItemType != ItemType.Battery)
             {
                 print("Entered UP");
                 SetGridPrefab(xIndex - 1, yIndex, ItemType.Cable, false);
             }*/
        }
        // left
        if (yIndex > 0 &&
            (cells[xIndex, yIndex - 1].connectionNode.CurrentObject != null ||
            cells[xIndex, yIndex - 1].cellStatus == CellStatus.Selected))
        {
            //if (cells[xIndex, yIndex - 1].connectionNode.ItemType != ItemType.Battery)
            //{
            left = true;
            //}
            if (firstLoop && cells[xIndex, yIndex - 1].cellStatus != CellStatus.Selected && cells[xIndex, yIndex - 1].connectionNode.ItemType == ItemType.Cable)
            {
                CreateAction_GridPrefab(xIndex, yIndex - 1, ItemType.Cable, false);
            }

            /*if (firstLoop && cells[xIndex, yIndex - 1].cellStatus != CellStatus.Selected
                && cells[xIndex, yIndex - 1].connectionNode.ItemType != ItemType.Battery)
            {
                SetGridPrefab(xIndex, yIndex - 1, ItemType.Cable, false);
                print("Entered LEFT");
            }*/
        }
        // down
        if (xIndex < cells.GetLength(0) - 1 &&
            (cells[xIndex + 1, yIndex].connectionNode.CurrentObject != null ||
            cells[xIndex + 1, yIndex].cellStatus == CellStatus.Selected))
        {
            //if (cells[xIndex + 1, yIndex].connectionNode.ItemType != ItemType.Battery)
            //{
            down = true;
            //}
            if (firstLoop && cells[xIndex + 1, yIndex].cellStatus != CellStatus.Selected && cells[xIndex + 1, yIndex].connectionNode.ItemType == ItemType.Cable)
            {
                CreateAction_GridPrefab(xIndex + 1, yIndex, ItemType.Cable, false);

            }
            /*if (firstLoop && cells[xIndex + 1, yIndex].cellStatus != CellStatus.Selected
                && cells[xIndex + 1, yIndex].connectionNode.ItemType != ItemType.Battery)
            {
                print("Entered DOWN");
                SetGridPrefab(xIndex + 1, yIndex, ItemType.Cable, false);
            }*/
        }
        // right
        if (yIndex < cells.GetLength(1) - lengthN &&
            (cells[xIndex, yIndex + lengthN].connectionNode.CurrentObject != null ||
            cells[xIndex, yIndex + lengthN].cellStatus == CellStatus.Selected))
        {
            /*if (itemType == ItemType.Battery)
            {

                List<GridCell> cellList = new List<GridCell>(); 
                for (int i = yIndex; i < yIndex + lengthN; i++)
                {
                    if (cells[xIndex, i].connectionNode.CurrentObject != null)
                    {
                        canBePlaced = false;
                        break;
                    }
                    cellList.Add(cells[xIndex, i]);
                }
                if (canBePlaced)
                {
                    foreach (var item in cellList)
                    {
                        cells[xIndex, yIndex].connectionNode = item.connectionNode;
                        //item.connectionNode.InstantiateBatteryNode();
                    }
                }
            }*/

            //if (cells[xIndex, yIndex + lengthN].connectionNode.ItemType != ItemType.Battery)
            //{
            right = true;
            //}
            if (firstLoop && cells[xIndex, yIndex + lengthN].cellStatus != CellStatus.Selected && cells[xIndex, yIndex + lengthN].connectionNode.ItemType == ItemType.Cable)
            {
                print("Entered RIGHT");
                CreateAction_GridPrefab(xIndex, yIndex + lengthN, ItemType.Cable, false);
            }
        }

        print($"row{xIndex} col{yIndex}" + Environment.NewLine + $"Up:{up}  Left:{left}  Down:{down}  Right:{right}");

        cells[xIndex, yIndex].InstantiateCorrectPrefab(itemType, canBePlaced, up, left, down, right);



    }

    public void Create_Battery_With_2Cables(GridCell[] cellArray)
    {
        for (int i = 0; i < cellArray.Length; i++)
        {
            cellArray[i].cellStatus = CellStatus.Selected;
        }
        int x0 = cellArray[0].GridIndex_X;
        int y0 = cellArray[0].GridIndex_Y;

        int x1 = cellArray[3].GridIndex_X;
        int y1 = cellArray[3].GridIndex_Y;

        CreateAction_GridPrefab(x0, y0, ItemType.Cable);
        CreateAction_GridPrefab(x1, y1, ItemType.Cable);

        GridCell[] cells = new GridCell[]
        {
            cellArray[2]
        };
        cellArray[1].InstantiateBatteryPrefabAndConnectNodes(cells, PlacementModeRotation);


        ClearSelections();
    }

    public GridCell FindHoveredCell()
    {
        foreach (var c in cells)
        {
            if (c.isHovered)
            {
                return c;
            }
        }
        return null;
    }

    public void ClearSelections()
    {
        foreach (var cell in cells)
        {
            cell.isFree = false;
            cell.isOccupied = false;
            if (cell.cellStatus != CellStatus.Hovered)
            {
                cell.cellStatus = CellStatus.Unselected;
            }
        }
    }

    public int GetCellLength(ItemType type)
    {
        switch (type)
        {
            case ItemType.Cable:
                return 1;
            case ItemType.Battery:
                return 4;
            case ItemType.Lightbulb:
                return 1;
            default:
                return 1;
        }
    }

    public void SpaceIsOccupied(GridCell c)
    {
        if (c.connectionNode.CurrentObject != null)
        {
            Console.WriteLine("space occupied");
            c.SpaceIsOccupied(false);
        }
        else
        {
            Console.WriteLine("space free");
            c.SpaceIsFree();
        }
    }
}
/*
  0  
3 x 1
  2
*/
