using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using ConvnetAvalonia.Properties;
using System;
using System.IO;
using System.Xml;
using TextMateSharp.Grammars;

namespace ConvnetAvalonia.PageViews
{
    public partial class EditPageView : UserControl
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public EditPageView()
        {
            //string[] names = this.GetType().Assembly.GetManifestResourceNames();
            //string[] anames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
           
            InitializeComponent();

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
            var editorDefinition = this.FindControl<TextEditor>("EditorDefinition");
            if (editorDefinition != null)
                editorDefinition.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".txt");

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
            var editorScript = this.FindControl<TextEditor>("EditorScript");
            if (editorScript != null)
                editorScript.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");

            // var textEditor = this.FindControl<TextEditor>("EditorScript");
            // var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
            // var textMateInstallation = editorScript.InstallTextMate(_registryOptions);
            // textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        //protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        //{

        //    //if (e.Property == ValorProperty)
        //    //    Actualizar();
        //    base.OnPropertyChanged(e);
        //}

        public void GridSplitter_DragCompleted(object? sender, VectorEventArgs e)
        {
            //if (!e.Handled)
            {
                Settings.Default.Save();
                e.Handled = true;
            }
        }
    }
}
