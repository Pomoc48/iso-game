using Godot;
using System;

public class Start : Spatial
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
        int rotationY = (int)_GenerateStartingPos();
        rotationY *= - 90;

        Vector3 rotation = new(0, rotationY, 0);
        this.RotationDegrees = rotation;
    }

    private Direction _GenerateStartingPos()
    {
        Globals.startingDirection = (Direction)Globals.GetRandomNumber(4);
        return Globals.startingDirection;
    }

    private void _UpdatePossibleMoves()
    {
        Globals.possibleMoves = new Direction[1]
        {
            Globals.startingDirection
        };
    }
}
