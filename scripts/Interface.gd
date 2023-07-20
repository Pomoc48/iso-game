extends Control


var _level

var _score_label: Label
var _high_score_label: Label
var _health_bar: Control
var _health_bar_cr: ColorRect

var _screen_size: float
var _update_health_by: float
var _health_bar_showed: bool

var _config_file = ConfigFile.new()


func _ready():
	_level = get_node("/root/Level")

	_score_label = get_node("Main/Score")
	_high_score_label = get_node("Main/HighScore")

	_health_bar = get_node("Main/Health")
	_health_bar_cr = get_node("Main/Health/Bar")

	_get_screen_size()
	_load_high_score()
	update_healthbar_color()


func save_high_score(value):
	_config_file.set_value("Main", "high_score", value)
	_config_file.save("user://config")
	
	_high_score_label.text = "HiScore: " + str(value)


func _load_high_score():
	var err = _config_file.load("user://config")

	if err != OK:
		return

	var hi_score: int = _config_file.get_value("Main", "high_score", 0)
	Globals.high_score = hi_score
	
	if hi_score != 0:
		_high_score_label.text = "HiScore: " + str(hi_score)
		

func calculate_healthbar():
	if not _health_bar_showed:
		_health_bar_showed = true

	var health = Globals.player_health * _update_health_by
	var new_position = Vector2(health, 16)

	_health_bar.set_size(new_position, false)


func update_score():
	var tween = self.create_tween()
	var speed = Globals.animation_speed / 2
	
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


func _on_up_button_down():
	_level.check_move(0)


func _on_right_button_down():
	_level.check_move(1)


func _on_down_button_down():
	_level.check_move(2)


func _on_left_button_down():
	_level.check_move(3)
