using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ConvnetAvalonia.Converters
{
    public class EnumConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Enum? enumValue = default(Enum);
            if (parameter is Type)
            {
                if (value != null)
                    enumValue = (Enum)Enum.Parse((Type)parameter, value.ToString());
            }
            return enumValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            int returnValue = 0;
            if (parameter is Type)
            {
                returnValue = (int)Enum.Parse((Type)parameter, value.ToString());
            }
            return returnValue;
        }
    }

    public class EnumToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    var EnumString = Enum.GetName((value.GetType()), value);
                    return EnumString;
                }
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // No need to implement converting back on a one-way binding 
        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class BoolToStringConverter : BoolToValueConverter<FontWeight> { }

    public class BatchSizeToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return (uint)value > (uint)1 ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullableBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            return (((bool?)value).HasValue && ((bool?)value).Value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseNullableBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            return (((bool?)value).HasValue && ((bool?)value).Value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert bool or Nullable bool to Visibility
        /// </summary>
        /// <param name="value">bool or Nullable bool</param>
        /// <param name="targetType">Visibility</param>
        /// <param name="parameter">null</param>
        /// <param name="culture">null</param>
        /// <returns>Visible or Collapsed</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is Nullable<bool>)
            {
                Nullable<bool> tmp = (Nullable<bool>)value;
                bValue = tmp.HasValue ? tmp.Value : false;
            }
            return (bValue) ? true : false;
        }

        /// <summary>
        /// Convert Visibility to boolean
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value == true;
            }
            else
            {
                return false;
            }
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; } = Visibility.Collapsed;
        public Visibility FalseValue { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            return value as bool? == true ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Visibility? == TrueValue;
        }
    }

    public class BooleanValueInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is IValueConverter))
            {
                // No second converter is given as parameter:
                // Just invert and return, if boolean input value was provided
                if (value is bool)
                    return !(bool)value;
                else
                    return DependencyProperty.UnsetValue; // Fallback for non-boolean input values
            }
            else
            {
                // Second converter is provided:
                // Retrieve this converter...
                IValueConverter converter = (IValueConverter)parameter;

                if (value is bool)
                {
                    // ...if boolean input value was provided, invert and then convert
                    bool input = !(bool)value;
                    return converter.Convert(input, targetType, null, culture);
                }
                else
                {
                    // ...if input value is not boolean, convert and then invert boolean result
                    object convertedValue = converter.Convert(value, targetType, null, culture);
                    if (convertedValue is bool)
                        return !(bool)convertedValue;
                    else
                        return DependencyProperty.UnsetValue; // Fallback for non-boolean return values
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(GridLength))
                throw new InvalidOperationException("The target must be a GridLength");

            return (bool)value ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Will return a*value + b
    /// </summary>
    public class FirstDegreeFunctionConverter : IValueConverter
    {
        public double A { get; set; }
      
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double a = GetDoubleValue(parameter, A);

            double x = GetDoubleValue(value, 0.0);

            return Math.Abs(x - a);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double a = GetDoubleValue(parameter, A);

            double y = GetDoubleValue(value, 0.0);

            return Math.Abs(y + a);
        }

        #endregion


        private double GetDoubleValue(object parameter, double defaultValue)
        {
            double a;

            if (parameter != null)
            {
                try
                {
                    a = System.Convert.ToDouble(parameter);
                }
                catch
                {
                    a = defaultValue;
                }
            }
            else
            {
                a = defaultValue;
            }

            return a;
        }
    }
}
