extends Control

# ---------------- Configuration ----------------
const BACKEND_URL := "http://127.0.0.1:5000/plan"
# Path to the compiled DSL DLL, relative to the Godot project folder.
const DSL_DLL_RELATIVE := "../dsl_temp/bin/Release/net10.0/dsl.dll"

# Implicit header values — added automatically before the user's input.
# Replace with values from your real session state when you have one.
const SCHEMA_VERSION := "1.0"
const GAME_ID        := "testgame"
const PLAYER_ID      := "testplayer"
# -----------------------------------------------

@onready var text_edit: TextEdit = $VBoxContainer/Terminal
@onready var submit_button: Button = $VBoxContainer/SubmitButton

var _http: HTTPRequest

func _ready() -> void:
	submit_button.pressed.connect(_on_submit_pressed)
	_http = HTTPRequest.new()
	add_child(_http)
	_http.request_completed.connect(_on_request_completed)

func _on_submit_pressed() -> void:
	var user_input := text_edit.text
	var dsl_source := _build_dsl_source(user_input)
	print("DSL source:\n", dsl_source)

	var json_body := _run_dsl(dsl_source)
	if json_body.is_empty():
		return
	print("POST body:\n", json_body)

	_submit_note(json_body)

# ----------------------------------------------------------------
# Header injection
# ----------------------------------------------------------------

func _build_dsl_source(user_text: String) -> String:
	return "Schema version: %s\nGame Id: %s\nPlayer Id: %s\n\n%s" % [
		SCHEMA_VERSION, GAME_ID, PLAYER_ID, user_text
	]

# ----------------------------------------------------------------
# DSL invocation — runs the F# binary via dotnet, returns its JSON output
# ----------------------------------------------------------------

func _run_dsl(source: String) -> String:
	var input_abs := ProjectSettings.globalize_path("user://dsl_input.txt")
	var output_abs := ProjectSettings.globalize_path("user://dsl_output.json")
	var project_root := ProjectSettings.globalize_path("res://")
	var dll_abs := (project_root + DSL_DLL_RELATIVE).simplify_path()

	# Write source to the temp input file.
	var f := FileAccess.open(input_abs, FileAccess.WRITE)
	if f == null:
		push_error("Could not open temp input file")
		return ""
	f.store_string(source)
	f.close()

	# Run: dotnet <dll> <input> <output>
	var output: Array = []
	var exit_code := OS.execute("dotnet", [dll_abs, input_abs, output_abs], output, true)
	if exit_code != 0:
		push_error("DSL failed (exit %d):\n%s" % [exit_code, "\n".join(output)])
		return ""

	# Read the JSON the DSL wrote.
	var f2 := FileAccess.open(output_abs, FileAccess.READ)
	if f2 == null:
		push_error("DSL did not produce an output file")
		return ""
	var json_text := f2.get_as_text()
	f2.close()
	return json_text

# ----------------------------------------------------------------
# HTTP POST to backend
# ----------------------------------------------------------------

func _submit_note(json_body: String) -> void:
	var headers := ["Content-Type: application/json"]
	var err := _http.request(BACKEND_URL, headers, HTTPClient.METHOD_POST, json_body)
	if err != OK:
		push_error("Could not start request: %s" % err)
	else:
		print("POSTing to ", BACKEND_URL)

func _on_request_completed(result: int, response_code: int, _headers: PackedStringArray, body: PackedByteArray) -> void:
	var body_text := body.get_string_from_utf8()
	if result != HTTPRequest.RESULT_SUCCESS:
		push_error("Request failed at transport layer (code %d). Backend running on 5020?" % result)
		return
	print("Response %d: %s" % [response_code, body_text])
	if response_code == 200:
		print("Plan accepted!")
	elif response_code == 400:
		push_warning("Backend rejected the plan. See response body above.")
