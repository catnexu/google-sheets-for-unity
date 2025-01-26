using System.Collections.Generic;

namespace catnexu.gsl
{
    public interface ISheetSet { }

    public interface ISheetSet<TData> : ISheetSet
    {
        void SetSheet(List<TData> value);
    }
}