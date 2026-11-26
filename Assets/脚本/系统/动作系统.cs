using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 动作系统（单例）
/// 负责按流程执行 GameAction，支持：
/// - 前置反应（PreReactions）
/// - 执行期反应（PerformReactions）
/// - 后置反应（PostReactions）
/// - 全局前置/后置回调订阅
/// </summary>
public class 动作系统 : Singleton<动作系统>
{
    // 当前正在处理的一系列反应动作（会在 Flow 的各个阶段被重新赋值）
    private List<GameAction> reactions = null;

    // 是否正在执行某个动作（防止并发执行）
    public bool IsPerforming { get; private set; } = false;

    // 全局前置回调字典：键=动作类型，值=该类型的所有回调委托（接收 GameAction）
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();

    // 全局后置回调字典
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();

    // 【改动1】新增字段：记录当前正在执行的动作
    private GameAction currentAction = null;

    // 动作执行器字典：键=动作类型，值=执行该动作的协程函数
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    // ========== 修复取消订阅的辅助字典 ==========
    // 用于存储原始委托 -> 包装后的委托的映射，以便正确取消订阅
    // 每个条目：原始委托 -> 包装委托
    private static Dictionary<Delegate, Action<GameAction>> preWrapperMap = new();
    private static Dictionary<Delegate, Action<GameAction>> postWrapperMap = new();

    /// <summary>
    /// 开始执行一个动作（外部入口）
    /// </summary>
    /// <param name="action">要执行的动作实例</param>
    /// <param name="OnPerformFinished">动作整体执行完成后的回调（可选）</param>
    public void Perform(GameAction action, Action OnPerformFinished = null)
    {
        if (IsPerforming) return;          // 已有动作在执行，忽略新请求
        IsPerforming = true;
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    /// <summary>
    /// 核心协程：按阶段处理一个动作（前置 → 执行 → 后置）
    /// </summary>
    /// <param name="action">当前动作</param>
    /// <param name="OnFlowFinished">该动作完全处理完成后的回调</param>
    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        // 【改动3】在协程开始时记录当前动作
        currentAction = action;

        // ---------- 1. 前置阶段 ----------
        // 获取前置反应列表，若为 null 则替换为空列表（防止后续遍历异常）
        reactions = action.PreReactions ?? new List<GameAction>();
        // 触发全局前置订阅回调（同步）
        PerformSubscribers(action, preSubs);
        // 执行前置反应动作（每个反应会递归调用 Flow，等待全部完成）
        yield return PerformReactions();

        // ---------- 2. 执行阶段 ----------
        reactions = action.PerformReactions ?? new List<GameAction>();
        // 执行该动作自身的表演逻辑（协程）
        yield return PerformPerformer(action);
        // 执行动作中定义的反应（如同时播放特效等）
        yield return PerformReactions();

        // ---------- 3. 后置阶段 ----------
        reactions = action.PostReactions ?? new List<GameAction>();
        // 触发全局后置订阅回调
        PerformSubscribers(action, postSubs);
        // 执行后置反应动作
        yield return PerformReactions();

        // 【改动3续】协程结束时清除记录
        currentAction = null;

        // 全部完成，调用回调
        OnFlowFinished?.Invoke();
    }

    /// <summary>
    /// 执行动作自身的表演者（核心逻辑）
    /// </summary>
    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if (performers.TryGetValue(type, out var performer))
        {
            yield return performer(action);
        }
        else
        {
            // 如果没有注册执行器，给出警告（但不中断流程）
            Debug.LogWarning($"动作 {type.Name} 没有注册执行器 (performer)，将跳过执行。");
        }
    }

    /// <summary>
    /// 触发给定字典中匹配该动作类型的所有回调（同步）
    /// </summary>
    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.TryGetValue(type, out var list))
        {
            foreach (var sub in list)
            {
                sub(action);
            }
        }
    }

    /// <summary>
    /// 执行当前 reactions 列表中的所有反应动作（依次递归）
    /// </summary>
    private IEnumerator PerformReactions()
    {
        // reactions 在调用前已经被赋值为非 null 列表，但再次检查更安全
        if (reactions == null) yield break;
        foreach (var reaction in reactions)
        {
            yield return Flow(reaction);   // 每个反应动作也是完整的动作流程
        }
    }

    // ========== 以下为公开的静态注册/注销方法 ==========

    /// <summary>
    /// 为某种动作类型注册执行器（表演者）
    /// </summary>
    /// <typeparam name="T">动作的具体类型（必须继承 GameAction）</typeparam>
    /// <param name="performer">执行该动作的协程函数，参数为 T 类型动作</param>
    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        // 包装为统一的 Func<GameAction, IEnumerator>
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        performers[type] = wrappedPerformer; // 如果已存在则覆盖
    }

    /// <summary>
    /// 移除某种动作类型的执行器
    /// </summary>
    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        performers.Remove(type);
    }

    /// <summary>
    /// 订阅全局反应（前置或后置）
    /// </summary>
    /// <typeparam name="T">动作类型</typeparam>
    /// <param name="reaction">回调委托，接收具体类型的动作</param>
    /// <param name="timing">前置还是后置</param>
    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        // 包装：将 Action<T> 转换为 Action<GameAction>
        void wrappedReaction(GameAction action) => reaction((T)action);

        // 根据时机选择目标字典和映射字典
        var subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        var wrapperMap = timing == ReactionTiming.PRE ? preWrapperMap : postWrapperMap;

        Type type = typeof(T);
        if (!subs.ContainsKey(type))
            subs[type] = new List<Action<GameAction>>();

        subs[type].Add(wrappedReaction);
        // 保存原始委托与包装委托的映射，以便后续取消订阅时能正确移除
        wrapperMap[reaction] = wrappedReaction;
    }

    /// <summary>
    /// 取消订阅全局反应
    /// </summary>
    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        var subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        var wrapperMap = timing == ReactionTiming.PRE ? preWrapperMap : postWrapperMap;

        Type type = typeof(T);
        if (!subs.ContainsKey(type)) return;

        // 通过映射字典找到当初包装过的委托
        if (wrapperMap.TryGetValue(reaction, out var wrapped))
        {
            subs[type].Remove(wrapped);
            wrapperMap.Remove(reaction);
        }
        // 如果列表变空，可以选择删除该类型的键（可选）
        if (subs[type].Count == 0)
            subs.Remove(type);
    }

    // ========== 注意：原代码中的 AddReaction 方法未使用且有逻辑问题，已删除 ==========
    // 如果确实需要在运行时向当前 reactions 列表添加动作，建议重新设计或改用其他方式。
    public void AddReaction(GameAction gameAction)
    {
        if (currentAction != null)
        {
            // 如果后置反应列表为空，则创建
            currentAction.PostReactions.Add(gameAction);
        }
        else
        {
            Debug.LogWarning("当前没有正在执行的动作，无法添加反应。请确保在动作执行过程中调用 AddReaction。");
        }
    }
}
