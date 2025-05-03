using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// JsonHelper —— 通过“点路径”在嵌套 JSON（Dictionary / Array）里取值的工具，
/// 支持单条和批量查询。根节点可以是 Dictionary 也可以是 Array。
///
/// 新增特性：
/// 1. 路径转义：在路径里使用 '\.' 表示字面点号，'\\' 表示字面反斜杠。
/// 2. Try 系列 API：可区分 “路径不存在” 和 “值为 null”。
/// </summary>
public partial class JsonHelper : Node
{
    //──────────────────────────────────────────────────────────────
    // 单条路径查询（保持原接口）
    //──────────────────────────────────────────────────────────────

    /// <summary>
    /// 通过字符串路径在 JSON 结构中查找单个节点的值。<br/>
    /// <para>
    /// 该方法是对外主入口（保持与旧版一致）：  
    /// ‑ 根节点可为 <see cref="Godot.Collections.Dictionary"/> 或
    ///   <see cref="Godot.Collections.Array"/>；  
    /// ‑ 路径段用点号分隔，可用 <c>"\\."</c> 转义字面点号。
    /// </para>
    /// </summary>
    /// <param name="root">
    /// 已 <c>JSON.Parse()</c> 得到的根 <see cref="Godot.Variant"/>。
    /// </param>
    /// <param name="path">
    /// 点号分隔的路径，例如 <c>"game.level1.enemy.0.hp"</c>。
    /// </param>
    /// <param name="debug">
    /// 置 <c>true</c> 时，在键缺失、索引越界等情况下
    /// 通过 <see cref="Godot.GD.PrintErr(object)"/> 打印调试信息。
    /// </param>
    /// <returns>
    /// 目标节点对应的 <see cref="Godot.Variant"/>；  
    /// 若路径无效，则返回 <c>default</c>（即 <c>Variant.Nil</c>）。
    /// </returns>
    public Variant GetValueByPath(Variant root, string path, bool debug = false)
    {
        string[] segments = GetSegments(path);
        return GetValueBySegments(root, segments, debug, out _);
    }

    //──────────────────────────────────────────────────────────────
    // 批量查询（保持原接口）
    //──────────────────────────────────────────────────────────────

    /// <summary>
    /// 批量解析路径并返回结果映射表。  
    /// <para>
    /// ⚠️兼容旧版：若某条路径无效，其值置为 <c>Variant.Nil</c>，  
    /// 调用方需额外判断是否为有效节点。
    /// </para>
    /// </summary>
    /// <param name="root">
    /// JSON 根节点的 <see cref="Godot.Variant"/>。
    /// </param>
    /// <param name="paths">
    /// 路径集合（<see cref="Godot.Collections.Array"/>，元素需为 <c>string</c>）。
    /// </param>
    /// <param name="debug">
    /// 若为 <c>true</c>，当单条路径解析失败时输出调试信息。
    /// </param>
    /// <returns>
    /// 一个 <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>：  
    /// ‑ <c>Key</c> = 原始路径字符串；  
    /// ‑ <c>Value</c> = 解析得到的 <see cref="Godot.Variant"/>（或 <c>Variant.Nil</c>）。
    /// </returns>
    public Dictionary<string, Variant> GetValuesByPaths(
        Variant root,
        Godot.Collections.Array paths,
        bool debug = false)
    {
        var results = new Dictionary<string, Variant>(paths.Count);

        foreach (Variant raw in paths)
        {
            if (raw.VariantType != Variant.Type.String)
            {
                if (debug) GD.PrintErr($"[JsonHelper] 非字符串路径项，类型 = {raw.VariantType}");
                continue;
            }

            string path = raw.AsString();
            results[path] = GetValueBySegments(root, GetSegments(path), debug, out _);
        }
        return results;
    }

    //──────────────────────────────────────────────────────────────
    // Try‑API（新增，原接口不受影响）
    //──────────────────────────────────────────────────────────────
    /// <summary>
    /// 尝试按照单条“点路径”在 <paramref name="root"/> 所代表的
    /// JSON 结构（<see cref="Godot.Collections.Dictionary"/> 或
    /// <see cref="Godot.Collections.Array"/>）中查找目标值。  
    /// 相较于 <see cref="GetValueByPath"/>, 本方法可区分
    /// “路径不存在” 与 “值为 <c>null</c>/Variant.Nil” 的情况。
    /// </summary>
    /// <param name="root">
    /// 解析得到的根 <see cref="Godot.Variant"/>；
    /// 顶层既可以是字典 (<c>{ ... }</c>)，也可以是数组 (<c>[ ... ]</c>)。
    /// </param>
    /// <param name="path">
    /// 以点号分隔的路径字符串，  
    /// 例如 <c>"player.0.hp"</c>；  
    /// 若键名本身包含点号，可使用转义序列 <c>"\\."</c> 表示字面点号。
    /// </param>
    /// <param name="value">
    /// 输出参数；当方法返回 <c>true</c> 时，
    /// 其内容为目标节点的 <see cref="Godot.Variant"/>。
    /// 返回 <c>false</c> 时，该参数设为 <c>default</c>（<c>Variant.Nil</c>）。
    /// </param>
    /// <param name="debug">
    /// 设为 <c>true</c> 时，在路径无效或索引越界等错误场景下
    /// 通过 <see cref="Godot.GD.PrintErr(object)"/> 打印调试信息。
    /// </param>
    /// <returns>
    /// <c>true</c> 表示路径成功解析到目标节点（即使节点值为 <c>null</c>），  
    /// <c>false</c> 表示路径中的某一段不存在或无法继续深入。
    /// </returns>
    public bool TryGetValueByPath(
        Variant root,
        string path,
        out Variant value,
        bool debug = false)
    {
        value = GetValueBySegments(root, GetSegments(path), debug, out bool ok);
        return ok;
    }

