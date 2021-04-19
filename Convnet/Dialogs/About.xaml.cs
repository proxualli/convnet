using System;
using System.Reflection;
using System.Windows;

namespace Convnet.Dialogs
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlockApplication.Text = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute))).Description;
        }
    }
}
