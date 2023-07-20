extends Node3D


var _player
var _interface
var _platforms

var _difficulty_cycle = 0
var _speedup_counter = 0

var _max_difficulty_cycle = 20
var _life_loss_rate = 0.04
var _life_gain_rate = 2.0

var _is_player_dead = false

var _inputs = [
	"ui_up",
	"ui_right",
	"ui_down", 
	"ui_left",
]


func _ready():
	_player = get_node("Player")
	_interface = get_node("Interface")
	_platforms = get_node("Platforms")

	Globals.new_game()
	_difficulty_cycle = Globals.get_next_cycle(_max_difficulty_cycle);

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
	if Globals.player_health <= 0 and not _is_player_dead:
		_game_over()
		
	if not Globals.first_move and not _is_player_dead:
		_loose_health_on_tick()


func check_move(direction):
	if not Globals.player_can_move:
		return

	Globals.player_can_move = false;
	Globals.animation_direction = _translate_direction(direction)

	if _is_move_valid():
		_correct_move()
	else:
		_wrong_move(direction)


func _translate_direction(direction) -> int:
	if Globals.camera_rotation != 3:
		direction -= Globals.camera_rotation + 1

	return posmod(direction, 4)


func _is_move_valid() -> bool:
	for direction in Globals.possible_moves:
		if Globals.animation_direction == direction:
			return true

	return false


func _correct_move():
	_player.animate_movement()

	Globals.session_score += 1
	_interface.update_score()

	Globals.update_emission_material()

	_platforms.generate()
	_player.update_color()
	_interface.update_healthbar_color()

	_give_player_health(_life_gain_rate)
	_difficulty_increase()

	if Globals.first_move:
		Globals.first_move = false
		_interface.show_healthbar()


func _difficulty_increase():
	_speedup_counter += 1

	if _speedup_counter < _difficulty_cycle:
		return

	_speedup_counter = 0
	_life_loss_rate += 0.01
	_life_gain_rate += 0.25

	# Cap player speed
	var new_speed = Globals.animation_speed - 0.005
	Globals.animation_speed = clamp(new_speed, 0.15, 0.25)

	_generate_new_difficulty_cycle()
	_player.rotate_camera_by(_get_random_rotation_ammount())


func _generate_new_difficulty_cycle():
	_max_difficulty_cycle = wrapi(_max_difficulty_cycle - 1, 5, 20)
	_difficulty_cycle = Globals.get_next_cycle(_max_difficulty_cycle)


func _get_random_rotation_ammount() -> int:
	var random_chance = randi() % 100

	if random_chance < 5:
		return 3
	elif random_chance < 30:
		return 2
	else:
		return 1


func _wrong_move(direction):	
	_take_player_health()

	if not _is_player_dead:
		var animation_name = "bounce" + str(direction)
		_player.play_spatial_animation(animation_name)


func _take_player_health():
	if not Globals.first_move:
		Globals.player_health -= 10

	if Globals.player_health <= 0 and not _is_player_dead:
		_game_over()


func _loose_health_on_tick():
	Globals.player_health -= _life_loss_rate
	_interface.calculate_healthbar()


func _give_player_health(ammount):
	var new_health = Globals.player_health + ammount
	Globals.player_health = clamp(new_health, 0, Globals.FULL_HEALTH)


func _game_over():
	if Globals.session_score > Globals.high_score:
		_interface.save_high_score(Globals.session_score)
		
	_is_player_dead = true
	
	Globals.player_can_move = false
	_interface.hide_healthbar()
	
	_player.play_kill_cam()
	_platforms.clear_playfield()


func new_game():
	Globals.new_game()
	
	_difficulty_cycle = Globals.get_next_cycle(_max_difficulty_cycle);
	_speedup_counter = 0

	_max_difficulty_cycle = 20
	_life_loss_rate = 0.04
	_life_gain_rate = 2.0

	_is_player_dead = false
