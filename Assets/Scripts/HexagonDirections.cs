using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexagonDirections
{
    NE, E, SE, SW, W, NW
}

public static class HexagonDirectionsExtensions
{

    public static HexagonDirections Opposite(this HexagonDirections direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexagonDirections Previous(this HexagonDirections direction)
    {
        return direction == HexagonDirections.NE ? HexagonDirections.NW : (direction - 1);
    }

    public static HexagonDirections Next(this HexagonDirections direction)
    {
        return direction == HexagonDirections.NW ? HexagonDirections.NE : (direction + 1);
    }
}
