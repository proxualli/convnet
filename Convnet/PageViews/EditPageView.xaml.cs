using Convnet.Properties;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.IO;
using System.Windows.Controls;
using System.Xml;


namespace Convnet.PageViews
{
    public partial class EditPageView : UserControl
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public EditPageView()
        {
            IHighlightingDefinition DefinitionHighlighting;
            using (Stream s = typeof(EditPageView).Assembly.GetManifestResourceStream("Convnet.Resources.Definition.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    DefinitionHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                    HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("Definition", [".txt"], DefinitionHighlighting);
            

            IHighlightingDefinition CSharpHighlighting;
            using (Stream s = typeof(EditPageView).Assembly.GetManifestResourceStream("Convnet.Resources.CSharp-Mode.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    CSharpHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                    HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("C#", [".cs"], CSharpHighlighting);
            
            InitializeComponent();

            EditorDefinition.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".txt");
            EditorScript.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!e.Canceled)
                Settings.Default.Save();
        }
    }
}
