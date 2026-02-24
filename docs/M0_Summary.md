# M0阶段实现摘要

本阶段完成了并行约束先行的基础框架，确保单线程执行但具备未来多核迁移的接口形态与确定性基线。

## M0完成内容

1. 数据驱动索引与稳定ID
   - 按 `Code` 字典序生成稳定ID，确保确定性与可复现
   - 形成 `Code -> ID` 与 `ID -> Code` 映射

2. SoA数据布局与局部缓冲
   - 使用扁平数组布局，`index = provinceId * commodityCount + commodityId`
   - Phase内只写局部缓冲，禁止直接写全局状态

3. Tick流水线骨架
   - 完整9个Phase的接口与调度骨架
   - 单线程执行，但保留阶段内并行的结构形态

4. 全球市场外参雏形
   - 外部JSON提供商品参数
   - tick级噪声，Box-Muller正态分布，裁剪±2%
   - 价格限制在 `0.5x ~ 2.0x` 基准范围

5. 确定性快照
   - 每Tick对关键数组生成哈希
   - 用于未来单线程与多线程结果一致性对比

## 代码模块说明

### Core/Ids
- `IdMapping`
  - 负责按 `Code` 字典序生成稳定ID，并提供映射表
- `CommodityCatalog`
  - 商品定义集，基于 `IdMapping` 输出有序商品列表

### Core/Random
- `DeterministicRandom`
  - 可复现的伪随机数生成器
  - 提供 `NextNormal`，使用 Box-Muller 生成正态分布噪声

### Core/Market
- `GlobalMarketConfig`
  - 外部JSON配置结构（seed + 商品列表）
- `GlobalMarketLoader`
  - 读取JSON并构建 `CommodityCatalog` 与 `GlobalMarketService`
- `GlobalMarketService`
  - 按tick更新全球市场价格，包含周期项 + 噪声

### Simulation/State
- `SimulationState`
  - 全局状态容器（生产、需求、全球价格）
  - 提供扁平数组访问索引
- `LocalBuffers`
  - 每Tick的局部缓冲（为未来并行预留）

### Simulation/Determinism
- `DeterminismHasher`
  - FNV风格哈希，用于快照生成
- `StateSnapshot`
  - 记录Tick与对应哈希

### Simulation
- `PhaseId`
  - 定义9个Tick阶段
- `PhaseContext`
  - 传递Tick、全局状态与局部缓冲
- `IPhase`
  - Phase接口
- `SimulationKernel`
  - Phase调度与Tick执行，负责快照记录

### Simulation/Phases
- `GlobalMarketPhase`
  - 在Phase4更新全球市场价格并写入状态
- `NoOpPhase`
  - 其他阶段的占位实现
- `PhaseSchedule`
  - 默认9阶段调度顺序

### Data
- `global_market.json`
  - 外部全球市场参数样例

## Phase输入/输出规范（M0版本）

本规范定义每个Phase的输入/输出边界，强调“只写局部缓冲、归并写全局状态”的并行友好约束。

### 全局数据容器
- `SimulationState`
  - `Production[province, commodity]`
  - `Demand[province, commodity]`
  - `GlobalMarketPrices[commodity]`

### 局部缓冲
- `LocalBuffers`
  - `Production[province, commodity]`
  - `Demand[province, commodity]`

### Phase 1: Province Local Update
- 读取：`GlobalMarketPrices`
- 写入（局部）：`LocalBuffers.Production`, `LocalBuffers.Demand`
- 输出（归并后）：`SimulationState.Production`, `SimulationState.Demand`

### Phase 2: Logistics
- 读取：`SimulationState.Production`, `SimulationState.Demand`
- 写入（局部）：`LocalBuffers.Production`, `LocalBuffers.Demand`
- 输出（归并后）：`SimulationState.Production`, `SimulationState.Demand`

### Phase 3: Aggregation
- 读取：`SimulationState.Production`, `SimulationState.Demand`
- 写入：聚合统计（预留结构，M1实现）

### Phase 4: Market Solve
- 读取：聚合统计 + `GlobalMarketPrices`
- 写入：`SimulationState.GlobalMarketPrices`（目前为外部全球市场输入）

### Phase 5: Income Distribution
- 读取：`SimulationState.Production`, `SimulationState.Demand`
- 写入（局部）：收入/消费缓冲（预留结构，M1实现）

### Phase 6: Political Update
- 读取：收入/通胀/价格状态（预留结构，M1实现）
- 写入（局部）：满意度/稳定度缓冲（预留结构）

