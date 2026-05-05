# 游戏导表

表格为通用 excel 格式 .xlsx，导出数据为一份 MessagePack 格式的二进制和一份供比对信息的 json，内建默认一共有三类表格式提供选择：**普通、本地化、全局**。

导表工具使用了线程池加速，如果要做自定义扩展请注意线程安全。

## 内建模板

### 类型分析

#### 基础类型

导表工具提供以下内建基础类型，这里枚举出来并附上与 C# 类型的关系：

```
Bool, // bool

Int8, // sbyte
Int16, // short
Int32, // int
Int64, // long

UInt8, // byte
UInt16, // ushort
UInt32, // uint
UInt64, // ulong

Float32, // float
Float64, // double
String, // string
```

下面罗列出各个字符串可以被解析为的对应基础类型：

```csharp
["bool"] = new TypeDefaultSetting(TypeInfo.EBaseType.Bool, TypeInfo.ETypeFlag.None),
["boolean"] = new TypeDefaultSetting(TypeInfo.EBaseType.Bool, TypeInfo.ETypeFlag.None),

["int8"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int8, TypeInfo.ETypeFlag.None),
["sbyte"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int8, TypeInfo.ETypeFlag.None),
["uint8"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt8, TypeInfo.ETypeFlag.None),
["byte"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt8, TypeInfo.ETypeFlag.None),

["int16"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int16, TypeInfo.ETypeFlag.None),
["short"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int16, TypeInfo.ETypeFlag.None),
["uint16"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt16, TypeInfo.ETypeFlag.None),
["ushort"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt16, TypeInfo.ETypeFlag.None),

["int32"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int32, TypeInfo.ETypeFlag.None),
["int"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int32, TypeInfo.ETypeFlag.None),
["uint32"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt32, TypeInfo.ETypeFlag.None),
["uint"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt32, TypeInfo.ETypeFlag.None),

["int64"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int64, TypeInfo.ETypeFlag.None),
["long"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int64, TypeInfo.ETypeFlag.None),
["uint64"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt64, TypeInfo.ETypeFlag.None),
["ulong"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt64, TypeInfo.ETypeFlag.None),

["float32"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float32, TypeInfo.ETypeFlag.None),
["float"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float32, TypeInfo.ETypeFlag.None),
["float64"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float64, TypeInfo.ETypeFlag.None),
["double"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float64, TypeInfo.ETypeFlag.None),

["link"] = new TypeDefaultSetting(TypeInfo.EBaseType.Link, TypeInfo.ETypeFlag.None),

["string"] = new TypeDefaultSetting(TypeInfo.EBaseType.String, TypeInfo.ETypeFlag.Nullable),
["str"] = new TypeDefaultSetting(TypeInfo.EBaseType.String, TypeInfo.ETypeFlag.Nullable),

["text"] = new TypeDefaultSetting(TypeInfo.EBaseType.Text, TypeInfo.ETypeFlag.Nullable),
```

#### text 类型 (本地化 key)

`text` 是一种特殊的字符串基础类型, 字面值形如 `[TABLE,KEY]`,
其中 `TABLE` 是本地化表名 (与本地化 xlsx 中 `<...>` 捕获的表名一致),
`KEY` 是 i18n 表中的 key 字段值. **TABLE / KEY 都可以使用 unicode 字符**
(字母 / 数字 / 下划线 / 横杠, 不可以数字开头), 中间允许空白.

例子:
- `[Common, PROJECT_NAME]`
- `[漫画对话, KEY_001]`
- `[Common, 项目-名称]`

text 字段在导出阶段按字符串原样落盘, 在校验阶段会:

1. 检查格式是否合法 (`[TABLE,KEY]`).
2. 检查 `TABLE` 是否存在于本地化表中.
3. 检查 `KEY` 是否在 `TABLE` 中已登记 (任一语言登记过即视为存在).



#### 复合类型

复合类型是由多个基础类型和名称结合起来的结构体，它的定义方式如下：

```
<baseType>: 基础类型
fieldName: 字段名
flag: 类型标签

{<baseType>flag fieldName, <baseType>flag fieldName, ...}flag

例子:
{int a, string! string, double c, bool d, int8? byte}
{byte b, str name}?
{int c, bool bool}

要求：
类型名称不能重复，复合类型最多支持 15 个字段，最少要包含两个字段
字段名必须是合法的程序标识符且禁止包含 `_`
```



#### 容器类型

导表工具提供了字典、数组两类容器类型，它们的定义方案如下：

```
<type>: 类型（包含复合类型和基础类型）
<baseType>: 基础类型
{flag}: 可写可不写

列表: <type>{flag}[]{flag}
字典: <type>{flag}[{baseType}]{flag} // 字典的 key 只能是基础类型并且不允许任何 flag

当 {flag} 放在容器类型定义的最末端时标识容器本身的特性, {flag} 直接放在容器内元素类型定义后时标识元素类型的特性
```



