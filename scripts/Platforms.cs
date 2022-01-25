using Godot;
using System;

public class Platforms : Spatial
{
    private Globals Globals;
    private Values Values;

    private int _history = 20;
    private int _total = 1;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Values = GetNode<Values>("/root/Values");
        
        _RotateStartingPlatform();
    }

    public void Generate()
    {
        PlaftormType platformType = Globals.GetPlatformType();

        switch (platformType)
        {
            case PlaftormType.Long:
                _Long();
                break;

            case PlaftormType.Corner:
                _Corner();
                break;

            case PlaftormType.Cross:
                _Cross();
                break;

            case PlaftormType.Twoway:
                _TwoWay();
                break;
        }

        if ((_total += 1) >= _history)
        {
            _RemoveOld();
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
        platformBlockInstance.RotationDegrees = _GetRotation();

        Globals.possibleMoves = new Direction[1]{Globals.animationDirection};
    }

    private void _Corner()
    {
        Spatial platformBlockInstance;

        int reverse = 0;
        // Change direction of the corner to the other side
        if (Globals.GetRandomBool())
        {
            reverse++;
            platformBlockInstance = _Place("CornerL");
        }
        else
        {
            platformBlockInstance = _Place("CornerR");
        }

        platformBlockInstance.RotationDegrees = _GetRotation();

        // Check direction change
        Globals.possibleMoves = new Direction[1]
        {
            Values.cornerMoves[(int)Globals.animationDirection, reverse]
        };
    }

    private void _Cross()
    {
        Spatial platformBlockInstance;
        platformBlockInstance = _Place("Cross");
        platformBlockInstance.RotationDegrees = _GetRotation();

        // Only opposite direction is removed from the array
        Globals.possibleMoves = new Direction[3];

        for (int i = 0; i < 3; i++)
        {
            Globals.possibleMoves[i] = Values.crossMoves[
                (int)Globals.animationDirection, i];
        }
    }

    private void _TwoWay()
    {
        Spatial platformBlockInstance;
        // Get new random orientation of the platform
        Random random = new();
        int randomSide = random.Next(30); // 66% for T shape

        int randomSideIndex = 0;
        String tPlatform = "TwoWay";

        float rotation = (int)Globals.animationDirection * -90;

        if (randomSide < 5) // 16% for -| shape
        {
            randomSideIndex = 1;
            tPlatform = "TwoWayR";
        }

        if (randomSide >= 5 && randomSide < 10) // 16% for |- shape
        {
            randomSideIndex = 2;
            tPlatform = "TwoWayL";
        }

        platformBlockInstance = _Place(tPlatform);

        platformBlockInstance.RotationDegrees = _GetRotation();

        // Get valid moves based on new rotation
        Globals.possibleMoves = new Direction[2];

        for (int i = 0; i < 2; i++)
        {
            Globals.possibleMoves[i] = Values.twowayMoves[
                (int)Globals.animationDirection, randomSideIndex, i];
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