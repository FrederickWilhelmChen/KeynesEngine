# 现代全球政治经济模拟游戏  
## 总体设计文档（v0.2 架构整合版）

---

## 一、项目目标

构建一个：

- 现代背景（2022起）
- 全球尺度
- 经济深度接近《维多利亚3》
- 单机
- 强AI国家行为
- 高度系统化、非脚本化

当前阶段目标：

构建一个可收敛、可扩展、可并行的经济模拟核心，
作为未来完整游戏的工程基础。

---

## 二、系统总体模块结构

Game.Core
│
├── Economy
│     ├── Production
│     ├── Consumption
│     ├── MarketSolver
│     ├── PriceModel
│
├── Logistics
│     ├── TransportNetwork
│     ├── Shipping / River / Rail / Road
│     ├── CostPropagation
│
├── Population
│     ├── ClassSystem
│     ├── IncomeDistribution
│     ├── Urbanization
│     ├── Stability
│
├── Government
│     ├── PolicySystem
│     ├── FiscalSystem
│     ├── CentralBank
│     ├── Currency
│     ├── Inflation
│
├── AI
│     ├── NationalStrategy
│     ├── EconomicEvaluation
│     ├── PoliticalEvaluation
│
└── SimulationKernel
      ├── TickPipeline
      ├── PhaseBarrier
      ├── ThreadScheduler

---

## 三、核心设计哲学

1. 数据驱动

- 所有参数 JSON 配置
- 商品、产业链、政策均数据化
- 不依赖硬编码逻辑树

2. 强模块隔离

- 经济 ≠ 政治 ≠ AI
- 通过状态接口交互
- 禁止跨模块直接修改

3. 并行友好优先

从第一天设计为：

阶段串行 + 阶段内并行

避免未来出现单核瓶颈。

---

## 四、经济系统结构

### 商品模型

每个商品包含：

- 基础生产率
- 需求弹性
- 储存损耗率
- 运输体积
- 战略价值

价格模型：

Price = BasePrice × f(Supply / Demand)

价格变化必须受限，避免发散。

---

### 市场结构

- 国家级市场
- 可扩展至全球市场
- 汇总供需
- 单线程价格求解
- 固定迭代次数

市场是系统核心瓶颈。

必须：

- 严格确定性
- 禁止并行修改
- 固定归并顺序

---

## 五、物流系统

运输方式：

- 公路
- 铁路
- 内河航运
- 海运

成本特性：

Sea << River << Rail < Road

物流影响：

- 市场有效供给
- 地区价格差
- 产业利润

路径预计算低频执行，
Tick内成本传播可并行。

---

## 六、人口与阶层系统

阶层细分：

- 资本所有者
- 管理阶层
- 专业技术阶层
- 技术工人
- 服务业
- 非熟练工
- 农民
- 失业人口

每阶层属性：

- 收入
- 消费结构
- 政治倾向
- 储蓄率
- 通胀敏感度

---

### 城市化模型

- 工业就业推动城市人口增长
- 城市提升生产效率
- 城市提升消费
- 城市提升政治活跃度

---

## 七、政府与宏观系统

### 财政系统

- 税收
- 支出
- 赤字
- 债务

### 央行系统

- 利率
- 货币供给
- 汇率管理
- 通胀控制

通胀影响：

- 实际收入
- 社会稳定
- 投资回报

---

## 八、社会稳定反馈

通胀上升
→ 实际工资下降
→ 满意度下降
→ 罢工概率上升
→ 生产效率下降

稳定度影响：

- 生产效率
- 投资意愿
- 政策通过率

---

## 九、AI系统设计

- 决策树 + 权重评估
- 不进行完整世界复制模拟
- 不进行深度未来预测
- 读取上一Tick稳定状态

AI可并行：

- 每国家独立线程
- 禁止修改主世界
- 只写决策意图

---

## 十、Tick执行架构

Tick Start

Phase 1: Province Local Update (Parallel)
- 生产
- 消费
- 人口自然增长

Phase 2: 物流成本传播 (Parallel)

Phase 3: 全局归并 (Single Thread)
- 汇总供需

Phase 4: 市场价格求解 (Single Thread)

Phase 5: 收入分配 (Parallel)

Phase 6: 社会稳定更新 (Parallel)

Phase 7: AI决策 (Parallel per Country)

Tick End

---

## 十一、并行设计原则

1. 固定线程池

- 固定线程数
- 固定分块
- 保证确定性

2. 禁止锁

Tick内不得使用 lock。

3. Map-Reduce模式

Province计算 → 线程局部缓存  
单线程统一归并

---

## 十二、数据结构规范

必须使用：

- 连续数组
- 索引访问
- 避免Dictionary
- 避免嵌套对象

推荐：

float production[province][commodity]  
float demand[province][commodity]  
float income[class]

禁止：

Province { Dictionary<Commodity, float> }

---

## 十三、潜在性能瓶颈

模块 | 并行性 | 风险等级
生产 | 高 | 低
物流 | 高 | 中
市场 | 低 | 高
AI | 中 | 高

真正风险在：

- 市场平衡算法
- AI决策复杂度

---

## 十四、MVP阶段目标

阶段1：

- 单线程实现完整经济循环
- 验证价格收敛
- 验证通胀稳定

阶段2：

- 并行省级生产
- 并行收入分配

阶段3：

- 并行AI

---

## 十五、未来扩展路径

- 引入战争系统
- 引入金融市场
- 引入企业微观行为
- 引入全球资本流动

架构已预留接口。

---

## 十六、核心风险控制

1. 禁止全局共享可写状态
2. 禁止Tick内部递归系统调用
3. 禁止浮点随机顺序计算
4. 所有模块必须可单元测试

---

## 十七、工程最终目标

构建一个：

- 可扩展至1000+省份
- 100+商品
- 50+国家
- 多核可扩展
- 确定性稳定

的现代全球政治经济模拟引擎。
