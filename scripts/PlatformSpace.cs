using Godot;
using System;

public class PlatformSpace : Spatial
{
    private Globals Globals;

    private int _history = 4;
    private int _total = 1;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
    }

    public void Generate()
    {
        _PlaceCorrectType();

        if ((_total += 1) >= _history)
        {
            _RemoveOld();
        }
    }

    private void _PlaceCorrectType()
    {
        int[] difficultyChancesList = {};

        switch (Globals.platformDifficulty)
        {
            case PlafformDifficulty.Easy:
            {
                int[] easyChances = {10, 40, 75};
                difficultyChancesList = easyChances;
                break;
            }
            
            case PlafformDifficulty.Medium:
            {
                int[] mediumChances = {5, 35, 60};
                difficultyChancesList = mediumChances;
                break;
            }

            case PlafformDifficulty.Hard:
            {
                int[] hardChances = {2, 22, 40};
                difficultyChancesList = hardChances;
                break;
            }
        }

        _GetPlatformFromChances(difficultyChancesList);
    }

    private void _GetPlatformFromChances(int[] chances)
    {
        int chance = Globals.GetRandomNumber(100);

        if (chance < chances[0])
        {
            _Place("Cross");
        }

        if (chance >= chances[0] && chance < chances[1])
        {
            _Place("TwoWay");
        }

        if (chance >= chances[1] && chance < chances[2])
        {
            _Place("Long");
        }

        if (chance >= chances[2])
        {
            _Place("Corner");
        }
    }

    private void _Place(String type)
    {
        PackedScene platformBlock;
        Spatial blockInstance;

        String platformPath = "res://scenes/platforms/"+type+".tscn";

        platformBlock = (PackedScene)ResourceLoader.Load(platformPath);
        blockInstance = (Spatial)platformBlock.Instance();

        blockInstance.Translation = _GetPosition(blockInstance);

        this.AddChild(blockInstance);
    }

    private Vector3 _GetPosition(Spatial blockInstance)
    {
        blockInstance.Translation = Globals.GetFuturePosition();
        return blockInstance.Translation;
    }

    private void _RemoveOld()
    {
        int childIndex = _total - _history;
        Spatial child = this.GetChild<Spatial>(childIndex);

        _total--;
        child.GetNode("Spatial").Call("play_fade_out_animation");
    }
}
