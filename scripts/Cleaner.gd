extends Node3D


func _on_animation_finished(_animation):
	self.queue_free()
