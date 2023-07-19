extends Control


var _level

var _score_label: Label
var _health_bar: Control
var _health_bar_cr: ColorRect

var _screen_size: float
var _update_health_by: float
var _health_bar_showed: bool


func _ready():
	_level = get_node("/root/Level")

	var high_score_label = get_node("Main/HighScore") as Label
	high_score_label.text = _get_previous_highsore()

	_score_label = get_node("Main/Score")
	_health_bar = get_node("Main/Health")
	_health_bar_cr = get_node("Main/Health/Bar")

	_get_screen_size()
	update_healthbar_color()


func calculate_healthbar():
	if not _health_bar_showed:
		_health_bar_showed = true

	var health = Globals.player_health * _update_health_by
	var new_position = Vector2(health, 16)

	_health_bar.set_size(new_position, false)


func update_score():
	var tween = self.create_tween()
	var speed = Globals.animation_speed / 2
	# tween.set_trans(Tween.TRANS_EXPO)
	
	tween.tween_property(_score_label, "label_settings:font_size", 86, speed)
	_score_label.text = str(Globals.session_score)
	tween.tween_property(_score_label, "label_settings:font_size", 72, speed)


func update_healthbar_color():
	var hue = Globals.get_emission_material(0.25)
	_health_bar_cr.color = hue.emission


func show_healthbar():
	var tween = self.create_tween()
	tween.set_trans(Tween.TRANS_EXPO)
	tween.tween_property(_health_bar, "position:y", 0, 0.5)


func hide_healthbar():
	_health_bar.position.y = -16


func _get_screen_size():
	_screen_size = get_viewport().size.x
	_update_health_by = _screen_size / Globals.FULL_HEALTH


func _get_previous_highsore() -> String:
	Globals.high_score = Globals.load_stats("HighScore")
	return "HiScore: " + str(Globals.high_score)


func _on_up_button_down():
	_level.check_move(0)


func _on_right_button_down():
	_level.check_move(1)


func _on_down_button_down():
	_level.check_move(2)


func _on_left_button_down():
	_level.check_move(3)
