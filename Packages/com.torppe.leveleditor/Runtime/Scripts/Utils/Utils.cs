using UnityEngine;
using UnityEngine.InputSystem;

public static class Utils
{
    public static Vector3 GetMouseEdgeScrollDirection()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        var width = Screen.width;
        var height = Screen.height;
        var edgeSize = 10;

        var direction = Vector3.zero;

        if (mousePosition.x < edgeSize)
        {
            direction += Vector3.left;
        }
        if (mousePosition.x > width - edgeSize)
        {
            direction += Vector3.right;
        }
        if (mousePosition.y > height - edgeSize)
        {
            direction += Vector3.up;
        }
        if (mousePosition.y < edgeSize)
        {
            direction += Vector3.down;
        }

        return direction;
    }
}