using Godot;
using System;

public class Globals : Node
{
    public Vector3 playerPosition;

    public SpatialMaterial materialBlue;

    public Texture openTexture;
    public Texture closeTexture;

    public Direction cameraRotation;
    public Direction animationDirection;
    public Direction[] possibleMoves;

    public Color emissionColor;

    public readonly int FULL_HEALTH = 24;
    public readonly int FIVE_SEC_IN_FRAMES = 600;
    public readonly float INCREASE_HEIGHT_BY = 0.05f;

    public bool firstMove;
    public bool perspectiveMode;

    public float playerHealth;
    public float platformHeight;
    public float animationSpeed;

    public int sessionScore;
    public int highScore;
    public int correctMoves;
    public int totalMoves;

    public String[] categoriesArray = {
        "HighScore",
        "NumberOfGames", 
        "CombinedScore",
        "CorrectMoves",
        "TotalMoves"
    };
    
    private Random _random = new();
    private ConfigFile _configFile = new();

    private PlafformDifficulty platformDifficulty;
    private Direction startingDirection;

    private int cyclesCount;
    private readonly int _FULL_MOVE = 20;
    private readonly float _CHANGE_HUE_BY = 0.025f;

    public override void _Ready()
    {
        String materialPath = "res://materials/emission";
        materialBlue = (SpatialMaterial)GD.Load(materialPath + ".tres");

        openTexture = (Texture)GD.Load("res://assets/textures/Stats.png");
        closeTexture = (Texture)GD.Load("res://assets/textures/Close.png");

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
        animationSpeed = 0.25f;
        totalMoves = 0;
        platformHeight = 0f;
        emissionColor = materialBlue.Emission;

    }

    public void SaveStats(String[] categories, int[] values)
    {
        for (int n = 0; n < categories.Length; n++)
        {
            _configFile.SetValue("Main", categories[n], values[n]);
        }
        
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

    public bool GetRandomBool()
    {
        int chance = _random.Next(100);

        return chance switch
        {
            < 50 => false,
            _ => true
        };
    }

    public Vector3 GetFuturePosition()
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

    public bool IsMoveValid()
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

    public Direction RetranslateDirection(Direction direction)
    {
        if (cameraRotation != Direction.LeftUp)
        {
            direction -= (cameraRotation + 1);
        }

        if (direction < 0)
        {
            direction += 4;
        }

        return direction;
    }

    public int GetNextCycle(int cycle)
    {
        if ((cyclesCount += 1) > 5)
        {
            cyclesCount = 0;

            if (platformDifficulty < PlafformDifficulty.Hard)
            {
                platformDifficulty++;
            }
        }

        return cycle + _random.Next(5);
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
        SpatialMaterial newMaterial = new();
        Color _oldEmissionColor = emissionColor;

        if ((emissionColor.h += _CHANGE_HUE_BY) > 1)
        {
            emissionColor.h = 0;
        }

        emissionColor.v = 1;

        newMaterial.EmissionEnabled = true;
        newMaterial.Emission = emissionColor;

        return newMaterial;
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
}