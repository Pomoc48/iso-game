using Godot;
using System;

public class Level : Spatial
{
    private Globals Globals;
    private Values Values;

    private Spatial _platformsSpace;
    private Spatial _decorationsSpace;

    private WorldEnvironment _worldEnviroment;

    private int _platformHistory = 24;
    private int _totalDecorations = 0;
    private int _totalPlatforms = 1;

    private float _degreeInRadians = 1.5707963268f;
    private float _fogHeight = 24f;

    // Init function
    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Values = GetNode<Values>("/root/Values");

        _platformsSpace = GetNode<Spatial>("Platforms");
        _decorationsSpace = GetNode<Spatial>("Decorations");
        _worldEnviroment = GetNode<WorldEnvironment>("WorldEnvironment");
        
        // Rotate starting platform
        float rotation = (int)Globals.GenerateStartingPlatformPos();

        rotation *= -_degreeInRadians;
        _platformsSpace.GetChild<Spatial>(0).RotateY(rotation);

        _SetEnviromentFogHeight(0);
    }

    // Create floating cubes decorations
    public void CreateDecoration()
    {
        Vector3 blockPosition = new Vector3();

        if (Globals.firstMove) // Idle animation position fix
        {
            blockPosition = Globals.playerPosition;
        }
        else // Future move pos
        {
            blockPosition = Globals.DirectionCalc();
        }

        // Random offset
        blockPosition = Globals.CalculateDecorationPosition(blockPosition);
        
        blockPosition.y += Globals.platformHeight;
        _LoadDecoration(blockPosition);
    }

    private void _LoadDecoration(Vector3 blockPosition)
    {
        String blockPath;

        // if (Globals.perspectiveMode)
        // {
        //     blockPath = "res://scenes/BlockM.tscn";
        // }
        // else
        // {
        //     blockPath = "res://scenes/Block.tscn";
        // }

        blockPath = "res://scenes/Block.tscn";

        PackedScene block = (PackedScene)ResourceLoader.Load(blockPath);
                
        Spatial blockInstance = (Spatial)block.Instance();
        blockInstance.Translation = blockPosition;


        SpatialMaterial newHue = new SpatialMaterial();

        newHue.EmissionEnabled = true;
        newHue.Emission = Globals.emissionColor;


        MeshInstance meshInstance = blockInstance.GetNode<MeshInstance>("MeshInstance");
        CPUParticles cpuParticles = blockInstance.GetNode<CPUParticles>("CPUParticles");

        cpuParticles.Mesh.SurfaceSetMaterial(0, newHue);

        meshInstance.SetSurfaceMaterial(0, newHue);
        
        _decorationsSpace.AddChild(blockInstance);
        _totalDecorations++;
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
                _platformCorner();
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

    private void _platformCorner()
    {
        Spatial platformBlockInstance;

        int reverse = 0;
        // Change direction of the corner to the other side
        if (Globals.RandomBool())
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
        Random random = new Random();
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
        Spatial platformBlockInstance;
        MeshInstance meshInstance;

        String platformPath = "res://scenes/platformsH/"+type+".tscn";

        platformBlock = (PackedScene)ResourceLoader.Load(platformPath);
        platformBlockInstance = (Spatial)platformBlock.Instance();

        // if (Globals.perspectiveMode)
        // {
        //     meshInstance = platformBlockInstance.GetChild(0).GetNode<MeshInstance>("Border");
        //     meshInstance.SetSurfaceMaterial(0, Globals.materialRed);
        // }

        meshInstance = platformBlockInstance.GetChild(0).GetNode<MeshInstance>("Border");
        meshInstance.SetSurfaceMaterial(0, Globals.RotateHue());

        // Get platform future pos
        platformBlockInstance.Translation = Globals.DirectionCalc();

        // Starting animation fix
        Vector3 translationVector = platformBlockInstance.Translation;
        translationVector.y = -16;

        // Adjust platform height
        translationVector.y += Globals.platformHeight;
        // Adjust world fog height
        _SetEnviromentFogHeight(Globals.platformHeight);

        platformBlockInstance.Translation = translationVector;

        _platformsSpace.AddChild(platformBlockInstance);
        return platformBlockInstance;
    }

    private Vector3 _GetPlatformRotation()
    {
        Vector3 rotationVector = new Vector3();

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

    private void _SetEnviromentFogHeight(float level)
    {
        _worldEnviroment.Environment.FogHeightMin = level;
        _worldEnviroment.Environment.FogHeightMax = -_fogHeight + level;
    }

    private void _RemoveOldPlatforms()
    {
        int childIndex = _totalPlatforms - _platformHistory;
        Spatial child = _platformsSpace.GetChild<Spatial>(childIndex);

        _totalPlatforms--;
        // TBD
        child.QueueFree();
        //child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Hide");
    }
}