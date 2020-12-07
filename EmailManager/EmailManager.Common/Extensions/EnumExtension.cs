using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EmailManager.Common.Extensions
{
    /// <summary>
    /// Custom Extension Method implementation for Enum.ToDisplay()
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Extension method for the enum type, add the ability to display user selective string of an enum value.
        /// </summary>
        /// <param name="c">This parameter is here to specify the type to add an extension method to.</param>
        /// <returns>
        /// The return value is a string containing the name of the constant or the value of the description attribute if one exists 
        /// If the FlagsAttribute is applied and there is a combination of one or more named constants equal to the value of this instance 
        /// then the return value is a string containing a delimiter-separated list of the names of the constants or the value of this instance of the description attribute if one exists
        /// Otherwise, the return value is the string representation of the numeric value of this instance or the value of this instance of the description attribute if one exists.
        /// </returns>

        public static string ToDisplay(this Enum c)
        {
            return GetDescription(c);
        }
        /// <summary>
        /// Determine if Enum has [flags] or not
        /// </summary>
        /// <param name="t">Enum type to check</param>
        /// <returns>True/False</returns>
        private static bool HasFlagsAttribute(Type t)
        {
            return t.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
        }

        /// <summary>
        /// Get the description depending on [flags] attribute
        /// </summary>
        /// <param name="value">comma separated list of values</param>
        /// <returns>Returns description string</returns>
        private static string GetDescription(Enum value)
        {
            if (HasFlagsAttribute(value.GetType()))
            {
                return GetFlagDescriptions(value);
            }
            else
            {
                return GetNoFlagDescriptions(value);
            }
        }

        /// <summary>
        /// Return either the value for the custom attribute or the value
        /// </summary>
        /// <param name="value">Enum to check</param>
        /// <returns>Returns string</returns>
        private static string GetNoFlagDescriptions(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            try
            {
                DescriptionAttribute[] attributes =
                      (DescriptionAttribute[])fi.GetCustomAttributes(
                      typeof(DescriptionAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
            }
            catch (NullReferenceException)
            {
                return value.ToString();
            }
        }
        /// <summary>
        /// Return either the value for the custom attribute or the value.  In this case handle enums with the
        /// [flags] attribute
        /// </summary>
        /// <param name="value">Enum to check</param>
        /// <returns>Returns string</returns>
        private static string GetFlagDescriptions(Enum value)
        {

            string[] names = value.ToString().Split(new string[] { ", " }, StringSplitOptions.None);
            StringBuilder descriptions = new StringBuilder();
            for (int i = 0; i < names.Length; i++)
            {
                Enum enumValue = (Enum)Enum.Parse(value.GetType(), names[i]);
                descriptions.Append(GetNoFlagDescriptions(enumValue));
                descriptions.Append(",");
            }
            descriptions.Length--;
            return descriptions.ToString();
        }

        /// <summary>
        /// Returns the enum whose description attribute value is passed
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="description">Description</param>
        /// <returns>the enum</returns>
        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
        }
        // This method creates a specific call to the above method, requesting the
        // Display MetaData attribute.
        //e.g. [Display(Name = "S4x")]
        public static string ToDisplayName(this Enum value)
        {
            var attribute = value.GetAttribute<DisplayAttribute>();
            return attribute == null ? value.ToString() : attribute.Name;
        }

        // This extension method is broken out so you can use a similar pattern with 
        // other MetaData elements in the future. This is your base method for each.
        //In short this is generic method to get any type of attribute.
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return (T)attributes.FirstOrDefault();
        }

        // This method creates a specific call to the above method, requesting the
        // Description MetaData attribute.
        //e.g. [Description("Day of week. Sunday")]
        public static string ToDescription(this Enum value)
        {
            var attribute = value.GetAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
