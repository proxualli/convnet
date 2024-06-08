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
            {
                editorDefinition.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".txt");
                editorDefinition.TextChanged += EditorDefinition_TextChanged;
            }


            //IHighlightingDefinition CSharpHighlighting;
            //using (Stream? s = typeof(EditPageView).Assembly.GetManifestResourceStream("ConvnetAvalonia.Resources.CSharp-Mode.xshd"))
            //{
            //    if (s == null)
            //        throw new InvalidOperationException("Could not find embedded resource");
            //    using (XmlReader reader = new XmlTextReader(s))
            //    {
            //        CSharpHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            //    }
            //}
            //HighlightingManager.Instance.RegisterHighlighting("C#", new string[] { ".cs" }, CSharpHighlighting);

            var editorScript = this.FindControl<TextEditor>("EditorScript");
            if (editorScript != null)
            {
                editorScript.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
                editorScript.TextChanged += EditorScript_TextChanged;
                var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
                var textMateInstallation = editorScript.InstallTextMate(registryOptions);
                textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cs").Id));
            }

            var gr = this.FindControl<Grid>("grid");
            if (gr != null)
                gr.ColumnDefinitions.First().Width = new GridLength(Settings.Default.EditSplitPositionA, GridUnitType.Pixel);
        }

        private void EditorDefinition_TextChanged(object? sender, EventArgs e)
        {
            if (DataContext != null && sender != null)
            {
                var epvm = DataContext as EditPageViewModel;
                if (epvm != null)
                    epvm.Definition = ((CodeEditor)sender).Text;
            }
        }

        private void EditorScript_TextChanged(object? sender, EventArgs e)
        {
            if (DataContext != null && sender != null)
            {
                var epvm = DataContext as EditPageViewModel;
                if (epvm != null )
                    epvm.Script = ((CodeEditor)sender).Text;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void GridSplitter_DragCompleted(object? sender, VectorEventArgs e)
        {
            if (!e.Handled)
            {
                var gr = this.FindControl<Grid>("grid");
                if (gr != null)
                { 
                    Settings.Default.EditSplitPositionA = gr.ColumnDefinitions.First().ActualWidth;
                    Settings.Default.Save();
                    e.Handled = true;
                }
            }
        }
    }
}
