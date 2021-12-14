extends Node


const FULL_HEALTH = 24
const FULL_MOVE = 20


var player_health: float = FULL_HEALTH


var firstMove: bool = true


var camera_rotation_index: int = 3
var session_score: int = 0
var anim_direction: int
var prev_direction: int

var prev_block: String


# Globals.session_score