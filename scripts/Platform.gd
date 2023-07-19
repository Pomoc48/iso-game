extends Node3D


var _border: MeshInstance3D


func _ready():
	_border = get_node("Border")

	_recolor()


func play_fade_out_animation():
	var material = _border.get_surface_override_material(0) as StandardMaterial3D
	
	_play_tween_animation(material, 0, true)


func _recolor():
	var material: StandardMaterial3D = Globals.get_emission_material(0)
	material.emission_energy_multiplier = 0
	
	_border.set_surface_override_material(0, material)
	_play_tween_animation(material, 1)


func _play_tween_animation(material, finalValue, destroy = false):
	var speed = Globals.animation_speed
	speed -= 0.05
	
	var tween = self.create_tween()
	
	tween.set_trans(Tween.TRANS_SINE)
	tween.tween_property(material, "emission_energy_multiplier", finalValue, speed)
	
	if destroy:
		tween.tween_callback(func(): self.get_parent().queue_free())
