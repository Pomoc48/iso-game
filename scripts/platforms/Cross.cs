using Godot;
using System;

public class Cross : Spatial
{
    private Globals Globals;

    public Direction[,] crossMoves =
    {
        {Direction.LeftUp, Direction.RightUp, Direction.RightDown},
        {Direction.RightDown, Direction.RightUp, Direction.LeftDown},
        {Direction.LeftUp, Direction.RightDown, Direction.LeftDown},
        {Direction.LeftUp, Direction.RightUp, Direction.LeftDown},
    };

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");

        _UpdatePossibleMoves();
    }

    private void _UpdatePossibleMoves()
    {
        Globals.possibleMoves = new Direction[3];
        int d = (int)Globals.animationDirection;

        for (int i = 0; i < 3; i++)
        {
            Globals.possibleMoves[i] = crossMoves[d, i];
        }
    }
}