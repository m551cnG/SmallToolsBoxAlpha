extends Node

var configFile_kill: bool = true # 用于判断是否需要重新加载配置文件

func _ready():
	if configFile_kill or Engine.is_editor_hint():
		configFileKill()

	initial_file_loading()

func configFileKill():
	var result = IOControl_MinLib.DeletePath("user://Config data", false)
	if result.success:
		print("[删除配置文件成功]")
	else:
		printerr("[删除配置文件失败]：", result.problem)

# 加载配置文件
func initial_file_loading():
	var result = IOControl_MinLib.Exists("user://Config data", true)
	if not result.success:
		print("[配置文件不存在，正在创建...]")
		var copy_result = IOControl_MinLib.CopyDirectory("res://data for user/Config data", "user://Config data", true)
		if copy_result.success:
			print("[配置文件复制成功！]")
		else:
			printerr("[配置文件复制失败]：", copy_result.problem)
	else:
		print("[配置文件已存在]")
