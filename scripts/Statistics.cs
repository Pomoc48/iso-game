using Godot;
using System;

public class Statistics : TextureButton
{
    private Globals G;
    private Interface I;

    private Label statsText;

    private Texture openTexture;
    private Texture closeTexture;
    
    private bool statsOpened = false;

    // numberOfGames combinedScore correctMoves totalMoves
    private int[] stats = new int[4];

    public override void _Ready()
    {
        G = GetNode<Globals>("/root/Globals");
        I = GetNode<Interface>("/root/Level/Interface");

        String path2 = "/root/Level/Interface/Main/Stats/Stats";
        statsText = GetNode<Label>(path2);

        openTexture = (Texture)GD.Load("res://assets/textures/Stats.png");
        closeTexture = (Texture)GD.Load("res://assets/textures/Close.png");

        LoadStatistics();
    }

    private void LoadStatistics()
    {
        for (int i = 0; i < 4; i++)
        {
            stats[i] = G.Load(G.categoriesP[i+1]);
        }

        float percentageFloat = 0f;

        if (stats[3] != 0)
        {
            percentageFloat = stats[2] / (float)stats[3];
            percentageFloat *= 100; 
        }

        String percentageS = percentageFloat.ToString("F");

        String games = "Number of games: " + stats[0] + "\n";
        String score = "Combined score: " + stats[1] + "\n";
        String percentage = "Correct move percentage: " + percentageS + "%\n";
        String moves = "Total moves: " + stats[3] + "\n";

        statsText.Text = games + score + percentage + moves;
    }

    public void UploadStatistics()
    {
        int games = stats[0] + 1;
        int score = stats[1] + G.sessionScore;
        int cMoves = stats[2] + G.correctMoves;
        int moves = stats[3] + G.totalMoves;

        int[] values = {G.highScore, games, score, cMoves, moves};

        G.Save(G.categoriesP, values);
    }

    private void _on_StatsButton_button_down()
    {
        if (statsOpened)
        {
            I.ShowStatistics(false);
            this.TextureNormal = openTexture;

            statsOpened = false;
        }
        else
        {
            I.ShowStatistics(true);
            this.TextureNormal = closeTexture;

            statsOpened = true;
        }
    }
}