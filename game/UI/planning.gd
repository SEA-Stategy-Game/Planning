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
# Keywords for intellisense, man tilføjer bare ting så vil den prøve at suggest ting
const DSL_KEYWORDS := ["MoveTo", "Harvest", "Construct", "if", "END", "unit"]


@onready var text_edit: CodeEdit = $VBoxContainer/Terminal
@onready var submit_button: Button = $VBoxContainer/SubmitButton

var _http: HTTPRequest

func _ready() -> void:
	submit_button.pressed.connect(_on_submit_pressed)
	_http = HTTPRequest.new()
	add_child(_http)
	_http.request_completed.connect(_on_request_completed)
	text_edit.code_completion_enabled = true
	text_edit.text_changed.connect(_on_text_changed)

func _on_submit_pressed() -> void:
	var user_input := text_edit.text
	var dsl_source := _build_dsl_source(user_input)
	print("DSL source:\n", dsl_source)

	var json_body := _run_dsl(dsl_source)
	if json_body.is_empty():
		return
	print("POST body:\n", json_body)

	_submit_note(json_body)

func _on_text_changed() -> void:
	var line := text_edit.get_caret_line()
	var col := text_edit.get_caret_column()
	var line_text: String = text_edit.get_line(line)
	var before_cursor := line_text.substr(0, col)
	var stripped := before_cursor.lstrip(" \t")

	# Case A — after `unit `: suggest live unit IDs
	if stripped.begins_with("unit ") or stripped.begins_with("unit\t"):
		var rest := stripped.substr(4).lstrip(" \t")
		# already typed colon or moved past the id → no popup
		if ":" in rest or " " in rest or "\t" in rest:
			text_edit.cancel_code_completion()
			return
		for uid in _get_live_unit_ids():
			text_edit.add_code_completion_option(CodeEdit.KIND_VARIABLE, uid, uid)
		text_edit.update_code_completion_options(false)
		return

	# Below cases need to be on the first word of the line
	if stripped.is_empty() or " " in stripped or "\t" in stripped:
		text_edit.cancel_code_completion()
		return

	# Case B — typing a `Unit...` reference: suggest `Unit<id>`
	if "Unit".begins_with(stripped) or stripped.begins_with("Unit"):
		for uid in _get_live_unit_ids():
			var label := "Unit" + uid
			text_edit.add_code_completion_option(CodeEdit.KIND_VARIABLE, label, label)
		text_edit.update_code_completion_options(false)
		return

	# Case C — fall back to keywords
	for kw in DSL_KEYWORDS:
		text_edit.add_code_completion_option(CodeEdit.KIND_PLAIN_TEXT, kw, kw)
	text_edit.update_code_completion_options(false)


func _get_live_unit_ids() -> Array[String]:
	var ids: Array[String] = []
	var gateway = get_node_or_null("/root/ActionGateway")
	if gateway == null or not gateway.has_method("get_all_units"):
		return ids
	for u in gateway.get_all_units():
		var id_val = u.get("id") if u else null
		if id_val != null:
			ids.append(str(id_val))
	return ids
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
