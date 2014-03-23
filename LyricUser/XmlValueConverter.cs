
namespace LyricUser
{
    internal class XmlValueConverter
    {
        public static ValueType ConvertStringToValue<ValueType>(string stringValue)
        {
            try
            {
                return (ValueType)System.Convert.ChangeType(stringValue, typeof(ValueType));
            }
            catch (System.FormatException)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Can't convert '" + stringValue + "' to " + typeof(ValueType).ToString());

                throw;
            }
        }
    }
}
