using Godot;
using System;

public class Level : Spatial
{
    private Globals Globals;
    private Player Player;

    private String[] _inputMap = {
        "ui_up",
        "ui_right",
        "ui_down", 
        "ui_left",
    };

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Player = GetNode<Player>("/root/Level/Player");
    }

    public override void _Process(float delta)
    {
        // if (_canPlayerMove && Input.IsActionPressed(_inputMap[0])
        //     && Input.IsActionPressed(_inputMap[1]))
        // {
        //     CheckMove(Direction.RightUp);
        // }

        // if (_canPlayerMove && Input.IsActionPressed(_inputMap[1])
        //     && Input.IsActionPressed(_inputMap[2]))
        // {
        //     CheckMove(Direction.RightDown);
        // }

        // if (_canPlayerMove && Input.IsActionPressed(_inputMap[2])
        //     && Input.IsActionPressed(_inputMap[3]))
        // {
        //     CheckMove(Direction.LeftDown);
        // }

        // if (_canPlayerMove && Input.IsActionPressed(_inputMap[3])
        //     && Input.IsActionPressed(_inputMap[0]))
        // {
        //     CheckMove(Direction.LeftUp);
        // }
    }
}
