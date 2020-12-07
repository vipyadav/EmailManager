using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace EmailManager.Common.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        /// <summary>
        /// Convert value for binding from source object
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Enum enumValue = default;
                if (parameter is Type type)
                {
                    enumValue = (Enum)Enum.Parse(type, value.ToString());
                }

                var fieldInfo = enumValue.GetType().GetMember(enumValue.ToString());
                if (fieldInfo != null && fieldInfo.Any())
                {
                    var attributes = fieldInfo.First().GetCustomAttributes(typeof(Attribute), false);
                    var attribute = (DescriptionAttribute)attributes.FirstOrDefault();
                     
                    return attribute == null ? value.ToString() : attribute.Description;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// ConvertBack value from binding back to source object
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.Exception">Can't convert back.</exception>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back.");
        }
    }
}
