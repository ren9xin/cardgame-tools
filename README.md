# CardGame · 回合制卡牌战斗原型

一个用于验证**卡牌战斗架构**的回合制原型：把「出牌 → 结算 → 敌方回合」拆成可插拔的动作序列，卡牌、技能、效果全部数据驱动，新增一张卡不需要改战斗逻辑。

项目重点不在美术与关卡，而在**动作系统（GameAction / Reaction）与数据分层**的设计——这也是后续卡牌编辑器工具链的宿主工程。

Unity（团结引擎）实现，C# 脚本 62 个 / 1641 行，自研代码按 `系统 / 游戏动作 / 模型 / 视图 / UI / 数据` 六层组织。

![战斗界面](docs/screenshots/battle.png)
<!-- 截图待补充：战斗主界面 / 出牌选目标 / 技能触发 -->

## 玩法

单场回合制卡牌战斗：

1. **开局装配**：`MatchSetupSystem` 初始化双方，`HeroSystem` 建英雄，`EnemySystem` 建敌人视图
2. **抽牌**：`DrawCardGA` / `DrawCardsGA`，牌入 `手牌视图`
3. **出牌**：`PlayCardGA` 触发 `SpendManaGA` 扣除法力
4. **选目标**：需要指向的卡进入 `ManualTargetSystem`，用 `ArrowView` 拉箭头选敌
5. **结算**：`动作系统` 执行动作队列，伤害走 `DamageSystem`，增益走 `EffectSystem`
6. **敌方回合**：`EnemyTurnGA` → `AttackHeroGA`，结束回到己方并 `RefillManaGA` 回蓝
7. **手牌循环**：回合结束 `DiscardAllCardsGA` 清空手牌，进入弃牌堆

内置 4 张卡（剑雨、抽卡、格挡、火球）、2 种敌人（普通史莱姆、红色史莱姆）、1 个英雄、1 个技能（反击），足以跑通完整循环。

## 技术栈

| 项 | 版本 / 说明 |
| --- | --- |
| 引擎 | 团结引擎 Tuanjie 1.10.4（Editor 2022.3.62t16） |
| 渲染管线 | Built-in，3D 卡牌 + UI 混合 |
| 语言 | C# |
| UI | uGUI + TextMeshPro 3.0.10 |
| 动画 | DOTween、Cartoon FX Remaster（JMO Assets） |
| 数据结构 | Unity Collections 2.5.8 / Burst 1.8.30 |

## 运行方式

1. 用团结引擎 1.10.4（或 Unity 2022.3.62f 系列）打开工程
2. 打开 `Assets/Scenes/SampleScene.scene`，按 Play

> **本项目未配置 Build Settings 场景列表，必须手动打开 `SampleScene` 才能进 Play。**

> **依赖提示**：`Packages/manifest.json` 中 `cn.tuanjie.ai.generators` 指向 `file:../.codely-cli/...`，该路径在仓库之外。在其它机器 clone 后打开工程，Unity 会报包解析失败。处理方式：把该条依赖从 `manifest.json` 中删除，或在工程同级目录建 `.codely-cli` 目录后重新导入。其余依赖均可正常解析。

## 架构设计

这是本项目最值得看的部分。整个战斗流程由「动作（GameAction）+ 反应（Reaction）」两层构成。

### 动作系统

`Assets/脚本/系统/动作系统.cs`（226 行）是战斗执行的中枢。

- 内部维护 `Dictionary<Type, Action<GameAction>>` 作为**派发方法表**，每种具体 `GameAction` 类型对应一个执行方法
- 另维护 `Dictionary<Type, Delegate>` 作为**泛型注册表**，使订阅与退订可以成对出现，不会因闭包新建而漏退订
- 执行时把当前动作的 reactions 复制到订阅表，挂到 `currentAction.PostReactions` 上，动作结束后统一触发

```csharp
// 派发：按具体类型找执行方法
_performMethods.TryGetValue(action.GetType(), out var perform);
perform?.Invoke(action);

// 订阅：包装一层以便持有引用、支持退订
public void AddReaction<T>(Action<T> reaction) where T : GameAction
```

这套设计让「出牌」本身不再是一个大函数，而是一串可组合的动作：`PlayCardGA` → `SpendManaGA` → `DealDamageGA` → `PerformEffectGA`，每个动作只做一件事，失败可单点排查，新增动作只需注册类型与执行方法。

