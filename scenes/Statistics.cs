using Godot;
using System;

public class Statistics : TextureButton
{
    private Globals G;
    private AnimationPlayer statsAnimation;
    
    private bool statsOpened = false;

    public override void _Ready()
    {
        G = GetNode<Globals>("/root/Globals");

        String path = "/root/Level/Interface/Main/Stats/BlindAnim";
        statsAnimation = GetNode<AnimationPlayer>(path);
    }

    private void _on_StatsButton_button_down()
    {
        if (statsOpened)
        {
            statsAnimation.Play("Hide");
            statsOpened = false;
            return;
        }

        statsAnimation.Play("Show");
        statsOpened = true;
    }
}
