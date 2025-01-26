using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace catnexu.googlesheetsforunity.Editor
{
    internal class GoogleSheetRowConverter : IRowConverter
    {
        private class TypeInfo
        {
            public readonly Type Type;
            public readonly List<KeyValuePair<FieldInfo, SheetColumnAttribute>> Fields;
            public readonly List<FieldInfo> DataFields;

            public TypeInfo(Type type, List<KeyValuePair<FieldInfo, SheetColumnAttribute>> fields, List<FieldInfo> dataFields)
            {
                Type = type;
                Fields = fields;
                DataFields = dataFields;
            }
        }

        private readonly List<string> _headers;
        private readonly List<TypeInfo> _typeInfos;

        public GoogleSheetRowConverter()
        {
            _headers = new List<string>();
            _typeInfos = new List<TypeInfo>();
        }

        public List<object> Convert(List<List<object>> table, Type outObjectType)
        {
            var result = new List<object>();
            _headers.Clear();
            _headers.AddRange(table[0].Cast<string>());
            for (int i = 1; i < table.Count; i++)
            {
                var item = GetParsedObject(table[i], outObjectType);
                if (item != null)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private object GetParsedObject(IList<object> values, Type outputType)
        {
            var typeInfo = GetTypeInfo(outputType);
            var item = Activator.CreateInstance(outputType);
            bool isSetup = false;

            if (typeInfo.Fields != null && typeInfo.Fields.Count > 0)
            {
                foreach (var field in typeInfo.Fields)
                {
                    if (field.Value.ValueIndex < values.Count)
                    {
                        var value = ValueConvertHelper.Convert(values[field.Value.ValueIndex].ToString(), field.Key.FieldType);
                        if (value != null)
                        {
                            field.Key.SetValue(item, value);
                            isSetup = true;
                        }
                    }
                }
            }

            if (typeInfo.DataFields != null && typeInfo.DataFields.Count > 0)
            {
                foreach (var field in typeInfo.DataFields)
                {
                    var dataObj = GetParsedObject(values, field.FieldType);
                    if (dataObj != null)
                    {
                        field.SetValue(item, dataObj);
                        isSetup = true;
                    }
                }
            }

            return isSetup ? item : null;
        }

        private TypeInfo GetTypeInfo(Type type)
        {
            TypeInfo typeInfo = _typeInfos.FirstOrDefault(x => x.Type == type);
            if (typeInfo != null)
            {
                return typeInfo;
            }

            typeInfo = new TypeInfo(type, new List<KeyValuePair<FieldInfo, SheetColumnAttribute>>(), new List<FieldInfo>());
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.IsDefined(typeof(SheetColumnAttribute), false));
            foreach (var field in fields)
            {
                var attribute = (SheetColumnAttribute) Attribute.GetCustomAttribute(field, typeof(SheetColumnAttribute));
                if (_headers.Contains(attribute.ColumnName))
                {
                    attribute.ValueIndex = _headers.IndexOf(attribute.ColumnName);
                    typeInfo.Fields.Add(new KeyValuePair<FieldInfo, SheetColumnAttribute>(field, attribute));
                }
            }

            _typeInfos.Add(typeInfo);
            return typeInfo;
        }
    }
}