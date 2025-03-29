using Godot;
using System;

public partial class JsonHelper : Node
{
    /// <summary>
    /// 通过字符串路径访问 JSON 数据 (支持字典和数组)
    /// </summary>
    /// <param name="data">Godot.Collections.Dictionary 格式的数据</param>
    /// <param name="path">字符串路径，如："game.level1.enemy.0.hp"</param>
    /// <returns>路径对应的数据，若无效返回 null</returns>
    public Variant GetValueByPath(Godot.Collections.Dictionary data, string path,bool debug = false)
    {
        string[] keys = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        Variant current = data;

        foreach (var key in keys)
        {
            if (current.VariantType == Variant.Type.Dictionary)
            {
                var dict = current.AsGodotDictionary();
                if (dict.ContainsKey(key))
                {
                    current = dict[key];
                }
                else
                {
                    if (debug)
                        GD.PrintErr($"路径无效：字典中不存在键 '{key}'");
                    return default;
                }
            }
            else if (current.VariantType == Variant.Type.Array)
            {
                var array = current.AsGodotArray();
                if (int.TryParse(key, out int index))
                {
                    if (index >= 0 && index < array.Count)
                    {
                        current = array[index];
                    }
                    else
                    {
                        if (debug)
                            GD.PrintErr($"路径无效：数组索引 '{key}' 越界");
                        return default;
                    }
                }
                else
                {
                    if (debug)
                        GD.PrintErr($"路径无效：当前是数组，但键 '{key}' 不是有效索引");
                    return default;
                }
            }
            else
            {
                if (debug)
                    GD.PrintErr($"路径无效：'{key}' 之前的数据不是数组或字典");
                return default;
            }
        }

        return current;
    }
}