#### 类型标签

当前内建的类型标签有两个：! 和 ?，分别标识类型的可空性（! 为不可空, ? 为可空），可以改变导表时面对空字符串所取的默认值。

#### 类型注解 `<...>`

类型字符串可以在 fields 块和 flag 之间附加一段尖括号注解, 用半角逗号分隔多个项,
每一项要么是单纯的 tag (如 `<asset>`), 要么是 `key=value` (如 `<index=World>`).
注解整体在导表数据流中**不会**作为字段的值落盘, 而是参与校验, 并通过生成代码
`CfgGenMeta.g.cs` 暴露给运行时使用.

```
<type>: 类型 (基础类型或复合类型)
<attribute>: 注解, 形如 <key1=value1, key2, key3=value3>

完整文法 (注解出现在 fields 之后, 类型 flag 之前):
    <type>{<attribute>}{flag}{[ {keyType} ]{flag}}

例子:
    text                          // 本地化 key
    text<i18n>                    // 同上, i18n 仅作语义提示, 当前不被识别 -> 会输出 warning
    int<index=World>              // 字段值为 World 表的主键
    int<index=World>[]            // 数组, 每一项都是 World 表的主键
    string<asset>                 // 字段值是引擎能解析的资产路径 (如 res://...)
    string<desc="包含, 和=的备注"> // 引号里允许任意 unicode 字符
```

**注解里的字符**:

- 裸 key/value 允许 unicode 字母、数字、下划线、横杠、点 等任意非分隔符 (`,` `=` `>`
  `"` 除外).
- `key=value` 中的 value 可以用半 / 全角双引号 `"..."` 包裹, 内部可放任意字符
  (含 `,` `=` `>`); 用反斜杠 `\` 转义引号自身和反斜杠.
- 同名 key 后者覆盖前者.

**当前内建支持的注解 key**:

| key      | value          | 含义                                           |
| -------- | -------------- | ---------------------------------------------- |
| `index`  | 目标普通表名   | 字段值必须是目标表的主键 (Id) 之一             |
| `asset`  | (无)           | 字段值是工程内可解析的资产路径, 由外部裁定存在 |

未识别的 key 会在校验阶段输出 warning, 不阻止导表.

**运行时访问**: 导表后会生成 `CfgGenMeta.bytes` (二进制) 与 `CfgGenMeta.g.cs` (反序列化器代码).
框架在 `ConfigModule.InitProcedure` 中按需加载, 数据保存到 `Game.Config.Main.CfgMeta`,
**与配置表三大件 (Common/Global/I18n) 一样支持热刷**. 业务代码可通过:

```csharp
using Game.Config.Main;

if (CfgMeta.TryGetCommonAttr("World", "path", "asset", out var v))
    // 字段被标注为 asset
```

或直接拿到注解 map: `CfgMeta.GetCommonFieldAttrs("World", "path")` /
`CfgMeta.GetGlobalFieldAttrs("Common", "ProjectName")`.

### 普通表

普通表的实现见 `Builtin/CommonTable.cs`，它主要表达为如下结构：

- 普通表的 xlsx 文件名禁止以 i18n- 或 g- 开头

- 前四行为表头：
  - 第一行：注释，可以任意填写
  - 第二行：字段类型
  - 第三行：字段导出 flag，默认设定若包含 `client` 则导出
  - 第四行：字段名，字段名必须为合法的程序标识符，并且禁止以 `__` 开头
- 余下行为表的正文，填写数据
- ※※ 一个普通表一定要有存在 `Id` 字段，并且字段类型必须为 string 或数值型，这个字段将会导出为该表的主键。
- ※※ 一个配置表以 worksheet 为单位，对 worksheet 名的要求：worksheet 名应该形如 `随便写点啥<CfgTableName>`，在尖括号内的会被 match 为配置表名（因为 worksheet 名有长度限制，该处可将配置表名填为空：`<>`，为空的情况在下一项补充说明），配置表名也禁止以 `__` 开头。若配置表名相同，则会合并两张表，若在合并的过程中出现了 “同一字段名有不同的字段类型” 的情况，则会抛出异常。
- ※※ 当 worksheet 名为形如 `随便写点啥<>`格式时，导表工具会到配置表的第 [0, 0] 单元格寻找形如 `<validName>` 的表名，实例如下**（该规则只有普通表有）**：

| 表头`<CfgTabelName>` | A                              |              | B                   | C        |
| -------------------- | ------------------------------ | ------------ | ------------------- | -------- |
| int                  | {int a, double b, string c}?[] | int[string]? | string?             |          |
| client               | client                         |              | client              | client   |
| Id                   | content1                       |              | content2            | content3 |
| 17                   |                                |              |                     |          |
| 18                   |                                |              | aaa:22\|bbb:33      |          |
| 19                   |                                |              |                     |          |
| 20                   | 1, 2, aaa\|2 , 4.5, bb         |              | add                 |          |
| 21                   |                                |              |                     |          |
| 22                   |                                |              |                     |          |
| 23                   |                                |              |                     |          |
| 24                   |                                |              |                     | minu     |
| 25                   |                                | xx           |                     | sub      |
| 26                   |                                |              | s1:56\|s2:9\|s1:009 |          |
| 27                   |                                |              |                     |          |
| 28                   |                                |              |                     | neg      |
| 29                   |                                |              |                     |          |
| 30                   |                                |              |                     |          |
| 31                   |                                |              |                     |          |

表的数据解析：

- 对于基础类型，就是简单的字面解析。特殊地，对于 bool 值，可以使用 true/false 也可以使用 1/0。

- 对于复合类型：

```
按照字段定义顺序解析, 每个字段的数据由逗号隔开, 想取默认值的数据可以直接省略:
{int a, double b, string c, bool d, float? e}?

