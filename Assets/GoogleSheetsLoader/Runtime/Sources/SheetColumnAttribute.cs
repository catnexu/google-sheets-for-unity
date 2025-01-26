using System;

namespace catnexu.gsl
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SheetColumnAttribute : Attribute
    {
        public readonly string ColumnName;
        
        public int ValueIndex { get; set; }

        public SheetColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}