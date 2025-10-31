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
```



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
- ※※ 一个配置表以 worksheet 为单位，对 worksheet 名的要求：worksheet 名应该形如 `随便写点啥<CfgTableName>`，在尖括号内的会被 match 为配置表名（因为 worksheet 名有长度限制，该处可将配置表名填为星号：`<*>`，为星号的情况在下一项补充说明），配置表名也禁止以 `__` 开头。若配置表名相同，则会合并两张表，若在合并的过程中出现了 “同一字段名有不同的字段类型” 的情况，则会抛出异常。
- ※※ 当 worksheet 名为形如 `随便写点啥<*>`格式时，导表工具会到配置表的第 [0, 0] 单元格寻找形如 `<validName>` 的表名，实例如下**（该规则只有普通表有）**：

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