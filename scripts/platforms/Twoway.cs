using Godot;
using System;

public class Twoway : Spatial
{
    private Globals Globals;

    private int _randomSide;
    private int _randomSideIndex = 0;

    public Direction[,,] twowayMoves =
    {
        {
            {Direction.RightDown, Direction.LeftUp},
            {Direction.RightUp, Direction.RightDown},
            {Direction.RightUp, Direction.LeftUp}
        },
        {
            {Direction.RightUp, Direction.LeftDown},
            {Direction.LeftDown, Direction.RightDown},
            {Direction.RightUp, Direction.RightDown}
        },
        {
            {Direction.RightDown, Direction.LeftUp},
            {Direction.LeftUp, Direction.LeftDown},
            {Direction.RightDown, Direction.LeftDown}
        },
        {
            {Direction.LeftDown, Direction.RightUp},
            {Direction.RightUp, Direction.LeftUp},
            {Direction.LeftUp, Direction.LeftDown}
        },
    };

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");

        _randomSide = Globals.GetRandomNumber(30);

        if (_randomSide < 5) // 16% for -| shape
        {
            _randomSideIndex = 1;
        }

        if (_randomSide >= 5 && _randomSide < 10) // 16% for |- shape
        {
            _randomSideIndex = 2;
        }
    }

    public void Rotate()
    {
        this.RotationDegrees = _GetRotation();
    }

    public void UpdatePossibleMoves()
    {
        Globals.possibleMoves = new Direction[2];
        int d = (int)Globals.animationDirection;

        for (int i = 0; i < 2; i++)
        {
            Globals.possibleMoves[i] = twowayMoves[d, _randomSideIndex, i];
        }
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

        rotationVector.y += _GetOverrideRotation();

        return rotationVector;
    }

    private float _GetOverrideRotation()
    {
        return _randomSideIndex switch
        {
            2 => -90,
            1 => 90,
            _ => 0
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