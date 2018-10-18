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
        public static void DatabaseChange(this M2kConnection connection, Database database)
        {
            connection.Database = database;
            connection.BTIFolder = database.GetDescription();
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
    }
}
