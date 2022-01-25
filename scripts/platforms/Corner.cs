using Godot;
using System;

public class Corner : Spatial
{
    private Globals Globals;
    private bool _reverse;

    public Direction[,] moves =
    {
        {Direction.RightDown, Direction.LeftUp},
        {Direction.LeftDown, Direction.RightUp},
        {Direction.LeftUp, Direction.RightDown},
        {Direction.RightUp, Direction.LeftDown},
    };

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        _reverse = Globals.GetRandomBool();
    }

    public void Rotate()
    {
        this.RotationDegrees = _GetRotation();
    }

    public void UpdatePossibleMoves()
    {
        Globals.possibleMoves = new Direction[1]
        {
            moves[(int)Globals.animationDirection, (_reverse ? 1 : 0)]
        };
    }

    private Vector3 _GetRotation()
    {
        Vector3 rotationVector = new();

        if (Globals.animationDirection == Direction.RightDown)
        {
            rotationVector.y = -90;
        }
        if (Globals.animationDirection == Direction.LeftDown)
        {
            rotationVector.y = 180;
        }
        if (Globals.animationDirection == Direction.LeftUp)
        {
            rotationVector.y = 90;
        }

        if (_reverse)
        {
            rotationVector.y -= 90;
        }

        return rotationVector;
    }

    private void _OnAnimationPlayerFinished(String animationName)
    {
        if (animationName == "Hide")
        {
            this.QueueFree();
        }
    }
}