using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

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
        /// Build a DataView RowFilter Search query based on search input criteria
        /// </summary>
        /// <param name="table">Source DataTable</param>
        /// <param name="query">Search string</param>
        public static string SearchRowFilter(this DataTable table, string query)
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
            return queryBuilder.ToString();
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
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        /// <summary>
        /// Retrieve the index of an item within an ICollectionView
        /// </summary>
        /// <param name="view">ICollectionView object</param>
        /// <param name="item">Item to search for</param>
        /// <returns>index as int</returns>
        public static int IndexOf(this ICollectionView view, object item)
        {
            var e = view.GetEnumerator();
            var idx = 0;
            while (e.MoveNext())
            {
                if (Equals(item, e.Current))
                    return idx;
                else
                    idx++;
            }
            //if we've got this far it means that the item is either filtered out or is not in the source collection
            return -1;
        }
    }
}