### Phase 7: Government
- 读取：财政/通胀/稳定度（预留结构）
- 写入（局部）：政策意图缓冲（预留结构）

### Phase 8: AI Decision
- 读取：上一Tick稳定快照（预留结构）
- 写入（局部）：AI决策意图缓冲（预留结构）

### Phase 9: Decision Apply
- 读取：政策/AI意图缓冲
- 写入：全局状态更新（单线程固定顺序）

## Phase数据结构草案（可执行版本）

本节给出M1可直接落地的数据结构草案，确保阶段内并行友好、边界清晰、可扩展。

### 基础索引

```csharp
public readonly struct ProvinceId
{
    public int Value { get; }
    public ProvinceId(int value) => Value = value;
}

public readonly struct CommodityId
{
    public int Value { get; }
    public CommodityId(int value) => Value = value;
}
```

### 全局状态容器（SoA）

```csharp
public sealed class SimulationState
{
    public int ProvinceCount { get; }
    public int CommodityCount { get; }

    // 主状态（全局）
    public float[] Production { get; }
    public float[] Demand { get; }
    public float[] GlobalMarketPrices { get; }

    // 可扩展：价格、库存、收入、通胀、稳定度等
    public float[] LocalPrices { get; }
    public float[] Inventory { get; }

    public int Index(int provinceId, int commodityId)
        => provinceId * CommodityCount + commodityId;
}
```

### 局部缓冲（并行写入）

```csharp
public sealed class LocalBuffers
{
    public float[] Production { get; }
    public float[] Demand { get; }
    public float[] LocalPrices { get; }

    public void Clear()
    {
        Array.Clear(Production, 0, Production.Length);
        Array.Clear(Demand, 0, Demand.Length);
        Array.Clear(LocalPrices, 0, LocalPrices.Length);
    }
}
```

### 汇总与市场结构

```csharp
public sealed class MarketAggregation
{
    public float[] NationalSupply { get; }
    public float[] NationalDemand { get; }
    public float[] GlobalSupply { get; }
    public float[] GlobalDemand { get; }
}

public sealed class MarketState
{
    public float[] NationalPrices { get; }
    public float[] GlobalPrices { get; }
}
```

### Phase I/O 接口草案

```csharp
public interface IPhase
{
    PhaseId Id { get; }
    void Execute(PhaseContext context);
}

public sealed class PhaseContext
{
    public int Tick { get; }
    public SimulationState State { get; }
    public LocalBuffers Buffers { get; }
    public MarketAggregation Aggregation { get; }
    public MarketState Market { get; }
}
```

### Phase 1: 省份本地计算
- **读取**：`GlobalMarketPrices`, `LocalPrices`, `Inventory`
- **写入（局部）**：`LocalBuffers.Production`, `LocalBuffers.Demand`
- **归并写入**：`SimulationState.Production`, `SimulationState.Demand`

### Phase 2: 物流成本传播
- **读取**：`SimulationState.Production`, `SimulationState.Demand`
- **写入（局部）**：`LocalBuffers.Production`, `LocalBuffers.Demand`
- **归并写入**：`SimulationState.Production`, `SimulationState.Demand`

### Phase 3: 全局归并
- **读取**：`SimulationState.Production`, `SimulationState.Demand`
- **写入**：`MarketAggregation.NationalSupply/Demand`, `MarketAggregation.GlobalSupply/Demand`

### Phase 4: 市场求解
- **读取**：`MarketAggregation` + `GlobalMarketPrices`
- **写入**：`MarketState.NationalPrices`, `MarketState.GlobalPrices`

### Phase 5: 收入分配
- **读取**：`MarketState.NationalPrices`
- **写入（局部）**：收入/消费缓冲（后续结构）

### Phase 6: 政治系统更新
- **读取**：收入、通胀、满意度相关缓冲
- **写入（局部）**：满意度/稳定度缓冲

### Phase 7: 政府行为
- **读取**：财政、通胀、稳定度缓冲
- **写入（局部）**：政策意图缓冲

### Phase 8: AI决策
- **读取**：上一Tick稳定快照
- **写入（局部）**：AI决策意图

### Phase 9: 决策执行
- **读取**：政策/AI意图缓冲
- **写入**：`SimulationState` 全局状态（单线程固定顺序）

## 验证方式

运行测试项目：

```bash
dotnet run --project src/KeynesEngine.Tests
```

输出包括：
- 每Tick的全球市场价格
- 确定性哈希序列
