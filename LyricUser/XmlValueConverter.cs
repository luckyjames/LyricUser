
namespace LyricUser
{
    internal class XmlValueConverter
    {
        public static ValueType ConvertStringToValue<ValueType>(string stringValue)
        {
            return (ValueType)System.Convert.ChangeType(stringValue, typeof(ValueType));
        }
    }
}
