using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ayok.Excel.Helper
{
    public static class PropertiesHelper
    {
        public static List<PropertyInfo> GetBasePropertiesFirstWithoutNew(
            Type type,
            bool includeInherited
        )
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            if (includeInherited)
            {
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                List<Type> list2 = new List<Type>();
                Type type2 = type;
                while (type2 != null && type2 != typeof(object))
                {
                    list2.Insert(0, type2);
                    type2 = type2.BaseType;
                }
                HashSet<string> addedProperties = new HashSet<string>();
                foreach (
                    PropertyInfo item in from hierarchyType in list2
                    select hierarchyType.GetProperties(
                        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public
                    ) into declaredProperties
                    from prop in declaredProperties
                    where !addedProperties.Contains(prop.Name)
                    select prop
                )
                {
                    list.Add(item);
                    addedProperties.Add(item.Name);
                }
            }
            else
            {
                list.AddRange(
                    type.GetProperties(
                        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public
                    )
                );
            }
            return list;
        }

        public static string GetPropertyDisplayName(PropertyInfo property)
        {
            return property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name;
        }
    }
}
