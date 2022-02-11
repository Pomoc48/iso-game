using Godot;
using System;

public class Statistics : TextureButton
{
    private Globals Globals;
    private Interface Interface;

    private Label _statsTextLabel;
    
    private bool _statsOpened = false;

    // numberOfGames combinedScore correctMoves totalMoves
    private int[] _statsArray = new int[4];

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Interface = GetNode<Interface>("/root/Level/Interface");

        String rootPath = "/root/Level/Interface/Main/Stats/Stats";
        _statsTextLabel = GetNode<Label>(rootPath);

        _LoadStatistics();
    }

    public void UploadStatistics()
    {
        int games = _statsArray[0] + 1;
        int score = _statsArray[1] + Globals.sessionScore;
        int correctMoves = _statsArray[2] + Globals.correctMoves;
        int moves = _statsArray[3] + Globals.totalMoves;

        int[] values = {Globals.highScore, games, score, correctMoves, moves};

        Globals.SaveStats(Globals.categoriesArray, values);
    }

    private void _LoadStatistics()
    {
        for (int i = 0; i < 4; i++)
        {
            _statsArray[i] = Globals.LoadStats(Globals.categoriesArray[i+1]);
        }

        float percentageFloat = 0f;

        if (_statsArray[3] != 0)
        {
            percentageFloat = _statsArray[2] / (float)_statsArray[3];
            percentageFloat *= 100; 
        }

        // Limit float to 2 decimal places
        String percentageString = percentageFloat.ToString("F");

        String games = "Number of games: " + _statsArray[0] + "\n";
        String score = "Combined score: " + _statsArray[1] + "\n";
        String percentage = "Correct move percentage: " + percentageString + "%\n";

        _statsTextLabel.Text = percentage + games + score;
    }

    private void _OnStatsButtonDown()
    {
        if (_statsOpened)
        {
            _CloseStats();
        }
        else
        {
            _OpenStats();
        }
    }

    private void _OpenStats()
    {
        Interface.ShowStatistics();
        this.TextureNormal = Globals.closeTexture;

        _statsOpened = true;
    }

    private void _CloseStats()
    {
        Interface.HideStatistics();
        this.TextureNormal = Globals.openTexture;

        _statsOpened = false;
    }
}
