using Gorira.Attributes.ConversionAttributes;
using System.Reflection;

namespace Gorira.Helpers
{
    public static class EnumExtension
    {
        public static string GetStringValue(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            StringValueAttribute[] attribs = field.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            return attribs.Length > 0 ? attribs[0].Value : null;
        }
    }
}
