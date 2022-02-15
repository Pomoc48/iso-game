extends Control


var _level
var _interface_animation: AnimationPlayer

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

	_interface_animation = get_node("InterfaceAnim")
	_score_label = get_node("Main/Score")
	_health_bar = get_node("Main/Health")
	_health_bar_cr = get_node("Main/Health/Bar")

	_get_screen_size()


func calculate_healthbar():
	if not _health_bar_showed:
		_interface_animation.play("healthbar_show")
		_health_bar_showed = true

	var health = Globals.player_health * _update_health_by
	var position = Vector2(health, 16)

	_health_bar.set_size(position, false)


func calculate_perspective_bar(frames: float):
	# Game physics runs 60f/s, 5s is 300f
	frames /= -Globals.FIVE_SEC_IN_FRAMES
	frames += 1

	var new_size: float = _screen_size * frames
	var position = Vector2(new_size, 16)

	_health_bar.set_size(position, false)


func update_score():
	_score_label.text = str(Globals.session_score)

	# Fix camera animation freezing
	if not _interface_animation.is_playing():
		_interface_animation.play("score_bump")


func update_healthbar_color():
	var hue = Globals.get_emission_material(0.25)
	_health_bar_cr.color = hue.emission


func play_interface_animation(animation: String):
	_interface_animation.play(animation)


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