`1, 2.2, test \nstr, false, 0.234` => {1,2.2,`test \nstr`,false,0.234f} // 特殊地, string 会被 trim 掉开头和末尾
`1,,"  ss", 0,` => {1,0.0,`  ss`,false,null} // 如果想输入带有前缀空格的字符串, 可以用半角双引号将字符串括起来
`,2,\" x",` => {0,2,`" x"`,false,null} // 如果想输入带前缀半角双引号的字符串可以使用反斜杠进行转义
```

- 对于容器类型：

  - 数组：

    ```
    数组包含多个元素, 元素间用 | 隔开：
    {int? a, string b, float c}?[]:
    `1,str,0.98|2,s,1|2,,4||,s|` => [{1,`str`,0.98f}, {2,`s`,1f}, {2,null,4f}, null, {null,s,0f}]
    
    {int a, double? b}[]!:
    `|||` => [{0,null}, {0,null}, {0,null}, {0, null}]
    `` => []
    
    string[]
    `a\|b|cd` => [`a|b`, `cd`] // 竖线也支持字符转义
    ```

  - 字典：

    ```
    字典在数组的元素上为每个元素增加了 key 的选项，与元素正文用半角冒号隔开，key 可以在元素最前面:
    {int a, double b}[str]
    `4:1,2|x:5,2.2||s: |4:2` => [
        [`4`]: {1, 2},
        [`x`]: {5, 2.2},
        [``]: {0, 0.0}, // 如果没写 key, 则 key 为当前基础类的默认值，特殊地，string 默认值为空串 ``
        [`s`]: {0, 0.0},
    ] // 如果存在，重复的 key，只会保留最前面的那一项
    ```

- 带 id 的普通表：
  - 有时候会想为同一类角色的不同个体定义自己的表：比如龙属于怪物，而一条龙定义一张表，并且这些表的表结构都是同样的。这种情况下就可以使用带 Id 的普通表：将表名定义为 `<CfgNameTable|Loong110101>`，这个例子中，竖线后面的“Loong110101”就是表 Id。在程序中，可以通过表名和这个表 Id 找到对应的二维表，表名决定了普通表在程序中的类型，表 Id 则区分了表的各个实例。表 Id 类型一定为非空 String。在没定义表 Id 时，表 Id 为空串 `""`。



### 本地化表

本地化表形如：

- xlsx 文件名以 i18n- 开头的配置表会被标记为本地化表。
- 本地化表的第一行为表头
  - key 字段必须存在
  - 其余字段都是可选的，其中， zh-cn、zh-hk、en 等皆为对应语言的国际标准地区码
- 本地化表的配置表名捕获与合并原则和普通表一样。

| key            | comment | zh-cn | zh-hk | en    | ru         | ja      | fr    |
| -------------- | ------- | ----- | ----- | ----- | ---------- | ------- | ----- |
| I18N_KEY_TEST1 |         | 测试1 | 測試1 | test1 | Испытания1 | テスト1 | Test1 |
| I18N_KEY_TEST2 |         | 测试2 | 測試2 | test2 | Испытания2 | テスト2 | Test2 |
| I18N_KEY_TEST3 |         | 测试3 | 測試3 | test3 | Испытания3 | テスト3 | Test3 |
| I18N_KEY_TEST4 |         | 测试4 | 測試4 | test4 | Испытания4 | テスト4 | Test4 |

### 全局表

- xlsx 文件名以 g- 开头的配置表会被标记为本地化表。
- 全局表第一行、第一列为表头，仅作注释，无实际要求。
  - 第二列为字段名，要求必须为合法的程序标识符且禁止以 `__` 开头
  - 第三列为导出过滤，默认只导出包含 `client` 的字段
  - 第四列为类型
  - 第五列为值
- 全局表的配置表名捕获与合并原则和普通表一样

| 备注 | 名称   | 导出选项 | 类型                                  | 值                                                       |
| ---- | ------ | -------- | ------------------------------------- | -------------------------------------------------------- |
| 测试 | Idx    | client   | int                                   | 1                                                        |
| 测试 | A      |          | double?                               | 0.2                                                      |
| 测试 | Data_A | client   | {int a, bool b , str  c}              | 16, t, dddd                                              |
| 测试 | Dict   | client   | {double e, str  string, bool check}[] | 0.4,  "大漠孤烟直", false\|0.256, afc\|0.01, c, f\|\|0.2 |
| 测试 | string | client   | int[str]                              | ???: 98\|aaa: 234\|\|xx: 10                              |

## 自定义表

配置表的导表逻辑可以通过继承并实现 TableTypeDec 来扩展。

## 产物文件约定

导表工具一次成功运行后会落盘以下文件:

| 文件 | 编码 | 输出目录 (`ConfImporter` 字段) | 说明 |
| --- | --- | --- | --- |
| `CommonTable.bytes` / `GlobalTable.bytes` / `I18nTable.bytes` / `CfgGenMeta.bytes` | 二进制 (MessagePack) | `ByteOutputTargetDir` | 运行时反序列化使用, 支持热刷 |
| `CommonTable.json` / `GlobalTable.json` / `I18nTable.json` / `CfgGenMeta.json` | UTF-8 无 BOM, 友好缩进 | `JsonOutputTargetDir` (空则回落 `ByteOutputTargetDir`) | 仅供人工 / 工具阅读, 不参与运行时加载 |
| `CfgGenStruct.g.cs` / `CfgGenTable.g.cs` / `CfgGenGlobalTable.g.cs` / `CfgGenMeta.g.cs` | UTF-8 无 BOM | `CodeOutputTargetDir` | 运行时强类型读表代码 / meta 反序列化器 |
| `validation_report.json` | UTF-8 无 BOM | 校验执行方决定 (GUI 默认 `JsonOutputTargetDir`) | 校验产生的 issue 列表与待外部裁定的 asset 候选 |

**所有文本产物一律使用 UTF-8 无 BOM、LF 行尾**, 与项目 `.gitattributes` 默认约定一致.

> JSON 产物**默认建议**单独放到一个 git-ignore 的目录 (例如本仓库的
> `docs/designer/table/__gitignore__json/`), 避免和发布期 `.bytes` 产物混在一起,
> 也避免人工调试用的明文随提交噪声进仓库. 运行时不读 `.json`, 因此只要
> `.bytes` 在 `ByteOutputTargetDir` 落得对就行, JSON 目录可任选.

CommonTable.json 例:

```jsonc
{
  "version": 1,
  "tables": [
    {
      "name": "World",
      "fields": [
        { "name": "Id", "type": "int32", "attribute": "" },
        { "name": "path", "type": "string?", "attribute": "asset" }
      ],
      "rows": [
        { "id": 0, "subId": "",
          "data": { "Id": 0, "path": "res://res/scene/entrance.tscn" } }
      ]
    }
  ]
}
```

## 校验 (Validation)

`ConfImporter.Validate.Validator` 提供了导表后的静态校验入口, 可以直接在外部进程
(例如 ConfImporterGUI) 中调用. 校验需要在 `CollectFiles` + `AnaTable` 之后执行,
不强制依赖 `GenByte` / `GenCode`.

```csharp
var report = ConfImporter.Validate.Validator.Run(importer);
report.WriteJsonTo("./validation_report.json");
```

校验项目:

- **text 类型字段值**: 检查 `[TABLE,KEY]` 字面格式, 并校验 (TABLE, KEY) 已经登记
  在本地化表的某个语言中.
- **`<index=TableName>` 注解**: 字段值需是目标普通表的主键之一 (数字 / 字符串 id 自动归一).
- **`<asset>` 注解**: 字段值非空时收集到 `report.Assets`, 由具备引擎运行时上下文的
  外部消费者 (例如 Godot addon, 或 ConfImporterGUI 自身根据 `--project-root` 做物理
  存在性校验) 裁定.
- **未识别的注解 key**: 输出 Warning, 不阻止导表.

校验报告 (`validation_report.json`) 的 schema:

```jsonc
{
  "errorCount":   0,
  "warningCount": 0,
  "issues": [
    { "severity": "Error", "source": "World[1].path", "category": "asset",
      "message": "..." }
  ],
  "assets": [
    { "source": "World[1].path", "path": "res://res/scene/entrance.tscn" }
  ]
}
```