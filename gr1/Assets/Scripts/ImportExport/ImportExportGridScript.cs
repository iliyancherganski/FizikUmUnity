using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ImportExportGridScript : MonoBehaviour
{
    public string jsonExport;
    public GridConnectionNode grid;
    public void GridExport()
    {
        GridObject obj = new GridObject(grid.GridSizeX, grid.GridSizeY);

        foreach (var cell in grid.cells)
        {
            if (cell.connectionNode.ItemType == ItemType.None)
            {
                continue;
            }
            if (cell.connectionNode.ItemType == ItemType.Cable || cell.connectionNode.ItemType == ItemType.Lightbulb)
            {
                print("Added cable/lightbulb to export");
                obj.cellObjects.Add(new GridObject.CellObject(cell.GridIndex_X, cell.GridIndex_Y, cell.connectionNode.ItemType));
            }
            else
            {
                if (cell.connectionNode.ItemType == ItemType.Battery && !cell.connectionNode.isPositive)
                {
                    continue;
                }
                print("Added battery/switch to export");
                obj.cellObjects.Add(new GridObject.CellObject(cell.GridIndex_X, cell.GridIndex_Y, cell.connectionNode.ItemType, cell.connectionNode.Rotation));
            }
        }

        // Serialize the entire GridObject
        jsonExport = JsonUtility.ToJson(obj);
        print("Grid exported successfully.");

        GUIUtility.systemCopyBuffer = jsonExport;
        print("Json copied on clipboard.");
    }

    public string GridImport(string json)
    {
        GridObject obj = null;
        try
        {
             obj = JsonUtility.FromJson<GridObject>(json);
        }
        catch (System.Exception)
        {
            return $"Невалиден JSON!";
            throw;
        }

        grid.RegenerateGrid(obj.gridSize_x, obj.gridSize_y);
        grid.inDeleteMode = false;

        foreach (var lightbulb in obj.cellObjects.Where(x => x.type == ItemType.Lightbulb))
        {
            grid.cells[lightbulb.X, lightbulb.Y].cellStatus = CellStatus.Selected;
            grid.CreateAction_GridPrefab(lightbulb.X, lightbulb.Y, ItemType.Lightbulb);
            grid.ClearSelections();
        }
        foreach (var Switch in obj.cellObjects.Where(x => x.type == ItemType.Switch))
        {
            grid.cells[Switch.X, Switch.Y].cellStatus = CellStatus.Selected;
            grid.PlacementModeRotation = Switch.rotation;
            grid.CreateAction_GridPrefab(Switch.X, Switch.Y, ItemType.Switch);
            grid.ClearSelections();
        }
        foreach (var battery in obj.cellObjects.Where(x => x.type == ItemType.Battery))
        {
            grid.cells[battery.X, battery.Y].cellStatus = CellStatus.Selected;

            grid.PlacementModeItemType = ItemType.Battery;
            grid.PlacementModeRotation = battery.rotation;

            GridCell[] cells = new GridCell[2];
            cells[0] = grid.cells[battery.X, battery.Y];

            if (grid.PlacementModeRotation == 0)
            {
                cells[1] = grid.cells[battery.X + 1, battery.Y];
            }
            else if (grid.PlacementModeRotation == 2)
            {
                cells[1] = grid.cells[battery.X - 1, battery.Y];
            }
            else if (grid.PlacementModeRotation == 1)
            {
                cells[1] = grid.cells[battery.X, battery.Y + 1];
            }
            else if (grid.PlacementModeRotation == 3)
            {
                cells[1] = grid.cells[battery.X, battery.Y - 1];
            }

            grid.cells[battery.X, battery.Y].InstantiateBatteryPrefabAndConnectNodes(cells, grid.PlacementModeRotation);
            grid.ClearSelections();
            grid.cells[battery.X, battery.Y].connectionNode.isPositive = true;
            print($"isPositive: {grid.cells[battery.X, battery.Y].connectionNode.isPositive}");
        }
        foreach (var cable in obj.cellObjects.Where(x => x.type == ItemType.Cable))
        {
            grid.cells[cable.X, cable.Y].cellStatus = CellStatus.Selected;

            grid.CreateAction_GridPrefab(cable.X, cable.Y, ItemType.Cable);
            grid.ClearSelections();
        }
        grid.flow.GridCheck();
        grid.ClearSelections();
        return "Успешно зареждане на симулацията.";
    }
}
