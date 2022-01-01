extends Spatial

# Removes blocks and platforms after animation
func _on_AnimationPlayer_animation_finished(anim_name):

	var anims = ["Hide", "Down", "Show"]

	for string in anims:
		if anim_name == string:
			self.queue_free()
