using System.Collections.Generic;

namespace catnexu.googlesheetsforunity
{
    public interface ISheetSet { }

    public interface ISheetSet<TData> : ISheetSet
    {
        void SetSheet(List<TData> value);
    }
}