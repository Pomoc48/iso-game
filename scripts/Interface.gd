extends Control


var player
var healthBar
var scoreText


var healthAnimation
var buttonsAnimLeft
var buttonsAnimRight
var textAnim


var screenSizeCalculated: bool
var healthBarShow: bool = true
var update_health_by: float


# Init function
func _ready():

	healthBar = get_node("Main/Health")
	healthAnimation = get_node("Main/Health/HealthAnim")
	buttonsAnimLeft = get_node("Main/Left/ShowHide")
	buttonsAnimRight = get_node("Main/Right/ShowHide")

	scoreText = get_node("Main/Score")
	textAnim = scoreText.get_node("Bump")

	player = get_node("/root/Level/Player")


# Connect UI buttons
func _on_Left_button_down():
	player.touch_controls(3)


func _on_Right_button_down():
	player.touch_controls(1)


func _on_Down_button_down():
	player.touch_controls(2)


func _on_Up_button_down():
	player.touch_controls(0)


# One time screen size calculation
func _get_screen_size():

	var screenSize = get_viewport().get_visible_rect().size.x
	update_health_by = screenSize / Globals.FULL_HEALTH
	screenSizeCalculated = true


# Calculate healthbar pixels
func calculate_health_bar():

	if healthBarShow:
		healthAnimation.play("HealthDown")
		healthBarShow = false

	if !screenSizeCalculated:
		_get_screen_size()

	var health = Globals.player_health * update_health_by
	var pos = Vector2(health, 16)

	healthBar.set_size(pos, false)


# Gameover call
func hide_ui_animation():

	buttonsAnimRight.play("Hide")
	buttonsAnimLeft.play("Hide")
	textAnim.play("Hide")
	
	healthAnimation.play("HealthUp")


# Update text and play animation
func add_score():

	scoreText.set_text(str(Globals.session_score))
	textAnim.play("TextAnim")
