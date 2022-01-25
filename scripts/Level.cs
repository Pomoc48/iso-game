using Godot;
using System;

public class Level : Spatial
{
    private Globals Globals;
    private Values Values;

    private Spatial _platformsSpace;

    private int _platformHistory = 20;
    private int _totalPlatforms = 1;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Values = GetNode<Values>("/root/Values");

        _platformsSpace = GetNode<Spatial>("Platforms");
        
        _RotateStartingPlatform();
    }

    public void GeneratePlatform()
    {
        PlaftormType platformType = Globals.GetPlatformType();

        switch (platformType)
        {
            case PlaftormType.Long:
                _PlatformLong();
                break;

            case PlaftormType.Corner:
                _PlatformCorner();
                break;

            case PlaftormType.Cross:
                _PlatformCross();
                break;

            case PlaftormType.Twoway:
                _PlatformTwoWay();
                break;
        }

        _totalPlatforms++;

        if (_totalPlatforms >= _platformHistory)
        {
            _RemoveOldPlatforms();
        }
    }

    private void _RotateStartingPlatform()
    {
        float rotation = (int)Globals.GenerateStartingPlatformPos();
        float _degreeInRadians = 1.5707963268f;

        rotation *= -_degreeInRadians;
        _platformsSpace.GetChild<Spatial>(0).RotateY(rotation);
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

    private void _PlatformLong()
    {
        Spatial platformBlockInstance;
        platformBlockInstance = _PlacePlatform("Long");
        platformBlockInstance.RotationDegrees = _GetPlatformRotation();

        Globals.possibleMoves = new Direction[1]{Globals.animationDirection};
    }

    private void _PlatformCorner()
    {
        Spatial platformBlockInstance;

        int reverse = 0;
        // Change direction of the corner to the other side
        if (Globals.GetRandomBool())
        {
            reverse++;
            platformBlockInstance = _PlacePlatform("CornerL");
        }
        else
        {
            platformBlockInstance = _PlacePlatform("CornerR");
        }

        platformBlockInstance.RotationDegrees = _GetPlatformRotation();

        // Check direction change
        Globals.possibleMoves = new Direction[1]
        {
            Values.cornerMoves[(int)Globals.animationDirection, reverse]
        };
    }

    private void _PlatformCross()
    {
        Spatial platformBlockInstance;
        platformBlockInstance = _PlacePlatform("Cross");
        platformBlockInstance.RotationDegrees = _GetPlatformRotation();

        // Only opposite direction is removed from the array
        Globals.possibleMoves = new Direction[3];

        for (int i = 0; i < 3; i++)
        {
            Globals.possibleMoves[i] = Values.crossMoves[
                (int)Globals.animationDirection, i];
        }
    }

    private void _PlatformTwoWay()
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

        platformBlockInstance = _PlacePlatform(tPlatform);

        platformBlockInstance.RotationDegrees = _GetPlatformRotation();

        // Get valid moves based on new rotation
        Globals.possibleMoves = new Direction[2];

        for (int i = 0; i < 2; i++)
        {
            Globals.possibleMoves[i] = Values.twowayMoves[
                (int)Globals.animationDirection, randomSideIndex, i];
        }
    }

    private Spatial _PlacePlatform(String type)
    {
        PackedScene platformBlock;
        Spatial blockInstance;

        String platformPath = "res://scenes/platforms/"+type+".tscn";

        platformBlock = (PackedScene)ResourceLoader.Load(platformPath);
        blockInstance = (Spatial)platformBlock.Instance();

        blockInstance = _RecolorPlatform(blockInstance);
        blockInstance.Translation = _GetPlatformPosition(blockInstance);

        _platformsSpace.AddChild(blockInstance);
        return blockInstance;
    }

    private Spatial _RecolorPlatform(Spatial instance)
    {
        MeshInstance meshInstance;

        meshInstance = instance.GetChild(0).GetNode<MeshInstance>("Border");
        meshInstance.SetSurfaceMaterial(0, Globals.RotateHue());

        return instance;
    }

    private Vector3 _GetPlatformPosition(Spatial blockInstance)
    {
        blockInstance.Translation = Globals.GetFuturePosition();
        Vector3 translationVector = blockInstance.Translation;

        float height = Globals.platformHeight - 16;
        translationVector.y = height;

        return translationVector;
    }

    private Vector3 _GetPlatformRotation()
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

    private void _RemoveOldPlatforms()
    {
        int childIndex = _totalPlatforms - _platformHistory;
        Spatial child = _platformsSpace.GetChild<Spatial>(childIndex);

        _totalPlatforms--;
        // child.QueueFree();
        child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Hide");
    }
}