﻿using System;

namespace catnexu.googlesheetsforunity
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GoogleSheetAttribute : Attribute
    {
        public readonly string TableName;
        public readonly string SheetName;

        public GoogleSheetAttribute(string tableName, string sheetName)
        {
            TableName = tableName;
            SheetName = sheetName;
        }
    }
}