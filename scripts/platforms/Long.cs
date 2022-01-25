using Godot;
using System;

public class Long : Spatial
{
    private Globals Globals;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
    }

    public void Rotate()
    {
        if (((int)Globals.animationDirection % 2) != 0)
        {
            Vector3 rotaion = new(0, 90, 0);

            this.RotationDegrees = rotaion;
        }
    }

    public void UpdatePossibleMoves()
    {
        Globals.possibleMoves = new Direction[1]
        {
            Globals.animationDirection
        };
    }

    private void _OnAnimationPlayerFinished(String animationName)
    {
        if (animationName == "Hide")
        {
            this.QueueFree();
        }
    }
}