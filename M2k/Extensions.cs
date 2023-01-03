using System;
using System.ComponentModel;
using System.Globalization;

namespace M2kClient
{
    public static class Extensions
    {
        /// <summary>
        /// Change the database that the M2kConnection is currently using
        /// </summary>
        /// <param name="connection">Current M2kConnection Object</param>
        /// <param name="database">New database to use</param>
        /// <param name="facCode">M2k Facility to use</param>
        public static void DatabaseChange(this M2kConnection connection, Database database, int facCode)
        {
            connection.Facility = facCode;
            connection.Database = database;
            connection.BTIFolder = $"{database.GetDescription()}BTI.TRANSACTIONS\\";
            connection.SFDCFolder = $"{database.GetDescription()}SFDC.TRANSACTIONS\\";
        }

        /// <summary>
        /// Get an Enum description
        /// </summary>
        /// <typeparam name="T">Any object that inherits from IConvertible that has used the component model to add a description value</typeparam>
        /// <param name="e">Object to get the component model description</param>
        /// <returns>Description</returns>
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);
                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }
                        break;
                    }
                }
            }
            return description;
        }

        /// <summary>
        /// Get an enum value from its description attribute
        /// </summary>
        /// <typeparam name="T">Enum value</typeparam>
        /// <param name="description">Description attribute to search for</param>
        /// <returns>Enum value</returns>
        public static string GetValueFromDescription<T>(this string description)
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
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }
    }
}
