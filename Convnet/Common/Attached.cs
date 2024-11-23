using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace Convnet.Common
{
    public class Attached
    {
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached("FormattedText", typeof(string), typeof(Attached), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure, FormattedTextPropertyChanged));

        public static void SetFormattedText(DependencyObject textBlock, string value)
        {
            textBlock.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject textBlock)
        {
            if (textBlock != null)
                return (string)textBlock.GetValue(FormattedTextProperty);
            else
                throw new ArgumentNullException("FormattedText");
        }

        private static void FormattedTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                string formattedText = (string)e.NewValue ?? string.Empty;
                formattedText = string.Format("<Span xml:space=\"preserve\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{0}</Span>", formattedText);

                textBlock.Inlines.Clear();
               
                using (var xmlReader = XmlReader.Create(new StringReader(formattedText)))
                {
                    if (XamlReader.Load(xmlReader) is Span result)
                        textBlock.Inlines.Add(result);
                }
            }
        }
    }
}
