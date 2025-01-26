using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEditor;
using UnityEngine;

namespace catnexu.googlesheetsforunity.Editor
{
    public static class GoogleSheetLoader
    {
        private class SheetOrderComparer : IComparer<KeyValuePair<int, ScriptableObject>>
        {
            public int Compare(KeyValuePair<int, ScriptableObject> x, KeyValuePair<int, ScriptableObject> y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }

        private const string Range = "{0}!A:AA";
        private const BindingFlags SetSheetFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        private static readonly IRowConverter s_rowConverter = new GoogleSheetRowConverter();

        [MenuItem("Tools/Utils/Google Sheets/Load from Google")]
        public static void LoadSheets()
        {
            List<ScriptableObject> sheetSets = GetOrderedSheetSets();
            int sheetCount = sheetSets.Count;
            for (int i = 0; i < sheetCount; i++)
            {
                ScriptableObject sheet = sheetSets[i];
                try
                {
                    if (!TryLoad(sheet))
                    {
                        Debug.LogError($"Can not load sheet [{sheet.name}]");
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Could not load sheet [{sheet.name}] with exception: {exception}");
                }
            }
        }

        public static bool TryLoadByName<T>(string tableName, string sheetName, out List<T> data)
        {
            data = new List<T>();
            if (!TryLoadByName(tableName, sheetName, out List<List<object>> table))
            {
                return false;
            }

            data = Convert(table, typeof(T)) as List<T>;
            return data != null;
        }

        public static bool TryLoadByName(string tableName, string sheetName, out List<List<object>> data)
        {
            data = new List<List<object>>();
            var sheetSettings = GoogleSheetSettings.instance;
            if (!sheetSettings.TryGetTableId(tableName, out string tableId))
            {
                Debug.LogError("Invalid Table Name!");
                return false;
            }

            return TryLoadById(tableId, sheetName, out data);
        }

        public static bool TryLoadById<T>(string tableId, string sheetName, out List<T> data)
        {
            data = new List<T>();
            if (!TryLoadById(tableId, sheetName, out List<List<object>> table))
            {
                return false;
            }

            data = Convert(table, typeof(T)) as List<T>;
            return data != null;
        }

        public static bool TryLoadById(string tableId, string sheetName, out List<List<object>> data)
        {
            data = new List<List<object>>();
            GoogleSheetsManager.AuthGoogle();
            if (string.IsNullOrEmpty(sheetName))
            {
                Debug.LogError("Sheet name is null or empty!");
                return false;
            }

            if (string.IsNullOrEmpty(tableId))
            {
                Debug.LogError("Table id is null or empty");
                return false;
            }

            string range = string.Format(Range, sheetName);
            SpreadsheetsResource.ValuesResource.GetRequest request =
                GoogleSheetsManager.SheetsService.Spreadsheets.Values.Get(tableId, range);
            IList<IList<object>> tableList = null;
            try
            {
                ValueRange response = request.Execute();
                tableList = response.Values;
                if (tableList == null || tableList.Count <= 0)
                {
                    throw new Exception("Table is empty");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.ToString());
                return false;
            }

            data = tableList.Cast<List<object>>().ToList();
            return true;
        }

        private static List<ScriptableObject> GetSheetSets()
        {
            string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            var sheetSetList = new List<ScriptableObject>();
            foreach (var guid in soGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (scriptableObject is ISheetSet)
                {
                    sheetSetList.Add(scriptableObject);
                }
            }

            return sheetSetList;
        }

        private static List<ScriptableObject> GetOrderedSheetSets()
        {
            List<ScriptableObject> sheetSets = GetSheetSets();
            int sheetCount = sheetSets.Count;
            var orderedSheetSets = new List<KeyValuePair<int, ScriptableObject>>(sheetCount);
            for (int i = 0; i < sheetCount; i++)
            {
                ScriptableObject sheetSet = sheetSets[i];
                var orderAttribute =
                    (LoadSheetOrderAttribute) Attribute.GetCustomAttribute(sheetSet.GetType(),
                        typeof(LoadSheetOrderAttribute));
                int order = orderAttribute != null ? orderAttribute.SheetOrder : int.MaxValue;
                orderedSheetSets.Add(new KeyValuePair<int, ScriptableObject>(order, sheetSet));
            }

            orderedSheetSets.Sort(new SheetOrderComparer());
            sheetSets.Clear();
            for (int i = 0; i < sheetCount; i++)
            {
                sheetSets.Add(orderedSheetSets[i].Value);
            }

            return sheetSets;
        }

        private static object Convert(List<List<object>> source, Type elementType)
        {
            List<object> data = s_rowConverter.Convert(source, elementType);
            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList list = (IList) Activator.CreateInstance(listType);
            if (data != null)
            {
                foreach (var value in data)
                {
                    list.Add(value);
                }

                return list;
            }

            return null;
        }

        private static bool TryLoad(ScriptableObject sheetSet)
        {
            Type sheetType = sheetSet.GetType();
            GoogleSheetAttribute sheetAttribute =
                (GoogleSheetAttribute) Attribute.GetCustomAttribute(sheetType, typeof(GoogleSheetAttribute));
            if (sheetAttribute == null)
            {
                return false;
            }

            Type[] sheetSetTypes = sheetType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISheetSet<>)).ToArray();
            if (sheetSetTypes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < sheetSetTypes.Length; i++)
            {
                Type sheetSetType = sheetSetTypes[i];
                MethodInfo setSheetMethodInfo = sheetSetType.GetMethod("SetSheet", SetSheetFlags);
                if (setSheetMethodInfo == null)
                {
                    continue;
                }

                Type singleArg = sheetSetType.GetGenericArguments().Single();
                if (!TryLoadByName(sheetAttribute.TableName, sheetAttribute.SheetName, out List<List<object>> data))
                {
                    continue;
                }

                setSheetMethodInfo.Invoke(sheetSet, new[] {Convert(data, singleArg)});
            }

            return true;
        }
    }
}