using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace SFW
{
    public static class Extensions
    {
        /// <summary>
        /// Search a DataTable for a value
        /// </summary>
        /// <param name="table">Source DataTable</param>
        /// <param name="query">Search string</param>
        public static void Search(this DataTable table, string query)
        {
            var columnNames = (from dc in table.Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();
            var counter = 0;
            var queryBuilder = new StringBuilder();
            foreach (var name in columnNames)
            {
                if (counter == 0)
                {
                    queryBuilder.Append($"Convert(`{name}`, 'System.String') LIKE '%{query}%'");
                }
                else
                {
                    queryBuilder.Append($"OR Convert(`{name}`, 'System.String') LIKE '%{query}%'");
                }
                counter++;
            }
            table.DefaultView.RowFilter = queryBuilder.ToString();
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