### 反应时机

`Assets/脚本/General/ReactionTiming.cs` 只有两个枚举值：`PRE` / `POST`。

技能通过 `AddReaction` 挂到指定动作的指定时机上，因此「敌人攻击时触发反击」这类逻辑可以完全独立于敌人代码写出来：

```csharp
// 技能条件判断与触发分离
OnEnemyAttackCondition  →  命中时满足  →  触发反击效果
```

`PerkData` 只描述数据，`PerkCondition` 只描述条件，反应挂在动作系统上——三者互不引用，是这套架构最大的收益。

### 效果链组合

`Effect` 系列使用 `[SerializeReference]` 声明，允许在 Inspector 里把多个效果对象组合成链，运行时按顺序执行。`AutoTargetEffect`、`ArmorUp`、`ArrowRain` 等都是可复用的效果单元，新建技能时在 Inspector 里拖拽拼装即可。

### 数据驱动

4 类 ScriptableObject 定义 + 8 个数据资产：

| SO 类型 | 用途 | 资产 |
| --- | --- | --- |
| `CardData` | 卡牌：名称、费用、效果链、目标模式 | 剑雨、抽卡、格挡、火球 |
| `HeroData` | 英雄：血量、初始牌组 | 默认英雄 |
| `EnemyData` | 敌人：血量、行动模式 | 普通史莱姆、红色史莱姆 |
| `PerkData` | 技能：触发条件、反应时机、效果 | 反击 |

### 目标选择策略化

`TargetMode` 作为基类，派生出 `AllEnemiesTM`、`RandomEnemyTM`、`NoTM` 三种策略。卡牌数据里声明用哪种模式，出牌时由 `ManualTargetSystem` 判断是否需要玩家手动拉箭头，逻辑与表现都无需针对具体卡牌写分支。

### 分层结构

```
脚本/
├── 系统/        战斗核心：动作系统、卡牌系统、伤害/法力/效果系统、回合
├── 游戏动作/    12 个原子动作，每个只做一件事
├── 模型/        Card / Effect / PerkCondition / TargetMode 等运行时模型
├── 视图/        卡片视图、手牌视图、战斗员视图、选敌箭头
├── 创建器/      视图实例化工厂
├── UI/          法力条、技能按钮、回合结束按钮
├── 数据/        4 类 SO 定义与资产
└── 目标模式/    AllEnemies / RandomEnemy / No
```

依赖方向是单向的：`视图 → 模型 ← 系统 ← 游戏动作`。视图不认识系统，系统不认识具体卡牌。

## 已知问题

- **Build Settings 场景列表为空**（`m_Scenes: []`），直接 Build 会得到空场景列表，需手动添加 `SampleScene`。
- **`cn.tuanjie.ai.generators` 依赖指向仓库外层**，异地 clone 后包解析失败，处理方式见上文「运行方式」。
- `ProjectSettings` 中 `productName` 仍为默认的 `My project`，`applicationIdentifier` 为空，打包前需自行设置。
- 仓库内含第三方资源：`JMO Assets/Cartoon FX Remaster`（付费特效包）、`DOTween`、`SREditor`、`Samples`（Timeline 官方示例）。这些**不是本项目的自研代码**，若要商用需自行确认授权。
- `Assets` 内 `.unity` 场景共 3 个，其中 2 个属第三方示例（`CFXRF Demo.unity`、`SRDemo.unity`），注意不要误当成主场景。
- 无存档与账号系统，战斗进度仅存于运行时内存。
- 卡牌数据靠 Inspector 手填，尚无批量导入工具。

## 后续计划

本项目定位为工具链宿主，后续重心在编辑器侧：

- [ ] **配置表管线**：Excel / CSV → ScriptableObject 批量生成器
- [ ] **数据校验器**：字段类型、ID 重复、引用完整性、数值区间检查
- [ ] **编辑器窗口**：卡牌一览、批量编辑、效果链可视化编辑
- [ ] `AssetPostprocessor` 拦截导入，配置表变更自动重建资产
- [ ] 补充 `CustomEditor` / `PropertyDrawer`，优化 SO 编辑体验
- [ ] 完善 Build Settings 与打包脚本（`BuildPipeline`）
- [ ] 补齐 DOTween 驱动的卡牌动画状态机
- [ ] 扩充卡牌与敌人数据，验证架构在数据量上升后的表现

## 开发分工

> 待补充。