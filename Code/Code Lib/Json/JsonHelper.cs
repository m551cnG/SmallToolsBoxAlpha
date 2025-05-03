using Godot;
using System;
using System.Collections.Generic;


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
    
    
    // 缓存路径拆分结果，避免重复拆分同一字符串
    private readonly Dictionary<string, string[]> _pathCache = new();

    /// <summary>
    /// 将 "a.b.c" 形式的路径拆分并缓存
    /// </summary>
    private string[] GetSegments(string path)
    {
        if (!_pathCache.TryGetValue(path, out var segments))
        {
            segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            _pathCache[path] = segments;
        }
        return segments;
    }

    /// <summary>
    /// 批量查询：按路径列表返回所有结果，Key=原路径, Value=取到的 Variant
    /// </summary>
    public Dictionary<string, Variant> GetValuesByPaths(
        Godot.Collections.Dictionary data,
        Godot.Collections.Array paths,
        bool debug = false)
    {
        var results = new Dictionary<string, Variant>(paths.Count);
        // Godot.Collections.Array 中枚举出的元素均为 Variant
        foreach (Variant raw in paths)
        {
            if (raw.VariantType != Variant.Type.String)
            {
                if (debug) GD.PrintErr($"[JsonHelper] 非字符串路径项，类型：{raw.VariantType}");
                continue;
            }
            string path = raw.AsString();
            var segments = GetSegments(path);
            var value = GetValueBySegments(data, segments, debug);
            results[path] = value;
        }
        return results;
    }

    /// <summary>
    /// 根据已拆分的 segments 逐层访问字典/数组
    /// </summary>
    private Variant GetValueBySegments(
        Godot.Collections.Dictionary data,
        string[] segments,
        bool debug)
    {
        Variant current = data;
        foreach (var key in segments)
        {
            if (current.VariantType == Variant.Type.Dictionary)
            {
                var dict = current.AsGodotDictionary();
                if (!dict.ContainsKey(key))
                {
                    if (debug) GD.PrintErr($"[JsonHelper] 字典中不存在键 '{key}'");
                    return default;
                }
                current = dict[key];
            }
            else if (current.VariantType == Variant.Type.Array)
            {
                var arr = current.AsGodotArray();
                if (!int.TryParse(key, out var idx) || idx < 0 || idx >= arr.Count)
                {
                    if (debug) GD.PrintErr($"[JsonHelper] 数组索引无效 '{key}'");
                    return default;
                }
                current = arr[idx];
            }
            else
            {
                if (debug) GD.PrintErr($"[JsonHelper] 无法访问 '{key}'，当前类型 = {current.VariantType}");
                return default;
            }
        }
        return current;
    }
}