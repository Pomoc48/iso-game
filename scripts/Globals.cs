using Godot;
using System;

public class Globals : Node
{
    public Vector3 playerPosition;

    public SpatialMaterial materialRed;
    public SpatialMaterial materialBlue;

    public Texture openTexture;
    public Texture closeTexture;
    public Texture blueTexture;
    public Texture redTexture;

    public Direction cameraRotation;
    public Direction animationDirection;

    public Color emissionColor;

    public readonly int FULL_HEALTH = 24;
    public readonly float INCREASE_HEIGHT_BY = 1f;

    public bool firstMove;
    public bool perspectiveMode;

    public float playerHealth;
    public float platformHeight;

    public int sessionScore;
    public int highScore;
    public int correctMoves;
    public int totalMoves;

    public Direction[] possibleMoves;

    public String[] categoriesArray = {
        "HighScore",
        "NumberOfGames", 
        "CombinedScore",
        "CorrectMoves",
        "TotalMoves"
    };
    
    private Random _random = new Random();
    private ConfigFile _configFile = new ConfigFile();

    private PlafformDifficulty platformDifficulty;
    private Direction startingDirection;

    private int cyclesCount;
    private readonly int _FULL_MOVE = 20;
    private readonly float _CHANGE_HUE_BY = 0.025f;

    public override void _Ready()
    {
        String matPath = "res://materials/emission";
        materialRed = (SpatialMaterial)GD.Load(matPath + "2.tres");
        materialBlue = (SpatialMaterial)GD.Load(matPath + ".tres");

        openTexture = (Texture)GD.Load("res://assets/textures/Stats.png");
        closeTexture = (Texture)GD.Load("res://assets/textures/Close.png");

        blueTexture = (Texture)GD.Load("res://assets/textures/squareBlue.png");
        redTexture = (Texture)GD.Load("res://assets/textures/squareRed.png");

        emissionColor = materialBlue.Emission;
    }

    public void NewGame()
    {
        playerHealth = FULL_HEALTH;
        firstMove = true;
        perspectiveMode = false;
        cameraRotation = Direction.LeftUp;
        sessionScore = 0;
        platformDifficulty = 0;
        cyclesCount = 0;
        correctMoves = 0;
        totalMoves = 0;
        platformHeight = -1f;
    }

    public void SaveStats(String[] categories, int[] values)
    {
        // Loop through all the categories
        for (int n = 0; n < categories.Length; n++)
        {
            _configFile.SetValue("Main", categories[n], values[n]);
        }
        
        // Write once
        _configFile.Save("user://config");
    }

    public int LoadStats(String category)
    {
        if (_configFile.Load("user://config") != Error.Ok)
        {
            int[] values = {0, 0, 0, 0, 0};
            SaveStats(categoriesArray, values);
        }

        int number = (int) _configFile.GetValue("Main", category, 0);
        return number;
    }

    public bool RandomBool()
    {
        int chance = _random.Next(100);

        return chance switch
        {
            < 50 => false,
            _ => true
        };
    }

    // Translate directions to vectors
    public Vector3 DirectionCalc()
    {
        Vector3 position = playerPosition;

        switch (animationDirection)
        {
            case Direction.RightDown:
                position.z += _FULL_MOVE;
                break;

            case Direction.LeftDown:
                position.x += -_FULL_MOVE;
                break;

            case Direction.LeftUp:
                position.z += -_FULL_MOVE;
                break;

            default: // Direction.RighUp
                position.x += _FULL_MOVE;
                break;
        }
            
        return position;
    }

    public bool IsMoveLegal()
    {
        if (firstMove)
        {
            return _FirstMoveCheck();
        }

        totalMoves++;

        foreach (Direction direction in possibleMoves)
        {
            if (animationDirection == direction)
            {
                correctMoves++;
                return true;
            }
        }

        return false;
    }

    private bool _FirstMoveCheck()
    {
        if (animationDirection == startingDirection)
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
        int[] difficultyChancesList;

        switch (platformDifficulty)
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

        return _GetPlatformChances(difficultyChancesList);
    }

    private PlaftormType _GetPlatformChances(int[] chances)
    {
        int chance = _random.Next(100);

        if (chance < chances[0])
        {
            return PlaftormType.Cross;
        }

        if (chance >= chances[0] && chance < chances[1])
        {
            return PlaftormType.Twoway;
        }

        if (chance >= chances[1] && chance < chances[2])
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

    public Vector3 CalculateDecorationPosition(Vector3 centerPosition)
    {
        int rangeX;
        int rangeZ;

        int randomShort = _random.Next(16, 24);
        int randomWide = _random.Next(24);

        if (RandomBool())
        {
            rangeX = randomShort;
            rangeZ = randomWide;
        }
        else
        {
            rangeX = randomWide;
            rangeZ = randomShort;
        }

        if (RandomBool())
        {
            centerPosition.x += rangeX;
        }
        else
        {
            centerPosition.x -= rangeX;
        }

        if (RandomBool())
        {
            centerPosition.z += rangeZ;
        }
        else
        {
            centerPosition.z -= rangeZ;
        }

        centerPosition.y = 2;
        return centerPosition;
    }

    // Get new cycle lenght for camera rotation
    public int GetNextCycle(int cycle, int range)
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
            temp = cycle + _random.Next(range);
        }
        else
        {
            temp = cycle - _random.Next(range);
        }

        return temp;
    }

    public Direction GenerateStartingPlatformPos()
    {
        startingDirection = (Direction)_random.Next(4);
        return startingDirection;
    }

    public int GetRandomRotationAmmount()
    {
        var randomChance = _random.Next(100);

        return randomChance switch
        {
            < 5 => 3,
            >= 5 and < 30 => 2,
            _ => 1
        };
    }

    public SpatialMaterial RotateHue()
    {
        SpatialMaterial newMaterial = new SpatialMaterial();
        Color _oldEmissionColor = emissionColor;

        emissionColor.h += _CHANGE_HUE_BY;

        if (emissionColor.h > 1)
        {
            emissionColor.h = 0;
        }

        emissionColor.v = 1;
        

        newMaterial.EmissionEnabled = true;
        newMaterial.Emission = emissionColor;

        return newMaterial;
    }
}