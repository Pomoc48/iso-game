using Godot;
using System;

public class Remove : Spatial
{
    String[] anims = {"Hide", "Down", "Show"};

    private void _on_AnimationPlayer_animation_finished(String anim_name)
    {
        foreach (String anim in anims)
        {
            if (anim_name == anim) this.QueueFree();
        }
    }
}