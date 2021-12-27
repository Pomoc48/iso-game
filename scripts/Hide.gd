extends Spatial

# Removes blocks and platforms after animation
func _on_AnimationPlayer_animation_finished(anim_name):

	if anim_name == "Hide" or anim_name == "Down" or anim_name == "Show":
		self.queue_free()
