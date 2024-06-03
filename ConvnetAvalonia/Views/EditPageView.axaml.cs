using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using ConvnetAvalonia.Properties;
using System;
using System.IO;
using System.Xml;

namespace ConvnetAvalonia.PageViews
{
    public partial class EditPageView : UserControl
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public EditPageView()
        {
            //string[] names = this.GetType().Assembly.GetManifestResourceNames();
            //string[] anames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            //var bitmap = new Bitmap(AssetLoader.Open(new Uri("ConvnetAvalonia.Resources.Definition.xshd")));

            IHighlightingDefinition DefinitionHighlighting;
            using (Stream? s = typeof(EditPageView).Assembly.GetManifestResourceStream("ConvnetAvalonia.Resources.Definition.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    DefinitionHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("Definition", new string[] { ".txt" }, DefinitionHighlighting);


            IHighlightingDefinition CSharpHighlighting;
            using (Stream? s = typeof(EditPageView).Assembly.GetManifestResourceStream("ConvnetAvalonia.Resources.CSharp-Mode.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    CSharpHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("C#", new string[] { ".cs" }, CSharpHighlighting);


            InitializeComponent();

            EditorDefinition.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".txt");
            EditorScript.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            //if (e.Property == ValorProperty)
            //    Actualizar();
            base.OnPropertyChanged(e);
        }
        private void GridSplitter_DragCompleted(object? sender, VectorEventArgs e)
        {
            if (!e.Handled)
            {
                Settings.Default.Save();
                e.Handled = true;
            }
        }
    }
}
