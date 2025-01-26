using System;

namespace catnexu.googlesheetsforunity
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LoadSheetOrderAttribute : Attribute
    {
        public readonly int SheetOrder;

        public LoadSheetOrderAttribute(int sheetOrder)
        {
            SheetOrder = sheetOrder;
        }
    }
}