# LitheDto + HereinNotify

## 面向 MVVM 的 DTO & 属性代码生成解决方案

在 WPF / Avalonia / MAUI 等 MVVM 项目中，常见痛点包括：

- 实体（Domain / Model） ≠ DTO ≠ ViewModel
- DTO 手写成本高、字段重复严重
- MVVM Toolkit 仅覆盖 ViewModel 层
- 属性通知、验证、状态追踪分散，维护困难

**LitheDto + HereinNotify** 是一套基于 **Source Generator** 的编译期代码生成工具，用于统一解决 **DTO 合并、实体映射、属性通知与 MVVM 扩展能力**。


> 一个同时覆盖 **DTO + ViewModel + 通知模型** 的 MVVM 基础设施，
> 用来补齐 MVVM Toolkit 无法解决的那一半问题。

---

## 演示

### 1. 多实体合并生成 DTO（LitheDto）

```csharp
[LitheDto(Source = typeof(TestObject1))]
[LitheDto(Source = typeof(TestObject2), IsUseINPC = true)]
internal partial class TestDto
{
    partial void OnIdChanged(int oldValue, int newValue);
}
```

自动生成内容：

- 多 Source 实体字段合并为一个 DTO
- DTO 属性（可选 INotifyPropertyChanged）
- `InputXxx / OutputXxx / WriteXxx` 映射方法
- 强类型属性变更钩子

特点：

- 无反射
- 无运行时映射
- 生成代码完全可读、可调试

---

### 2. DTO 职责边界清晰

生成的 DTO 仅保留：

- 基础属性
- 可选通知行为

不包含：

- 赋值验证
- 拦截逻辑
- 业务规则

```csharp
public int Id
{
    get => _Id;
    set
    {
        var old = _Id;
        SetProperty(ref _Id, value);
        OnIdChanged(old, value);
    }
}
```

非常适合作为：

- 列表展示模型
- API / 序列化模型
- UI 中间态数据

---

### 3. 字段 → 属性 的高级生成能力（HereinNotify）

```csharp
internal partial class TestModel : HereinNotifyObject
{
    [HereinNotifyProperty(IsVerify = true)]
    [HereinNotifyProperty(IsChanged = true)]
    private int _id ;
}
```

自动生成：

- 标准属性 + 通知
- 赋值验证拦截
- 变更状态追踪（具备通知行为的XxxIsChanged属性）
- 生命周期分部方法

```csharp
partial void VerifyIdSetter(ref bool isAllow, int newValue);
partial void OnIdVerifyFail(int value);
partial void OnIdChanged(int oldValue, int newValue);
```

相较 MVVM Toolkit 的 `[ObservableProperty]`：

- 能力更完整
- 扩展点更清晰
- 行为全部显式可控

---

### 4. 属性级扩展能力（MVVM Toolkit 不具备）

#### 多特性叠加（ JSON / UI / 自定义特性）

```csharp
[HereinNotifyProperty(Attr = typeof(TestAttribute), AttrParmas = "\"test_model\"")]
[HereinNotifyProperty(Attr = typeof(JsonPropertyNameAttribute), AttrParmas = "\"JSON_KEY\"")]
private int _id;

// 如果引用了其它命名空间的Attribute
// 当前需要在当前类上标记需要引入的命名空间（后续将改进减少这部分代码）
// [HereinUsing("System.Text.Json.Serialization")]
```


## 与 MVVM Toolkit 的核心差异

| 能力项 | MVVM Toolkit | LitheDto + HereinNotify |
|------|-------------|--------------------------|
| ViewModel 属性生成 | ✅ | ✅ |
| DTO 自动生成 | ❌ | ✅ |
| 多实体合并 DTO | ❌ | ✅ |
| 实体 ↔ DTO 映射 | ❌ | ✅ |
| 赋值验证 | ❌ | ✅ |
| 变更状态追踪 | ❌ | ✅ |
| 属性特性注入 | ❌ | ✅ |

---

## 典型使用场景

- WPF / Avalonia / MAUI 项目
- DTO / 实体字段高度重复的系统
- 需要严格区分：
  - Domain Model
  - DTO
  - ViewModel
- 不希望引入 AutoMapper 或运行时反射
- 偏好编译期可见、可调试代码

---

## 设计理念

- 声明式，而非侵入式
- 编译期生成，而非运行时魔法
- DTO 轻量，ViewModel 强能力
- 所有扩展点通过 partial 暴露


如果你当前：

- 正在使用 MVVM Toolkit
- DTO 仍依赖手写或复制粘贴
- 或被 AutoMapper 的隐式行为困扰

那么 **LitheDto + HereinNotify** 是一个更加工程化、可控、可扩展的选择。

它不是“另一个 MVVM 工具”，
而是 **MVVM 项目中长期缺失的 DTO 与通知基础设施**。

