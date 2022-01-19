using Godot;
using System;

enum PlafformDifficulty
{
    Easy,
    Medium,
    Hard,
}

public enum PlaftormType
{
    Long,
    Corner,
    Cross,
    Twoway
}

public enum Direction
{
    RightUp,
    RightDown,
    LeftDown,
    LeftUp
}

public class Values : Node
{
    public Direction[,] cornerMoves =
    {
        {Direction.RightDown, Direction.LeftUp},
        {Direction.LeftDown, Direction.RightUp},
        {Direction.LeftUp, Direction.RightDown},
        {Direction.RightUp, Direction.LeftDown},
    };

    public Direction[,] crossMoves =
    {
        {Direction.LeftUp, Direction.RightUp, Direction.RightDown},
        {Direction.RightDown, Direction.RightUp, Direction.LeftDown},
        {Direction.LeftUp, Direction.RightDown, Direction.LeftDown},
        {Direction.LeftUp, Direction.RightUp, Direction.LeftDown},
    };

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
}