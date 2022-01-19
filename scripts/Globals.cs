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

public class Globals : Node
{
    private Random rnd = new Random();
    private ConfigFile CF = new ConfigFile();

    public Vector3 playerPosition;
    public String prevBlock;

    public SpatialMaterial emissionRed;
    public SpatialMaterial emissionBlue;

    public Direction cameraRotation;
    public Direction animationDirection;

    public int fullHealth = 24;
    public int fullMove = 20;

    public float playerHealth;
    public bool firstMove;
    public bool perspectiveMode;

    public int sessionScore;
    public int highScore;
    public int correctMoves;
    public int totalMoves;

    public Direction[] possibleMoves;

    public String[] categoriesP = {
        "HighScore",
        "NumberOfGames", 
        "CombinedScore",
        "CorrectMoves",
        "TotalMoves"
    };
    
    private int cyclesCount;

    private PlafformDifficulty platformDifficulty;
    private Direction startDirection;

    public override void _Ready()
    {
        String matPath = "res://materials/emission";
        emissionRed = (SpatialMaterial)GD.Load(matPath + "2.tres");
        emissionBlue = (SpatialMaterial)GD.Load(matPath + ".tres");
    }

    public void NewGame()
    {
        playerHealth = fullHealth;
        firstMove = true;
        perspectiveMode = false;
        cameraRotation = Direction.LeftUp;
        sessionScore = 0;
        platformDifficulty = 0;
        cyclesCount = 0;
        correctMoves = 0;
        totalMoves = 0;
    }

    public void Save(String[] categories, int[] values)
    {
        // Loop through all the categories
        for (int n = 0; n < categories.Length; n++)
        {
            CF.SetValue("Main", categories[n], values[n]);
        }
        
        // Write once
        CF.Save("user://config");
    }

    public int Load(String category)
    {
        if (CF.Load("user://config") != Error.Ok)
        {
            int[] values = {0, 0, 0, 0, 0};
            Save(categoriesP, values);
        }

        int number = (int) CF.GetValue("Main", category, 0);
        return number;
    }

    public bool RandomBool()
    {
	    var foo = rnd.Next(100);

        return foo switch
        {
            < 50 => false,
            _ => true
        };
    }

    // Translate directions to vectors
    public Vector3 DirectionCalc()
    {
        Vector3 vect = playerPosition;

        switch (animationDirection)
        {
            case Direction.RightDown:
                vect.z += fullMove;
                break;

            case Direction.LeftDown:
                vect.x += -fullMove;
                break;

            case Direction.LeftUp:
                vect.z += -fullMove;
                break;

            default: // Direction.RighUp
                vect.x += fullMove;
                break;
        }
            
        return vect;
    }

    public bool IsMoveLegal()
    {
        if (firstMove)
        {
            return FirstMoveCheck();
        }

        totalMoves++;

        foreach (Direction n in possibleMoves)
        {
            if (animationDirection == n)
            {
                correctMoves++;
                return true;
            }
        }

        return false;
    }

    private bool FirstMoveCheck()
    {
        if (animationDirection == startDirection)
        {
            firstMove = false;

            correctMoves++;
            totalMoves++;

            return true;
        }

        return false;
    }

    public PlaftormType GetPlatformType()
    {
        int[] diffList;

        switch (platformDifficulty)
        {
            case PlafformDifficulty.Easy:
            {
                int[] easyChances = {10, 40, 75};
                diffList = easyChances;
                break;
            }
            
            case PlafformDifficulty.Medium:
            {
                int[] mediumChances = {5, 35, 60};
                diffList = mediumChances;
                break;
            }

            default: // PlafformDifficulty.Hard
            {
                int[] hardChances = {2, 22, 40};
                diffList = hardChances;
                break;
            }
        }

        return GetPlatformChances(diffList);
    }

    private PlaftormType GetPlatformChances(int[] diffList)
    {
        int chance = rnd.Next(100);

        if (chance < diffList[0])
        {
            return PlaftormType.Cross;
        }

        if (chance >= diffList[0] && chance < diffList[1])
        {
            return PlaftormType.Twoway;
        }

        if (chance >= diffList[1] && chance < diffList[2])
        {
            return PlaftormType.Long;
        }

        return PlaftormType.Corner;
    }

    public Direction RetranslateDirection(Direction direction)
    {
        // (Clockwise)
        if (cameraRotation != Direction.LeftUp)
        {
            direction -= (cameraRotation + 1);
        }

        // Reverse overflow check
		if (direction < 0)
        {
            direction += 4;
        }

	    return direction;
    }

	public Vector3 DecorationsCalc(Vector3 centerPos)
    {
        int rangeS;
        int rangeS2;

        if (RandomBool())
        {
            rangeS = rnd.Next(16, 24);
            rangeS2 = rnd.Next(24);
        }
        else
        {
            rangeS2 = rnd.Next(16, 24);
            rangeS = rnd.Next(24);
        }

        if (RandomBool())
        {
            centerPos.x += rangeS;
        }
        else
        {
            centerPos.x -= rangeS;
        }

        if (RandomBool())
        {
            centerPos.z += rangeS2;
        }
        else
        {
            centerPos.z -= rangeS2;
        }

        centerPos.y = 2;
        return centerPos;
    }

    // Get new cycle lenght for camera rotation
    public int GetMaxCycle(int cycle, int range)
    {
        cyclesCount++;

        // Increase platform difficulty every ~100 points 
        if (cyclesCount > 5)
        {
            cyclesCount = 0;

            if (platformDifficulty < PlafformDifficulty.Hard)
            {
                platformDifficulty++;
            }
        }

        int temp;

        if (RandomBool())
        {
            temp = cycle + rnd.Next(range);
        }
        else
        {
            temp = cycle - rnd.Next(range);
        }

        return temp;
    }

    public Direction GenerateStartingPlatformPos()
    {
        startDirection = (Direction)rnd.Next(4);
        return startDirection;
    }

    public int RandomRotationAmmount()
    {
        var randomChance = rnd.Next(100);

        return randomChance switch
        {
            < 5 => 3,
            >= 5 and < 30 => 2,
            _ => 1
        };
    }
}