using Godot;
using System;

public class Platforms : Spatial
{
    private Globals Globals;

    private int _history = 20;
    private int _total = 1;

    private float _valueSubtract = 0.05f;

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

        _AdjustExistingPlatformsValue();
    }

    private void _AdjustExistingPlatformsValue()
    {
        MeshInstance meshInstance;
        

        for (int i = _total; i > 0; i--)
        {
            meshInstance = this.GetChild(i-1).GetNode<MeshInstance>("Spatial/Border");

            SpatialMaterial materialOld = (SpatialMaterial)meshInstance.GetSurfaceMaterial(0);
            Color emission = materialOld.Emission;

            emission.v -= _valueSubtract;

            SpatialMaterial material = new();
            material.EmissionEnabled = true;
            material.Emission = emission;
            
            meshInstance.SetSurfaceMaterial(0, material);
        }
    }

    private void _PlaceCorrectType()
    {
        int[] difficultyChancesList;

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

            default: // PlafformDifficulty.Hard
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
        Spatial platformBlockInstance;

        int chance = Globals.GetRandomNumber(100);

        if (chance < chances[0])
        {
            platformBlockInstance = _Place("Cross");
        }

        if (chance >= chances[0] && chance < chances[1])
        {
            platformBlockInstance = _Place("TwoWay");
        }

        if (chance >= chances[1] && chance < chances[2])
        {
            platformBlockInstance = _Place("Long");
        }

        if (chance >= chances[2])
        {
            platformBlockInstance = _Place("Corner");
        }
    }

    private Spatial _Place(String type)
    {
        PackedScene platformBlock;
        Spatial blockInstance;

        String platformPath = "res://scenes/platforms/"+type+".tscn";

        platformBlock = (PackedScene)ResourceLoader.Load(platformPath);
        blockInstance = (Spatial)platformBlock.Instance();

        blockInstance = _Recolor(blockInstance);
        blockInstance.Translation = _GetPosition(blockInstance);

        this.AddChild(blockInstance);
        return blockInstance;
    }

    private Spatial _Recolor(Spatial instance)
    {
        MeshInstance meshInstance;

        meshInstance = instance.GetChild(0).GetNode<MeshInstance>("Border");
        meshInstance.SetSurfaceMaterial(0, Globals.GetEmissionMaterial());

        return instance;
    }

    private Vector3 _GetPosition(Spatial blockInstance)
    {
        blockInstance.Translation = Globals.GetFuturePosition();
        Vector3 translationVector = blockInstance.Translation;

        float height = Globals.platformHeight - 16;
        translationVector.y = height;

        return translationVector;
    }

    private void _RemoveOld()
    {
        int childIndex = _total - _history;
        Spatial child = this.GetChild<Spatial>(childIndex);

        _total--;
        // child.QueueFree();
        child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Hide");
    }
}