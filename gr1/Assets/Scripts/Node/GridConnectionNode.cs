using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class GridConnectionNode : MonoBehaviour
{
    public GridElectricityFlow flow;
    public GridCell[,] cells;
    public ImportExportGridScript importExport;

    public int GridSizeX;
    public int GridSizeY;
    public CameraMovement CAMERA;
    public GameObject TableGO;
    public GameObject GridCell;

    public bool inPlacementMode = false;
    public bool inDeleteMode = false;
    public ItemType PlacementModeItemType;
    public int PlacementModeRotation = 0;

    [Header("Keybinds")]
    public KeyCode placeCableKey = KeyCode.Alpha1;
    public KeyCode placeBatteryKey = KeyCode.Alpha2;
    public KeyCode placeLightbulbKey = KeyCode.Alpha3;
    public KeyCode placeSwitchKey = KeyCode.Alpha4;
    public KeyCode deleteKey = KeyCode.C;
    public KeyCode rotateItemKey = KeyCode.R;
    public KeyCode interactKey = KeyCode.E;

    [Header("Battery components")]
    public List<GridCell> cellsWithBattery = new List<GridCell>();
    public List<GridCell> cellsWithLightbulbs = new List<GridCell>();
    public List<GridCell> cellsWithSwitches = new List<GridCell>();
    public List<GridCell> batteryFirstCables = new List<GridCell>();
    public List<GridCell> batteryLastCables = new List<GridCell>();


    [Header("FOR EDITOR")]
    public List<GridCell> allSelected = new List<GridCell>();

    void Start()
    {
        if (GridSizeX >= 25)
        {
            GridSizeX = 25;
        }
        if (GridSizeY >= 25)
        {
            GridSizeY = 25;
        }

        GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
        {
            print("Exporting grid...");
            importExport.GridExport();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
        {

        }
        EDITOR_FindAllSelected();
        KeybindInput();

    }

    public void KeybindInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            RegenerateGrid(GridSizeX, GridSizeY);
        }
        if (Input.GetKeyDown(placeBatteryKey))
        {
            PlacementModeItemType = ItemType.Battery;
            inPlacementMode = true;
        }
        else if (Input.GetKeyDown(placeSwitchKey))
        {
            PlacementModeItemType = ItemType.Switch;
            inPlacementMode = true;
        }
        if (!inPlacementMode)
        {
            // PLACE CABLE
            if (Input.GetKeyDown(placeCableKey) && !Input.GetKey(KeyCode.LeftControl))
            {
                FindAllSelectedGridCells(ItemType.Cable, ActionType.Create);
                flow.GridCheck();
            }
            if (Input.GetKeyDown(placeLightbulbKey) && !Input.GetKey(KeyCode.LeftControl))
            {
                FindAllSelectedGridCells(ItemType.Lightbulb, ActionType.Create);
                flow.GridCheck();
            }
            if (Input.GetKeyDown(interactKey) && !Input.GetKey(KeyCode.LeftControl))
            {
                FindAllSelectedGridCells(ItemType.Lightbulb, ActionType.Interact);
                flow.GridCheck();
            }

            // DELETE
            if (Input.GetKeyDown(deleteKey) && !Input.GetKey(KeyCode.LeftControl))
            {
                // Item type here does not matter beacuse the action type is delete.
                FindAllSelectedGridCells(ItemType.Cable, ActionType.Delete);
                flow.GridCheck();
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

    public void RegenerateGrid(int gridSizeX, int gridSizeY)
    {
        foreach (var item in cells)
        {
            Destroy(item.connectionNode.CurrentObject);
            Destroy(item.connectionNode);
            Destroy(item.gameObject);
        }
        GridSizeX = gridSizeX;
        GridSizeY = gridSizeY;

        GenerateGrid();
    }

    public void GenerateGrid()
    {
        CAMERA.camInitialPos = new Vector3((-GridSizeX + 1) * 2, 0, (-GridSizeY + 1) * 2) * -1;
        CAMERA.transform.position = new Vector3((-GridSizeX + 1) * 2, 0, (-GridSizeY + 1) * 2) * -1;
        TableGO.transform.position = new Vector3((-GridSizeX + 1) * 2, 0, (-GridSizeY + 1) * 2) * -1;

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
                obj.name = $"X[{i}] Y[{j}]";
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
                    else if (actionType == ActionType.Interact)
                    {
                        InteractWithSelectedGridCells();
                    }
                }
            }
        }
    }
    public void EDITOR_FindAllSelected()
    {
        allSelected = new List<GridCell>();
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                if (cells[i, j].cellStatus == CellStatus.Selected)
                {
                    allSelected.Add(cells[i, j]);
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
                    //print("can be placed");
                    switch (PlacementModeItemType)
                    {
                        case ItemType.Battery:
                            Create_Battery_With_2Cables(cellArray);
                            break;
                        case ItemType.Switch:
                            cells[xIndex, yIndex].cellStatus = CellStatus.Selected;
                            CreateAction_GridPrefab(xIndex, yIndex, ItemType.Switch, true);
                            ClearSelections();
                            break;
                        default:
                            break;
                    }
                    //flow.GridCheck();
                }
                else
                {
                    ClearSelections();
                    inPlacementMode = false;
                    //print("can NOT be placed");
                }

                inPlacementMode = false;
            }
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                ClearSelections();
                inPlacementMode = false;
            }
        }
    }

    public void DeleteAction_GridPrefab(int xIndex, int yIndex)
    {
        var cell = cells[xIndex, yIndex];
        if (cell.connectionNode.ItemType == ItemType.None)
            return;
        if (cell.connectionNode.ItemType != ItemType.None ||
            (cell.connectionNode.ItemType == ItemType.Battery && cell.connectionNode.batteryNode != null))
        {
            //print("deleting");
            cell.DeletePrefab();
            inDeleteMode = true;
        }
        //ClearSelections();
        if (xIndex + 1 < GridSizeX)
        {
            var currentCell = cells[xIndex + 1, yIndex];
            var currentCellType = currentCell.connectionNode.ItemType;
            //print($"cell type: {currentCellType}");
            if (currentCellType == ItemType.Cable
                || currentCellType == ItemType.Lightbulb
                || currentCellType == ItemType.Switch)
            {
                CreateAction_GridPrefab(currentCell.GridIndex_X, currentCell.GridIndex_Y, currentCellType, false);
            }
        }
        if (yIndex + 1 < GridSizeY)
        {
            var currentCell = cells[xIndex, yIndex + 1];
            var currentCellType = currentCell.connectionNode.ItemType;
            //print($"cell type: {currentCellType}");
            if (currentCellType == ItemType.Cable
                || currentCellType == ItemType.Lightbulb
                || currentCellType == ItemType.Switch)
            {
                CreateAction_GridPrefab(currentCell.GridIndex_X, currentCell.GridIndex_Y, currentCellType, false);
            }
        }
        if (xIndex - 1 >= 0)
        {
            var currentCell = cells[xIndex - 1, yIndex];
            var currentCellType = currentCell.connectionNode.ItemType;
            //print($"cell type: {currentCellType}");
            if (currentCellType == ItemType.Cable
                || currentCellType == ItemType.Lightbulb
                || currentCellType == ItemType.Switch)
            {
                CreateAction_GridPrefab(currentCell.GridIndex_X, currentCell.GridIndex_Y, currentCellType, false);
            }
        }
        if (yIndex - 1 >= 0)
        {
            var currentCell = cells[xIndex, yIndex - 1];
            var currentCellType = currentCell.connectionNode.ItemType;
            //print($"cell type: {currentCellType}");
            if (currentCellType == ItemType.Cable
                || currentCellType == ItemType.Lightbulb
                || currentCellType == ItemType.Switch)
            {
                CreateAction_GridPrefab(currentCell.GridIndex_X, currentCell.GridIndex_Y, currentCellType, false);
            }
        }
        inDeleteMode = false;
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

        //print(itemType.ToString());
        //bool wasSelected = false;
        /* if (cells[xIndex, yIndex].cellStatus == CellStatus.Selected)
         {
             //wasSelected = true;
             cells[xIndex, yIndex].cellStatus = CellStatus.Unselected;
         }*/

        // UP
        if (xIndex > 0)
        {
            var cell = cells[xIndex - 1, yIndex];

            if (cell.connectionNode.CurrentObject != null
                || (cell.cellStatus == CellStatus.Selected && !inDeleteMode))
            {
                /*if (cell.connectionNode.ItemType != ItemType.Battery
                    || cells[xIndex, yIndex] == FirstCableAfterBattery(cell)
                    || cells[xIndex, yIndex] == LastCableAfterBattery(cell))
                {
                    print("entered UP");
                    up = true;

                    if (firstLoop && cell.cellStatus != CellStatus.Selected
                        && cell.connectionNode.ItemType == ItemType.Cable)
                    {
                        CreateAction_GridPrefab(xIndex - 1, yIndex, ItemType.Cable, false);
                    }
                }*/
                up = true;

                var itemTypeTemp = cell.connectionNode.ItemType;
                if (firstLoop && cell.cellStatus != CellStatus.Selected
                    && (itemTypeTemp == ItemType.Cable
                    || itemTypeTemp == ItemType.Lightbulb
                    || itemTypeTemp == ItemType.Switch))
                {
                    CreateAction_GridPrefab(xIndex - 1, yIndex, itemTypeTemp, false);
                }
            }
        }

        // LEFT
        if (yIndex > 0)
        {
            var cell = cells[xIndex, yIndex - 1];
            if (cell.connectionNode.CurrentObject != null
                || (cell.cellStatus == CellStatus.Selected && !inDeleteMode))
            {
                /*if (cell.connectionNode.ItemType != ItemType.Battery
                    || cells[xIndex, yIndex] == FirstCableAfterBattery(cell)
                    || cells[xIndex, yIndex] == LastCableAfterBattery(cell))
                {
                    print("entered LEFT");
                    left = true;
                    if (firstLoop && cell.cellStatus != CellStatus.Selected
                        && cell.connectionNode.ItemType == ItemType.Cable)
                    {
                        CreateAction_GridPrefab(xIndex, yIndex - 1, ItemType.Cable, false);
                    }
                }*/
                left = true;
                var itemTypeTemp = cell.connectionNode.ItemType;
                if (firstLoop && cell.cellStatus != CellStatus.Selected
                    && (itemTypeTemp == ItemType.Cable
                    || itemTypeTemp == ItemType.Lightbulb
                    || itemTypeTemp == ItemType.Switch))
                {
                    CreateAction_GridPrefab(xIndex, yIndex - 1, itemTypeTemp, false);
                }
            }
        }

        // DOWN
        if (xIndex < cells.GetLength(0) - 1)
        {
            var cell = cells[xIndex + 1, yIndex];
            if (cell.connectionNode.CurrentObject != null
                || (cell.cellStatus == CellStatus.Selected && !inDeleteMode))
            {
                /*if (cell.connectionNode.ItemType != ItemType.Battery
                    || cells[xIndex, yIndex] == FirstCableAfterBattery(cell)
                    || cells[xIndex, yIndex] == LastCableAfterBattery(cell))
                {
                    print("entered DOWN");
                    down = true;
                    if (firstLoop && cell.cellStatus != CellStatus.Selected
                        && cell.connectionNode.ItemType == ItemType.Cable)
                    {
                        CreateAction_GridPrefab(xIndex + 1, yIndex, ItemType.Cable, false);

                    }
                }*/
                down = true;
                var itemTypeTemp = cell.connectionNode.ItemType;
                if (firstLoop && cell.cellStatus != CellStatus.Selected
                    && (itemTypeTemp == ItemType.Cable
                    || itemTypeTemp == ItemType.Lightbulb
                    || itemTypeTemp == ItemType.Switch))
                {
                    CreateAction_GridPrefab(xIndex + 1, yIndex, itemTypeTemp, false);

                }
            }
        }

        // RIGHT
        if (yIndex < cells.GetLength(1) - 1)
        {
            var cell = cells[xIndex, yIndex + 1];
            if (cell.connectionNode.CurrentObject != null
                || (cell.cellStatus == CellStatus.Selected && !inDeleteMode))
            {
                /*if (cell.connectionNode.ItemType != ItemType.Battery
                    || cells[xIndex, yIndex] == FirstCableAfterBattery(cell)
                    || cells[xIndex, yIndex] == LastCableAfterBattery(cell))
                {
                    print("entered RIGHT");
                    right = true;
                    if (firstLoop && cell.cellStatus != CellStatus.Selected
                        && cell.connectionNode.ItemType == ItemType.Cable)
                    {
                        //print("Entered RIGHT");
                        CreateAction_GridPrefab(xIndex, yIndex + 1, ItemType.Cable, false);
                    }
                }*/
                right = true;
                var itemTypeTemp = cell.connectionNode.ItemType;
                if (firstLoop && cell.cellStatus != CellStatus.Selected
                    && (itemTypeTemp == ItemType.Cable
                    || itemTypeTemp == ItemType.Lightbulb
                    || itemTypeTemp == ItemType.Switch))
                {
                    //print("Entered RIGHT");
                    CreateAction_GridPrefab(xIndex, yIndex + 1, itemTypeTemp, false);
                }
            }
        }

        //print($"row{xIndex} col{yIndex}" + Environment.NewLine + $"Up:{up}  Left:{left}  Down:{down}  Right:{right}");
        var rotation = 0;
        var currCell = cells[xIndex, yIndex].connectionNode;
        if (currCell.ItemType == ItemType.Switch)
        {
            rotation = currCell.Rotation;
            //print("SWITCH");
        }
        else rotation = PlacementModeRotation;

        cells[xIndex, yIndex].InstantiateCorrectPrefab(itemType, canBePlaced, up, left, down, right, rotation);
        /*if (wasSelected)
        {
            cells[xIndex, yIndex].cellStatus = CellStatus.Selected;
        }*/
       /* if (firstLoop)
        {
            flow.GridCheck();
        }*/

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
            cellArray[1],
            cellArray[2]
        };
        cellArray[1].InstantiateBatteryPrefabAndConnectNodes(cells, PlacementModeRotation);
        flow.GridCheck();

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

    public GridCell FirstCableAfterBattery(GridCell battery)
    {
        //    1st   2nd
        //r0: x-1   x+2
        //r1: y-1   y+2
        //r2: x+1   x-2
        //r3: y+1   y-2
        if (battery.connectionNode.ItemType == ItemType.Battery)
        {
            GridCell cell = null;
            switch (battery.connectionNode.Rotation)
            {
                case 0:
                    cell = cells[battery.GridIndex_X - 1, battery.GridIndex_Y];
                    break;
                case 1:
                    cell = cells[battery.GridIndex_X, battery.GridIndex_Y - 1];
                    break;
                case 2:
                    cell = cells[battery.GridIndex_X + 1, battery.GridIndex_Y];
                    break;
                case 3:
                    cell = cells[battery.GridIndex_X, battery.GridIndex_Y + 1];
                    break;
            }
            if (cell != null && cell.connectionNode.ItemType == ItemType.Cable)
            {
                return cell;
            }
        }
        return null;
    }
    public GridCell LastCableAfterBattery(GridCell battery)
    {
        //    1st   2nd
        //r0: x-1   x+2
        //r1: y-1   y+2
        //r2: x+1   x-2
        //r3: y+1   y-2
        if (battery.connectionNode.ItemType == ItemType.Battery)
        {
            GridCell cell = null;
            switch (battery.connectionNode.Rotation)
            {
                case 0:
                    cell = cells[battery.GridIndex_X + 2, battery.GridIndex_Y];
                    break;
                case 1:
                    cell = cells[battery.GridIndex_X, battery.GridIndex_Y + 2];
                    break;
                case 2:
                    cell = cells[battery.GridIndex_X - 2, battery.GridIndex_Y];
                    break;
                case 3:
                    cell = cells[battery.GridIndex_X, battery.GridIndex_Y - 2];
                    break;
            }
            if (cell != null && cell.connectionNode.ItemType == ItemType.Cable)
            {
                return cell;
            }
        }
        return null;
    }

    public void InteractWithSelectedGridCells()
    {
        foreach (var cell in allSelected
            .Where(x => x.connectionNode.CurrentObject != null
            && x.connectionNode.ItemType == ItemType.Switch))
        {
            SwitchScript switchScr = cell.connectionNode.CurrentObject.GetComponent<SwitchScript>();
            if (switchScr != null)
            {
                if (!switchScr.isTurnedOn)
                {
                    switchScr.ButtonIsOn(true);
                }
                else
                {
                    switchScr.ButtonIsOn(false);
                }
            }
        }
    }
}
/*
  0  
3 x 1
  2
*/
