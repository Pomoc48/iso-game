extends Spatial


var _player
var _interface
var _platforms
var _statistics

var _frame_count = 0
var _failed_frame_count = 0
var _difficulty_cycle = 0
var _speedup_counter = 0

var _increase_score_by = 1
var _max_difficulty_cycle = 20
var _life_loss_rate = 0.04
var _life_gain_rate = 2.0

var _is_player_dead = false
var _player_can_move = false

var _inputs = [
	"ui_up",
	"ui_right",
	"ui_down", 
	"ui_left",
]


func _ready():
	_player = get_node("Player")
	_interface = get_node("Interface")
	_statistics = _interface.get_node("Main/StatsButton")
	_platforms = get_node("Platforms")

	Globals.NewGame()
	_difficulty_cycle = Globals.GetNextCycle(_max_difficulty_cycle);

	_player.update_color()


func _process(_delta):
	if (Input.is_action_pressed(_inputs[0])
			and Input.is_action_pressed(_inputs[1])):
		check_move(0)

	if (Input.is_action_pressed(_inputs[1])
			and Input.is_action_pressed(_inputs[2])):
		check_move(1)

	if (Input.is_action_pressed(_inputs[2])
			and Input.is_action_pressed(_inputs[3])):
		check_move(2)	

	if (Input.is_action_pressed(_inputs[3])
			and Input.is_action_pressed(_inputs[0])):
		check_move(3)


func _physics_process(_delta):
	if Globals.playerHealth <= 0 and not _is_player_dead:
		_game_over()

	if not Globals.perspectiveMode and not Globals.firstMove:
		_loose_health_on_tick()

	if Globals.perspectiveMode:
		_calculate_perspective_frames()


func toggle_controls(enable):
	_player_can_move = enable


func check_move(direction):
	if not _player_can_move:
		return

	toggle_controls(false)
	Globals.animationDirection = _translate_direction(direction)

	if _is_move_valid():
		_correct_move()
	else:
		_wrong_move(direction)


func _translate_direction(direction) -> int:
	if Globals.cameraRotation != 3:
		direction -= Globals.cameraRotation + 1

	return wrapi(direction, 0, 4)


func _is_move_valid() -> bool:
	Globals.totalMoves += 1

	for direction in Globals.possibleMoves:
		if Globals.animationDirection == direction:
			Globals.correctMoves += 1
			return true

	return false


func _correct_move():
	_player.animate_movement()

	Globals.sessionScore += _increase_score_by
	_interface.update_score()

	_roll_perspective_mode()
	Globals.UpdateEmissionMaterial()

	_platforms.generate()
	_player.update_color()
	_interface.update_healthbar_color()

	_give_player_health(_life_gain_rate)
	_difficulty_increase()

	if Globals.firstMove:
		Globals.firstMove = false


func _roll_perspective_mode():
	if not Globals.perspectiveMode:
		var chance = Globals.GetRandomNumber(100)
		_check_perspective_mode_chances(chance)


func _difficulty_increase():
	_speedup_counter += 1

	if _speedup_counter < _difficulty_cycle:
		return

	_speedup_counter = 0
	_life_loss_rate += 0.01
	_life_gain_rate += 0.25

	# Cap player speed
	var new_speed = Globals.animationSpeed - 0.005
	Globals.animationSpeed = clamp(new_speed, 0.15, 0.25)

	_generate_new_difficulty_cycle()
	_player.rotate_camera_by(_get_random_rotation_ammount())


func _generate_new_difficulty_cycle():
	if _max_difficulty_cycle != 5:
		_max_difficulty_cycle -= 1

	_difficulty_cycle = Globals.GetNextCycle(_max_difficulty_cycle)


func _get_random_rotation_ammount() -> int:
	var random_chance = Globals.GetRandomNumber(100)

	if random_chance < 5:
		return 3
	elif random_chance < 30:
		return 2
	else:
		return 1


func _check_perspective_mode_chances(chance):
	# 1% chance to activate special mode after 20 moves
	if chance < 1 and _failed_frame_count >= 20:
		_enable_perspective_mode()
		return

	_failed_frame_count += 1

	# Quadruple the chances after unlucky 100 moves
	if chance < 4 and _failed_frame_count >= 100:
		_enable_perspective_mode()


func _wrong_move(direction):
	if not Globals.perspectiveMode:
		_take_player_health()

	if not _is_player_dead:
		var animation_name = "bounce" + str(direction)
		_player.play_spatial_animation(animation_name)


func _take_player_health():
	if not Globals.firstMove:
		Globals.playerHealth -= 10

	if Globals.playerHealth <= 0 and not _is_player_dead:
		_game_over()


func _loose_health_on_tick():
	Globals.playerHealth -= _life_loss_rate
	_interface.calculate_healthbar()


func _calculate_perspective_frames():
	_frame_count += 1

	if _frame_count >= Globals.FIVE_SEC_IN_FRAMES:
		_frame_count = 0
		_disable_perspective_mode()


func _give_player_health(ammount):
	var new_health = Globals.playerHealth + ammount
	Globals.playerHealth = clamp(new_health, 0, Globals.FULL_HEALTH)


func _enable_perspective_mode():
	_failed_frame_count = 0
	Globals.perspectiveMode = true

	_interface.play_interface_animation("blind_perspective")
	_interface.calculate_healthbar()

	_give_player_health(Globals.FULL_HEALTH)
	_increase_score_by = 2


func _disable_perspective_mode():
	_interface.play_interface_animation("blind_orthogonal")
	Globals.perspectiveMode = false
	_increase_score_by = 1


func _game_over():
	_is_player_dead = true
	toggle_controls(false)

	if Globals.sessionScore > Globals.highScore:
		Globals.highScore = Globals.sessionScore
		_play_outro_animation_highscore()
	else:
		_play_outro_animation()

	_statistics.upload()


func _play_outro_animation():
	_interface.play_interface_animation("ui_hide")
	_player.play_spatial_animation("camera_up")


func _play_outro_animation_highscore():
	_interface.play_interface_animation("ui_hui_hide_highscoreide")
	_player.play_spatial_animation("camera_up_long")
