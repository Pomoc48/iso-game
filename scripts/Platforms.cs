using Godot;
using System;

public class Platforms : Spatial
{
    private Globals Globals;

    private int _history = 20;
    private int _total = 1;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        _RotateStartingPlatform();
    }

    public void Generate()
    {
        _PlaceCorrectPlatformType();

        if ((_total += 1) >= _history)
        {
            _RemoveOld();
        }
    }

    private void _PlaceCorrectPlatformType()
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
        int chance = Globals.GetRandomNumber(100);

        if (chance < chances[0])
        {
            _Cross();
        }

        if (chance >= chances[0] && chance < chances[1])
        {
            _TwoWay();
        }

        if (chance >= chances[1] && chance < chances[2])
        {
            _Long();
        }

        if (chance >= chances[2])
        {
            _Corner();
        }
    }

    private void _RotateStartingPlatform()
    {
        float rotation = (int)Globals.GenerateStartingPlatformPos();
        float _degreeInRadians = 1.5707963268f;

        rotation *= -_degreeInRadians;
        this.GetChild<Spatial>(0).RotateY(rotation);
    }

    // public void RepaintExistingPlatforms(bool red)
    // {
    //     MeshInstance meshInstance;
    //     SpatialMaterial spatialMaterial;

    //     if (red)
    //     {
    //         spatialMaterial = Globals.materialRed;
    //     }
    //     else
    //     {
    //         spatialMaterial = Globals.materialBlue;
    //     }

    //     foreach (Node _i in _platformsSpace.GetChildren())
    //     {
    //         meshInstance = _i.GetChild(0).GetNode<MeshInstance>("Border");
    //         meshInstance.SetSurfaceMaterial(0, spatialMaterial);
    //     }
    // }

    private void _Long()
    {
        Spatial platformBlockInstance;
        platformBlockInstance = _Place("Long");

        Long Long = (Long)platformBlockInstance;
        Long.Rotate();
        Long.UpdatePossibleMoves();
    }

    private void _Corner()
    {
        Spatial platformBlockInstance;
        platformBlockInstance = _Place("Corner");

        Corner Corner = (Corner)platformBlockInstance;
        Corner.Rotate();
        Corner.UpdatePossibleMoves();
    }

    private void _Cross()
    {
        Spatial platformBlockInstance;
        platformBlockInstance = _Place("Cross");

        Cross Cross = (Cross)platformBlockInstance;
        Cross.Rotate();
        Cross.UpdatePossibleMoves();
    }

    private void _TwoWay()
    {
        Spatial platformBlockInstance;
        platformBlockInstance = _Place("TwoWay");

        Twoway Twoway = (Twoway)platformBlockInstance;
        Twoway.Rotate();
        Twoway.UpdatePossibleMoves();
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
        meshInstance.SetSurfaceMaterial(0, Globals.RotateHue());

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

        return rotationVector;
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