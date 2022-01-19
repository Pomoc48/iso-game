using Godot;
using System;

public class Level : Spatial
{
    private Globals G;
    private Values V;

    private Spatial platformsSpace;
    private Spatial decorationsSpace;

    private int history = 4;
    private int totalDeco = 0;
    private int totalPlatforms = 1;

    private float degreeInRad = 1.5707963268f;

    // Init function
    public override void _Ready()
    {
        G = GetNode<Globals>("/root/Globals");
        V = GetNode<Values>("/root/Values");

        platformsSpace = GetNode<Spatial>("Platforms");
	    decorationsSpace = GetNode<Spatial>("Decorations");
        
        // Rotate starting platform
        float rotation = (int)G.GenerateStartingPlatformPos();

        rotation *= -degreeInRad;
        platformsSpace.GetChild<Spatial>(0).RotateY(rotation);
    }

    // Create floating cubes decorations
    public void CreateDecorations()
    {
        Vector3 blockPos = new Vector3();

        if (G.firstMove) // Idle animation position fix
        {
            blockPos = G.playerPosition;
        }
        else // Future move pos
        {
            blockPos = G.DirectionCalc();
        }

        // Random offset
        blockPos = G.DecorationsCalc(blockPos);

        LoadDecoration(blockPos);
    }

    private void LoadDecoration(Vector3 blockPos)
    {
        String blockPath;

        if (G.perspectiveMode)
        {
            blockPath = "res://scenes/BlockM.tscn";
        }
        else
        {
            blockPath = "res://scenes/Block.tscn";
        }

        PackedScene block = (PackedScene)ResourceLoader.Load(blockPath);
                
        Spatial blockI = (Spatial)block.Instance();
        blockI.Translation = blockPos;
        
        decorationsSpace.AddChild(blockI);
        totalDeco++;
    }

    public void GeneratePlatform()
    {
        PlaftormType platformType = G.GetPlatformType();

        switch (platformType)
        {
            case PlaftormType.Long:
                PlatformLong();
                break;

            case PlaftormType.Corner:
                platformCorner();
                break;

            case PlaftormType.Cross:
                PlatformCross();
                break;

            case PlaftormType.Twoway:
                PlatformTwoWay();
                break;
        }

        totalPlatforms++;

        if (totalPlatforms >= history)
        {
            RemoveOldPlatforms();
        }
    }

    private void PlatformLong()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("Long");

        // Rotate by 90 when animDirection uneven
        if ((int)G.animationDirection % 2 != 0)
        {
            Vector3 rotationL = new Vector3(0, 90, 0);
            platformBlockI.RotationDegrees =  rotationL;
        }

        G.possibleMoves = new Direction[1]{G.animationDirection};
    }

    private void platformCorner()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("Corner");

        bool doReverse = G.RandomBool();
        int reverse = 0;

        // Change direction of the corner to the other side
        if (doReverse)
        {
            reverse++;
        }

        float yRot = ((int)G.animationDirection + reverse) * -90;

        Vector3 rotationV = new Vector3(0, yRot, 0);
        platformBlockI.RotationDegrees =  rotationV;

        // Check direction change
        int doReverseInt = doReverse ? 0 : 1;

        G.possibleMoves = new Direction[1]
        {
            V.cornerMoves[(int)G.animationDirection, doReverseInt]
        };
    }

    private void PlatformCross()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("Cross");

        // Only opposite direction is removed from the array
        G.possibleMoves = new Direction[3];

        for (int i = 0; i < 3; i++)
        {
            G.possibleMoves[i] = V.crossMoves[(int)G.animationDirection, i];
        }
    }

    private void PlatformTwoWay()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("TwoWay");

        // Get new random orientation of the platform
        Random rnd = new Random();
        int side = rnd.Next(3);

        float yRotT = (int)G.animationDirection * -90;

        if (side == 1)
        {
            yRotT += 90;
        }

        if (side == 2)
        {
            yRotT += -90;
        }

        Vector3 rotationT = new Vector3(0, yRotT, 0);
        platformBlockI.RotationDegrees =  rotationT;

        // Get valid moves based on new rotation
        G.possibleMoves = new Direction[2];

        for (int i = 0; i < 2; i++)
        {
            G.possibleMoves[i] = V.twowayMoves[(int)G.animationDirection, side, i];
        }
    }

    private Spatial PlacePlatform(String type)
    {
        PackedScene platformBlock;
        Spatial platformBlockI;
        String platformPath;

        if (G.perspectiveMode)
        {
            platformPath = "res://scenes/platformsM/"+type+"M.tscn";
        }
        else
        {
            platformPath = "res://scenes/platforms/"+type+".tscn";
        }

        platformBlock = (PackedScene)ResourceLoader.Load(platformPath);

        platformBlockI = (Spatial)platformBlock.Instance();

        // Get platform future pos
        platformBlockI.Translation = G.DirectionCalc();

        // Starting animation fix
        Vector3 temp = platformBlockI.Translation;
        temp.y = -16;
        platformBlockI.Translation = temp;

        platformsSpace.AddChild(platformBlockI);
        return platformBlockI;
    }

    private void RemoveOldPlatforms()
    {
        int childIndex = totalPlatforms - history;
        Spatial child = platformsSpace.GetChild<Spatial>(childIndex);

        totalPlatforms--;
        child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Down");
    }
}