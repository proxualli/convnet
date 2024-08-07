﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.TextMate;
using Convnet.Common;
using Convnet.PageViewModels;
using Convnet.Properties;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using TextMateSharp.Grammars;

namespace Convnet.PageViews
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
            HighlightingManager.Instance.RegisterHighlighting("Definition", [".txt"], DefinitionHighlighting);
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
            //HighlightingManager.Instance.RegisterHighlighting("C#", [".cs"], CSharpHighlighting);
            //var editorScript = this.FindControl<TextEditor>("EditorScript");
            //if (editorScript != null)
            //{
            //    editorScript.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
            //    editorScript.TextChanged += EditorScript_TextChanged;
            //}

            var editorScript = this.FindControl<TextEditor>("EditorScript");
            if (editorScript != null)
            {
                editorScript.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
                editorScript.TextChanged += EditorScript_TextChanged;

                var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
                var textMateInstallation = editorScript.InstallTextMate(registryOptions);
                var csharpLanguage = registryOptions.GetLanguageByExtension(".cs");
                textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(csharpLanguage.Id));
            }

            var gr = this.FindControl<Grid>("grid");
            if (gr != null)
                gr.ColumnDefinitions.First().Width = new GridLength(Settings.Default.EditSplitPositionA, GridUnitType.Pixel);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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

        //private void TextMateInstallationOnAppliedTheme(object sender, TextMate.Installation e)
        //{
        //    ApplyThemeColorsToEditor(e);
        //    //ApplyThemeColorsToWindow(e);
        //}

        //void ApplyThemeColorsToEditor(TextMate.Installation e)
        //{
        //    var editorScript = this.FindControl<TextEditor>("EditorScript");
        //    if (editorScript != null)
        //    {
        //        ApplyBrushAction(e, "editor.background", brush => editorScript.Background = brush);
        //        ApplyBrushAction(e, "editor.foreground", brush => editorScript.Foreground = brush);

        //        if (!ApplyBrushAction(e, "editor.selectionBackground",
        //                brush => editorScript.TextArea.SelectionBrush = brush))
        //        {
        //            if (App.Current!.TryGetResource("TextAreaSelectionBrush", out var resourceObject))
        //            {
        //                if (resourceObject is IBrush brush)
        //                {
        //                    editorScript.TextArea.SelectionBrush = brush;
        //                }
        //            }
        //        }

        //        if (!ApplyBrushAction(e, "editor.lineHighlightBackground",
        //                brush =>
        //                {
        //                    editorScript.TextArea.TextView.CurrentLineBackground = brush;
        //                    editorScript.TextArea.TextView.CurrentLineBorder = new Pen(brush); // Todo: VS Code didn't seem to have a border but it might be nice to have that option. For now just make it the same..
        //                }))
        //        {
        //            editorScript.TextArea.TextView.SetDefaultHighlightLineColors();
        //        }

        //        //Todo: looks like the margin doesn't have a active line highlight, would be a nice addition
        //        if (!ApplyBrushAction(e, "editorLineNumber.foreground",
        //                brush => editorScript.LineNumbersForeground = brush))
        //        {
        //            editorScript.LineNumbersForeground = editorScript.Foreground;
        //        }
        //    }
        //}

        //bool ApplyBrushAction(TextMate.Installation e, string colorKeyNameFromJson, Action<IBrush> applyColorAction)
        //{
        //    if (!e.TryGetThemeColor(colorKeyNameFromJson, out var colorString))
        //        return false;

        //    if (!Color.TryParse(colorString, out Color color))
        //        return false;

        //    var colorBrush = new SolidColorBrush(color);
        //    applyColorAction(colorBrush);
        //    return true;
        //}
    }
}
