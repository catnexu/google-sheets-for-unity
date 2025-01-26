using System;
using UnityEngine;

namespace catnexu.gsl.Editor.ValueConverters
{
    public class EnumConverter : ValueConverter<Enum>
    {
        public override object Convert(string input, Type type)
        {
            object result = null;
            try
            {
                result = Enum.Parse(type, input);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error value convert while import from Google Sheets: " + ex);
            }
            
            return result;
        }
    }
}