    /// <summary>
    /// 批量尝试在 <paramref name="root"/> 里解析多条路径。  
    /// 每条路径的解析结果以 <c>(bool ok, Variant val)</c> 元组形式返回：  
    /// <list type="bullet">
    ///   <item><description>
    ///     <c>ok == true</c>  → 成功解析到目标节点，<c>val</c> 为对应值；
    ///   </description></item>
    ///   <item><description>
    ///     <c>ok == false</c> → 路径无效 / 越界，<c>val</c> 置为 <c>Variant.Nil</c>。
    ///   </description></item>
    /// </list>
    /// </summary>
    /// <param name="root">
    /// JSON 根节点的 <see cref="Godot.Variant"/>。
    /// </param>
    /// <param name="paths">
    /// 任意数量的路径集合（<see cref="Godot.Collections.Array"/>），
    /// 其元素必须为 <see cref="string"/>。
    /// </param>
    /// <param name="debug">
    /// 若为 <c>true</c>，在每条路径解析失败时打印调试信息。
    /// </param>
    /// <returns>
    /// 一个 <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
    /// ：键为原始路径字符串，值为解析结果元组 <c>(ok, val)</c>。
    /// </returns>
    public Dictionary<string, (bool ok, Variant val)> TryGetValuesByPaths(
        Variant root,
        Godot.Collections.Array paths,
        bool debug = false)
    {
        var dict = new Dictionary<string, (bool, Variant)>(paths.Count);

        foreach (Variant raw in paths)
        {
            if (raw.VariantType != Variant.Type.String)
            {
                if (debug) GD.PrintErr($"[JsonHelper] 非字符串路径项，类型 = {raw.VariantType}");
                continue;
            }

            string path = raw.AsString();
            Variant val = GetValueBySegments(root, GetSegments(path), debug, out bool ok);
            dict[path] = (ok, val);
        }
        return dict;
    }

    //──────────────────────────────────────────────────────────────
    // 内部：路径拆分 + 缓存（支持 '\.' 转义）
    //──────────────────────────────────────────────────────────────

    private readonly Dictionary<string, string[]> _pathCache = new();

    private string[] GetSegments(string path)
    {
        if (_pathCache.TryGetValue(path, out var segs))
            return segs;

        var list = new List<string>();
        var segment = new System.Text.StringBuilder();
        bool escaping = false;

        // ★ 唯一改动：使用 ReadOnlySpan<char>
        ReadOnlySpan<char> span = path.AsSpan();

        foreach (char c in span)
        {
            if (escaping)
            {
                segment.Append(c);
                escaping = false;
            }
            else if (c == '\\')
            {
                escaping = true;
            }
            else if (c == '.')
            {
                list.Add(segment.ToString());
                segment.Clear();
            }
            else
            {
                segment.Append(c);
            }
        }
        list.Add(segment.ToString());

        segs = list.ToArray();
        _pathCache[path] = segs;
        return segs;
    }

    //──────────────────────────────────────────────────────────────
    // 内部：深度遍历
    //──────────────────────────────────────────────────────────────

    /// <summary>
    /// 按已拆分的路径段递归向下遍历 <paramref name="current"/> 变量，
    /// 用于 <see cref="GetValueByPath"/>／<see cref="GetValuesByPaths"/> 的内部实现。  
    /// </summary>
    /// <param name="current">
    /// 当前遍历到的 <see cref="Godot.Variant"/>（初始为 JSON 根节点）。
    /// </param>
    /// <param name="segments">
    /// 由路径拆分得到的字符串数组。
    /// </param>
    /// <param name="debug">
    /// 调试标志；同外层接口。
    /// </param>
    /// <param name="success">
    /// <c>out</c> 参数；若整个路径成功解析到目标节点则为 <c>true</c>，
    /// 否则为 <c>false</c>。
    /// </param>
    /// <returns>
    /// 解析得到的 <see cref="Godot.Variant"/>；  
    /// 当 <paramref name="success"/> 为 <c>false</c> 时，返回 <c>Variant.Nil</c>。
    /// </returns>
    private Variant GetValueBySegments(
        Variant current,
        string[] segments,
        bool debug,
        out bool success)
    {
        foreach (string key in segments)
        {
            switch (current.VariantType)
            {
                case Variant.Type.Dictionary:
                    var dict = current.AsGodotDictionary();
                    if (!dict.ContainsKey(key))
                    {
                        if (debug) GD.PrintErr($"[JsonHelper] 字典缺少键 '{key}'");
                        success = false;
                        return default;
                    }
                    current = dict[key];
                    break;

                case Variant.Type.Array:
                    var arr = current.AsGodotArray();
                    if (!int.TryParse(key, out int idx))
                    {
                        if (debug) GD.PrintErr($"[JsonHelper] 数组索引无效 '{key}'");
                        success = false;
                        return default;
                    }
                    if (idx < 0) idx += arr.Count; // 允许负索引
                    if (idx < 0 || idx >= arr.Count)
                    {
                        if (debug) GD.PrintErr($"[JsonHelper] 数组索引越界 '{key}'");
                        success = false;
                        return default;
                    }
                    current = arr[idx];
                    break;

                default:
                    if (debug)
                        GD.PrintErr($"[JsonHelper] 无法继续访问 '{key}'，当前类型 = {current.VariantType}");
                    success = false;
                    return default;
            }
        }

        success = true;
        return current;  // 可能是 Variant.Nil（对应 JSON null）
    }
}
