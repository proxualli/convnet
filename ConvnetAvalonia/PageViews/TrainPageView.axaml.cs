using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.TextMate;
using ConvnetAvalonia.Common;
using ConvnetAvalonia.PageViewModels;
using ConvnetAvalonia.Properties;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml;
using TextMateSharp.Grammars;
using static System.Net.Mime.MediaTypeNames;

namespace ConvnetAvalonia.PageViews
{
    public partial class TrainPageView : UserControl
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public TrainPageView()
        {
            //string[] names = this.GetType().Assembly.GetManifestResourceNames();
            //string[] anames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
           
            InitializeComponent();


            //var gr = this.FindControl<Grid>("grid");
        }
      
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
