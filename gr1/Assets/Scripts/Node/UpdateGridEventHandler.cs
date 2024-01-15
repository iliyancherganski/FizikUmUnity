/// <summary>
/// This class is used for handling events about updating the nodes on the grid.
/// 
/// There would be two main stages of updating the grid's nodes:
/// 1.  ATTACH - when the items on the grid are placed, the classes in this stage
///     will ensure that every prefab of the given node is the correct one and its 
///     rotation is as expexted. 
///     >>> This stage is mainly used for visually linking the grid's nodes.
///     
/// 2.  CONNECT - when there is an electrical source on the grid, this stage checks
///     wether the nodes are connected properly and the electricity can be run 
///     properly.
///     >>> This stage is used for logically linking the grid's nodes.
/// </summary>
public static class UpdateGridEventHandler
{
    public static void ATTACH_PlaceNewNodes()
    {

    }
}
