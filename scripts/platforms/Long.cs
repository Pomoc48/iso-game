using Godot;
using System;

public class Long : Spatial
{
    private Globals Globals;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");

        _Rotate();
        _UpdatePossibleMoves();
    }

    private void _Rotate()
    {
        if (((int)Globals.animationDirection % 2) != 0)
        {
            Vector3 rotation = new(0, 90, 0);
            this.RotationDegrees = rotation;
        }
    }

    private void _UpdatePossibleMoves()
    {
        Globals.possibleMoves = new Direction[1]
        {
            Globals.animationDirection
        };
    }
}
