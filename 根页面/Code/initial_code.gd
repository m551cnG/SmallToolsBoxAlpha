extends Node

@export var configFile_kill: bool = false # 用于判断是否需要重新加载配置文件

func _ready():
	if configFile_kill == true || Engine.is_editor_hint():
		# 重新加载配置文件
		configFileKill()

	initial_file_loading()

func configFileKill():
	IoControlMinLib.DeletePath("user://Config data",false) # 删除配置文件

# 加载配置文件
func initial_file_loading():
	# 检查所需文件是否存在
	var file_bool = IoControlMinLib.Exists("user://Config data",true) # 检查Config data配置文件是否存在
	
	if file_bool == false:
		# 不存在则创建
		IoControlMinLib.CopyDirectory("res://data for user/Config data", "user://Config data", true) # 复制配置文件到用户目录下
		file_bool = true
