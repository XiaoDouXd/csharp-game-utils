#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;

#region ReSharper disable

// ReSharper disable ArrangeNamespaceBody
// ReSharper disable ConvertToAutoProperty
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

#endregion

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Type
{
    internal static class TypeHelper
    {
        internal static bool IsTypeCanBeId(this TypeInfo.EBaseType type)
        {
            return type is
                TypeInfo.EBaseType.Int8 or
                TypeInfo.EBaseType.Int16 or
                TypeInfo.EBaseType.Int32 or
                TypeInfo.EBaseType.Int64 or
                TypeInfo.EBaseType.UInt8 or
                TypeInfo.EBaseType.UInt16 or
                TypeInfo.EBaseType.UInt32 or
                TypeInfo.EBaseType.UInt64 or
                TypeInfo.EBaseType.String;
        }
    }
    
    internal struct TypeInfo
    {
        /// <summary> 最大字段数 </summary>
        public const int MaxFieldCount = 16;

        /// <summary> 类型标签 </summary>
        [Flags]
        public enum ETypeFlag : byte
        {
            None = 0b00000000,
            Nullable = 0b10000000
        }

        /// <summary> 基础类型 </summary>
        public enum EBaseType : byte
        {
            Null = default,

            Bool,

            Int8,
            Int16,
            Int32,
            Int64,

            UInt8,
            UInt16,
            UInt32,
            UInt64,

            Float32,
            Float64,
            String,

            /// <summary> link 类型 </summary>
            Link,
            /// <summary> 枚举类型 </summary>
            Enum,
            /// <summary> 自定义类型 </summary>
            Custom
        }

        /// <summary> 容器类型 </summary>
        public enum EContainer : byte
        {
            None = default,
            Array,
            Dictionary
        }

        public readonly struct Field : IEquatable<Field>
        {
            public readonly sbyte Order;
            public readonly string Name;
            public readonly EBaseType Type;
            public readonly ETypeFlag Flag;

            public Field(EBaseType type, ETypeFlag flag = ETypeFlag.None)
            {
                Order = -1;
                Type = type;
                Flag = flag;
                Name = string.Empty;
            }

            public Field(sbyte order, string name, EBaseType type, ETypeFlag flag = ETypeFlag.None)
            {
                Name = name;
                Type = type;
                Flag = flag;
                Order = order;
            }

            public override bool Equals(object? obj) => obj is Field other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Name, (int)Type, (int)Flag);
            public bool Equals(Field other) => Name == other.Name && Type == other.Type && Flag == other.Flag;
            public static bool operator ==(in Field a, in Field b) => !(a != b);
            public static bool operator !=(in Field a, in Field b) =>
                a.Type != b.Type && a.Name != b.Name && a.Flag != b.Flag;
        }

        /// <summary> 类型注解 </summary>
        public string Attribute { get; set; }

        /// <summary> 类型别名, 如果是容器, 则返回容器内元素类型的别名 </summary>
        public string Alias => _alias;
        /// <summary> 字段数量 </summary>
        public int FieldCount => _fieldCount;

        /// <summary> 类型标签 </summary>
        public ETypeFlag Flag => _flag;
        /// <summary> 若为容器, 则返回容器索引类型, 否则返回当前类型 </summary>
        public EBaseType KeyType => _type;
        /// <summary> 容器类型 </summary>
        public EContainer Container => _container;

        /// <summary> 若为容器, 则返回容器元素类型, 否则返回当前类型 </summary>
        public EBaseType ValType => _container switch
        {
            EContainer.None => _type,
            EContainer.Array => _fieldCount <= 0 ? _t00.Type : EBaseType.Custom,
            EContainer.Dictionary => _fieldCount <= 0 ? _t00.Type : EBaseType.Custom,
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <summary> 若为容器, 则为其元素 flag, 否则为其 flag </summary>
        public ETypeFlag ValFlag => _container switch
        {
            EContainer.None => _flag,
            EContainer.Array => _fieldCount <= 0 ? _t00.Flag : _valFlag,
            EContainer.Dictionary => _fieldCount <= 0 ? _t00.Flag : _valFlag,
            _ => throw new ArgumentOutOfRangeException()
        };

        public bool IsIdType
        {
            get
            {
                if (Container != EContainer.None) return false;
                if (ValFlag != ETypeFlag.None) return false;
                return string.IsNullOrWhiteSpace(Alias) && ValType.IsTypeCanBeId();
            }
        }

        /// <summary> 根据下表获得字段, 下标范围 0~15 </summary>
        public Field this[int i]
        {
            private set
            {
                switch (i)
                {
                    case 0: _t00 = value; return;
                    case 1: _t01 = value; return;
                    case 2: _t02 = value; return;
                    case 3: _t03 = value; return;
                    case 4: _t04 = value; return;
                    case 5: _t05 = value; return;
                    case 6: _t06 = value; return;
                    case 7: _t07 = value; return;
                    case 8: _t08 = value; return;
                    case 9: _t09 = value; return;
                    case 10: _t10 = value; return;
                    case 11: _t11 = value; return;
                    case 12: _t12 = value; return;
                    case 13: _t13 = value; return;
                    case 14: _t14 = value; return;
                    case 15: _t15 = value; return;
                }
            }

            get => i switch
            {
                0 => _t00,
                1 => _t01,
                2 => _t02,
                3 => _t03,
                4 => _t04,
                5 => _t05,
                6 => _t06,
                7 => _t07,
                8 => _t08,
                9 => _t09,
                10 => _t10,
                11 => _t11,
                12 => _t12,
                13 => _t13,
                14 => _t14,
                15 => _t15,
                _ => new Field(EBaseType.Null)
            };
        }

        public TypeInfo(EBaseType type, string alias, ETypeFlag flag)
        {
            _type = type;
            _alias = alias;
            _fieldCount = 0;
            _t00 = _t01 = _t02 = _t03 = _t04 = _t05 = _t06 = _t07 = _t08 =
                _t09 = _t10 = _t11 = _t12 = _t13 = _t14 = _t15 = default;
            _container = EContainer.None;
            _flag = flag;
            _valFlag = flag;
            Attribute = string.Empty;
        }

        public TypeInfo(EContainer container, EBaseType keyType, ETypeFlag valFlag, ETypeFlag flag)
            : this(keyType, string.Empty, valFlag)
        {
            switch (container)
            {
                case EContainer.None:
                    throw new ArgumentException("can't create a none container by the container constructor");
                case EContainer.Array:
                    if (_type != EBaseType.Int32) throw new ArgumentException("array can't set a none-int32 key type");
                    break;
                case EContainer.Dictionary:
                default: break;
            }

            _flag = flag;
            _valFlag = valFlag;
            _container = container;
        }

        public void Clear()
        {
            if (_type != EBaseType.Custom && _container == EContainer.None) return;
            for (var i = 0; i < MaxFieldCount; i++)
                this[i] = new Field(-1, string.Empty, EBaseType.Null);
            _fieldCount = 0;
        }

        public void SetContainerValTypeBase(EBaseType baseType, ETypeFlag flag = ETypeFlag.None)
        {
            Clear();
            _alias = string.Empty;
            _t00 = new Field(baseType, flag);
        }

        public void SetContainerValTypeAlias(string alias, ETypeFlag flag = ETypeFlag.None)
        {
            Clear();
            _alias = alias;
            _valFlag = flag;
            _t00 = new Field(EBaseType.Custom, flag);
        }

        public void SetContainerValTypeFlag(ETypeFlag flag)
        {
            _valFlag = flag;
        }

        public bool CustomEquals(in TypeInfo info)
        {
            if (_type != info._type) return false;
            if (_container != info._container) return false;

            if (_type != EBaseType.Custom) return _flag == info._flag;
            if (_alias == info._alias && (info._fieldCount == 0 || _fieldCount == 0)) return true;

            if (_fieldCount != info._fieldCount) return false;
            for (var i = 0; i < _fieldCount; i++) if (info[i] != this[i]) return false;
            return true;
        }

        public bool ContainerValueEquals(in TypeInfo info)
        {
            if (_container == EContainer.None) return false;
            if (string.IsNullOrEmpty(_alias))
            {
                if (_fieldCount == 0) return _t00.Type == info.KeyType;
                if (_fieldCount != info._fieldCount) return false;
                for (var i = 0; i < _fieldCount; i++) if (info[i] != this[i]) return false;
                return true;
            }

            if (_alias == info._alias && (info._fieldCount == 0 || _fieldCount == 0)) return true;
            if (_fieldCount != info._fieldCount) return false;
            for (var i = 0; i < _fieldCount; i++) if (info[i] != this[i]) return false;
            return true;
        }

        public void Add(sbyte order, EBaseType baseType, string name, ETypeFlag flag = ETypeFlag.None)
        {
            if (_type != EBaseType.Custom && _container == EContainer.None)
                throw new ArgumentException("can't add field to a base type");
            if (_container != EContainer.None && _fieldCount == 0 && _t00.Type != EBaseType.Null)
                throw new ArgumentException("can't add field to a base type");
            if (_fieldCount == MaxFieldCount)
                throw new IndexOutOfRangeException($"field count must less than {MaxFieldCount}");

            var i = 0;
            var newField = new Field(order, name, baseType, flag);

            for (; i < _fieldCount; i++)
            {
                var t = this[i];
                switch (CompareField(t, newField))
                {
                    case 0: throw new ArgumentException("repeated field");
                    case < 0: continue;
                }

                for (var j = i + 1; j < _fieldCount + 1; j++)
                    (this[j], t) = (t, this[j]);
                this[i] = newField;
                _fieldCount++;
                return;
            }
            this[_fieldCount++] = newField;
            return;

            static int CompareField(in Field a, in Field b)
            {
                return a.Type == b.Type
                    ? string.Compare(a.Name, b.Name, StringComparison.Ordinal)
                    : ((byte)a.Type).CompareTo((byte)b.Type);
            }
        }

        [Pure]
        public (Field field, int idx) GetFieldByOrder(int order)
        {
            for (var i = 0; i < FieldCount; i++)
            {
                var f = this[i];
                if (f.Order == order) return (f, i);
            }
            return (new Field(EBaseType.Null), -1);
        }

        public override int GetHashCode() => 0; // 不允许比较
        public override string ToString() => ToString(true);
        public override bool Equals([NotNullWhen(true)] object? obj) => false; // 不允许比较

        /// <summary>
        /// 所有类型都有自己的唯一元字串, 形如: I? 或 F! 或 Name1:B?|Name2:I?|Name3:D!|Name4:S!?+!
        /// <para> 类型解析时会将字段名重新排列, 故含同名字段且类型也相同的视作同一个类型 </para>
        /// </summary>
        /// <param name="isCheckContainer"> 是否显示容器类型, 若显示, 则类型末尾会加上 +/*, 分别代表数组和字典 </param>
        /// <param name="isCheckTypeNullable"> 是否检查类型可空, 若有则会在类型末尾加上 ?/! 代表可空与不可空, 该参数不会影响复合类型中字段的可空性显示 </param>
        /// <returns></returns>
        [Pure]
        public string ToString(bool isCheckContainer, bool isCheckTypeNullable = true)
        {
            var container = isCheckContainer ? _container : EContainer.None;
            switch (container)
            {
                case EContainer.None:
                {
                    // 基础类型, 形如: I? 或 F! 或 Name1:B?|Name2:I?|Name3:D!|Name4:S!?
                    if (ValType != EBaseType.Custom)
                    {
                        if (isCheckTypeNullable) return TypToStr(ValType) + FlagToStr(Flag);
                        return TypToStr(ValType);
                    }
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(_alias)) sb.Append(_alias).Append('.');
                    for (var i = 0; i < _fieldCount; i++)
                    {
                        var field = this[i];

                        if (i != 0) sb.Append('|');
                        sb.Append(field.Name)
                            .Append(':')
                            .Append(TypToStr(field.Type))
                            .Append(FlagToStr(field.Flag));
                    }
                    return isCheckTypeNullable ? sb + FlagToStr(Flag) : sb.ToString();
                }
                case EContainer.Array:
                {
                    // 数组类型, 形如: I!+? 或 Name1:F!|Name2:I?!+!
                    if (_fieldCount <= 0) return TypToStr(ValType) + FlagToStr(ValFlag) + '+' + (isCheckTypeNullable ? FlagToStr(Flag) : "");
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(_alias)) sb.Append(_alias).Append('.');
                    for (var i = 0; i < _fieldCount; i++)
                    {
                        var field = this[i];
                        if (i != 0) sb.Append('|');
                        sb.Append(field.Name)
                            .Append(':')
                            .Append(TypToStr(field.Type))
                            .Append(FlagToStr(field.Flag));
                    }
                    sb.Append(FlagToStr(ValFlag)).Append('+');
                    return sb + (isCheckTypeNullable ? FlagToStr(Flag) : "");
                }
                case EContainer.Dictionary:
                {
                    // 字典类型, 形如: A:I!|C:F!|B:D?!*?F
                    if (_fieldCount <= 0) return TypToStr(ValType) + FlagToStr(ValFlag) + '*' + TypToStr(KeyType) + (isCheckTypeNullable ? FlagToStr(Flag) : "");
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(_alias)) sb.Append(_alias).Append('.');
                    for (var i = 0; i < _fieldCount; i++)
                    {
                        var field = this[i];
                        if (i != 0) sb.Append('|');
                        sb.Append(field.Name)
                            .Append(':')
                            .Append(TypToStr(field.Type))
                            .Append(FlagToStr(field.Flag));
                    }
                    sb.Append(FlagToStr(ValFlag))
                        .Append('*')
                        .Append(isCheckTypeNullable ? FlagToStr(Flag) : "")
                        .Append(TypToStr(KeyType));
                    return sb.ToString();
                }
                default: return "";
            }

            static string FlagToStr(ETypeFlag flag)
                => flag == ETypeFlag.None ? "!" : "?";

        }

        /// <summary>
        /// 获得类型标识符
        /// </summary>
        /// <param name="sbA"> 字符串构造器, 如果 isAppend == true, 则将标识符直接 append 到该构造器中, 否则 sbA 将会被清空 </param>
        /// <param name="sbB"> 临时字符串构造器, 会被清空 </param>
        /// <param name="isAppend"> 如果为 true 则将结果直接 append 到 sbA 中 </param>
        /// <returns> 返回计算结果 </returns>
        [Pure]
        public string GetTypeIdentity(StringBuilder? sbA = null, StringBuilder? sbB = null, bool isAppend = false)
        {
            var len = 0;
            if (sbA == null) isAppend = false;
            sbA ??= new StringBuilder();
            sbB ??= new StringBuilder();
            if (!isAppend) sbA.Clear();
            else len = sbA.Length;
            sbB.Clear();

            sbB.Clear();
            for (var i = 0; i < FieldCount; i++)
            {
                var field = this[i];
                sbA.Append(TypToStr(field.Type));
                if ((field.Flag & ETypeFlag.Nullable) != 0) sbA.Append('0');
                sbB.Append(field.Name).Append('_');
            }
            sbA.Append('_').Append(sbB);
            sbB.Clear();
            var ret = isAppend ? sbA.ToString(len, sbA.Length - len) : sbA.ToString();
            if (!isAppend) sbA.Clear();
            return ret;
        }

        public static string TypToStr(EBaseType type)
            => type switch
            {
                EBaseType.Null => "N",
                EBaseType.Bool => "B",
                EBaseType.Int8 => "Y",
                EBaseType.Int16 => "H",
                EBaseType.Int32 => "I",
                EBaseType.Int64 => "L",
                EBaseType.UInt8 => "J",
                EBaseType.UInt16 => "O",
                EBaseType.UInt32 => "U",
                EBaseType.UInt64 => "P",
                EBaseType.Float32 => "F",
                EBaseType.Float64 => "D",
                EBaseType.String => "S",
                EBaseType.Link => "T",
                EBaseType.Enum => "E",
                // EBaseType.Custom => "C",
                _ => "_"
            };

        private ETypeFlag _valFlag;
        private string _alias;
        private byte _fieldCount;

        private readonly ETypeFlag _flag;

        private readonly EBaseType _type;
        private readonly EContainer _container;

        private Field _t00; // 若为容器, 且容器元素为基础类型, 则用该字段储存基础类型数据
        private Field _t01;
        private Field _t02;
        private Field _t03;
        private Field _t04;
        private Field _t05;
        private Field _t06;
        private Field _t07;
        private Field _t08;
        private Field _t09;
        private Field _t10;
        private Field _t11;
        private Field _t12;
        private Field _t13;
        private Field _t14;
        private Field _t15;
    }
}