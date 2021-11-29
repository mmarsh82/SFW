using System;
using System.ComponentModel;

namespace QT9Client
{
    public static class Extensions
    {
        /// <summary>
        /// Get an enum value from its description attribute
        /// </summary>
        /// <typeparam name="T">Enum value</typeparam>
        /// <param name="description">Description attribute to search for</param>
        /// <returns>Enum value</returns>
        public static string GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return ((T)field.GetValue(null)).ToString();
                }
                else
                {
                    if (field.Name == description)
                        return ((T)field.GetValue(null)).ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Retrieve the description attribute set for an enum value
        /// </summary>
        /// <param name="e">Current Enum</param>
        /// <returns>DescriptionAttribute as string</returns>
        public static string GetDescription(this Enum e)
        {
            var das = (DescriptionAttribute[])e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return das != null && das.Length > 0 ? das[0].Description : e.ToString();
        }
    }
}
