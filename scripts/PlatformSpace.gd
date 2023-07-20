extends Node3D


var _level


var _history = 4
var _total = 1

var _chances = [
	[10, 40, 75],
	[5, 35, 60],
	[2, 22, 40]
]


func _ready():
	_level = get_node("../")


func generate():
	_place(_get_type())
	_total += 1

	if _total >= _history:
		_remove_old()
		

func clear_playfield():
	get_children().map(_play_fade)
	
	var timer = Timer.new()
	
	timer.wait_time = Globals.animation_speed * 2
	timer.one_shot = true
	timer.autostart = true
	
	timer.connect("timeout", func(): _on_timer_timeout(timer))
	
	add_child(timer)
	

func _on_timer_timeout(timer):
	remove_child(timer)
	_place("Start", true)
	
	_history = 4
	_total = 1
	
	_level.new_game()


func _get_type() -> String:
	var difficulty = Globals.platform_diff
	var random = randi() % 100
	var types = ["Cross", "TwoWay", "Long", "Corner"]

	for i in 3:
		if random < _chances[difficulty][i]:
			return types[i]

	return types[3]


func _place(type, override_position = false):
	var platform_path = "res://scenes/platforms/"+type+".tscn"

	var platform_block = load(platform_path) as PackedScene
	var block_instance = platform_block.instantiate() as Node3D

	if override_position:
		var new_position = Globals.player_position
		new_position.y = 0
		
		block_instance.position = new_position
	else:
		block_instance.position = Globals.get_future_position()
		
	add_child(block_instance)


func _remove_old():
	var child_index = _total - _history
	var child = self.get_child(child_index) as Node3D

	_total -= 1
	_play_fade(child)


func _play_fade(node):
	node.get_node("Node3D").play_fade_out_animation()
