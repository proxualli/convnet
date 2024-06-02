using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;


namespace ConvnetAvalonia.Common
{
    public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        private readonly TextEditor editor;

        public HighlightCurrentLineBackgroundRenderer(TextEditor editor)
        {
            this.editor = editor;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public KnownLayer Layer
        {
            get { return KnownLayer.Background; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (editor.Document == null)
                return;

            textView.EnsureVisualLines();
            var currentLine = editor.Document.GetLineByOffset(editor.CaretOffset);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                if (textView.Width >= 32)
                    drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0x50, 0xCF, 0xCF, 0xCF)), null, new Rect(rect.Position, new Size(textView.Width - 32, rect.Height)));
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public class DefinitionEditor : TextEditor, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public DefinitionEditor()
        {
            FontSize = 13;
            FontFamily = new FontFamily("Consolas");
            Options = new TextEditorOptions
            {
                IndentationSize = 4,
                ConvertTabsToSpaces = false
            };

            TextArea.TextView.BackgroundRenderers.Add(new HighlightCurrentLineBackgroundRenderer(this));
            TextArea.Caret.PositionChanged += (sender, e) => TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        }

        public static readonly DirectProperty<DefinitionEditor, string> DefinitionProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, string>(
            nameof(Definition), 
            o => o.Definition, 
            (o, v) => 
            {
                if (string.Compare(o.Definition, v) != 0)
                    o.Definition = v; 
            }, 
            "", 
            Avalonia.Data.BindingMode.TwoWay);

        public string Definition
        {
            get { return Text; }
            set
            {
                Text = value;
                OnPropertyChanged(nameof(Definition));
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            SetCurrentValue(DefinitionProperty, Text);
            OnPropertyChanged(nameof(Length));
            base.OnTextChanged(e);
        }

        public int Length
        {
            get { return base.Text.Length; }
        }

        public static readonly DirectProperty<DefinitionEditor, TextLocation> TextLocationProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, TextLocation>(
            nameof(TextLocation),
            o => o.TextLocation,
            (o, v) =>
            {
                if (o.TextLocation != v)
                {
                    o.TextArea.Caret.Line = v.Line;
                    o.TextArea.Caret.Column = v.Column;
                    o.TextArea.Caret.BringCaretToView();
                    o.TextArea.Caret.Show();
                    o.ScrollTo(v.Line, v.Column);
                    o.TextLocation = v;

                }
            },
            new TextLocation(0,0), 
            Avalonia.Data.BindingMode.TwoWay);

        public TextLocation TextLocation
        {
            get { return base.Document.GetLocation(SelectionStart); }
            set
            {
                SetValue(TextLocationProperty, value);
                OnPropertyChanged(nameof(TextLocation));
            }
        }

        public static readonly DirectProperty<DefinitionEditor, int> CaretOffsetProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, int>(
            nameof(CaretOffset),
            o => o.CaretOffset,
            (o, v) =>
            {
                if (o.CaretOffset != v)
                    o.CaretOffset = v;
            },
            0,
            Avalonia.Data.BindingMode.TwoWay);

        public new int CaretOffset
        {
            get { return base.CaretOffset; }
            set { SetValue(CaretOffsetProperty, value); }
        }

        public static readonly DirectProperty<DefinitionEditor, int> SelectionLengthProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, int>(
            nameof(SelectionLength),
            o => o.SelectionLength,
            (o, v) =>
            {
                if (o.SelectionLength != v)
                    o.SelectionLength = v;
            },
            0,
            Avalonia.Data.BindingMode.TwoWay);

        public new int SelectionLength
        {
            get { return base.SelectionLength; }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public static readonly DirectProperty<DefinitionEditor, int> SelectionStartProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, int>(
            nameof(SelectionStart),
            o => o.SelectionStart,
            (o, v) =>
            {
                if (o.SelectionStart != v)
                    o.SelectionStart = v;
            },
            0,
            Avalonia.Data.BindingMode.TwoWay);

        public new int SelectionStart
        {
            get { return base.SelectionStart; }
            set { SetValue(SelectionStartProperty, value); }
        }
        
        public static readonly DirectProperty<DefinitionEditor, string> FilePathProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, string>(
           nameof(FilePath),
           o => o.FilePath,
           (o, v) =>
           {
               if (string.Compare(o.FilePath, v) != 0)
                   o.FilePath = v;
           },
           "",
           Avalonia.Data.BindingMode.TwoWay);

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        //public static PropertyChangedCallback OnFilePathChanged { get; private set; }

     
        public static object VisualLine { get; private set; }

        #region INotifyPropertyChanged Members

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // If you raise PropertyChanged and do not specify a property name,
            // all properties on the object are considered to be changed by the binding system.
            if (String.IsNullOrEmpty(propertyName))
                return;

            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new ArgumentException(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #endregion // INotifyPropertyChanged Members
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public class CodeEditor : TextEditor, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public CodeEditor()
        {
            FontSize = 13;
            FontFamily = new FontFamily("Consolas");
            Options = new TextEditorOptions
            {
                IndentationSize = 4,
                ConvertTabsToSpaces = false
            };

            TextArea.TextView.BackgroundRenderers.Add(new HighlightCurrentLineBackgroundRenderer(this));
            TextArea.Caret.PositionChanged += (sender, e) => TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        }

        public static readonly DirectProperty<CodeEditor, string> SourceCodeProperty = AvaloniaProperty.RegisterDirect<CodeEditor, string>(
            nameof(SourceCode),
            o => o.SourceCode,
            (o, v) =>
            {
                if (string.Compare(o.SourceCode, v) != 0)
                    o.SourceCode = v;
            },
            "",
            Avalonia.Data.BindingMode.TwoWay);

        public string SourceCode
        {
            get { return Text; }
            set
            {
                Text = value;
                OnPropertyChanged(nameof(SourceCode));
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            SetCurrentValue(SourceCodeProperty, Text);
            OnPropertyChanged(nameof(Length));
            base.OnTextChanged(e);
        }

        public int Length
        {
            get { return base.Text.Length; }
        }

        public static readonly DirectProperty<CodeEditor, TextLocation> TextLocationProperty = AvaloniaProperty.RegisterDirect<CodeEditor, TextLocation>(
           nameof(TextLocation),
           o => o.TextLocation,
           (o, v) =>
           {
               if (o.TextLocation != v)
               {
                   o.TextArea.Caret.Line = v.Line;
                   o.TextArea.Caret.Column = v.Column;
                   o.TextArea.Caret.BringCaretToView();
                   o.TextArea.Caret.Show();
                   o.ScrollTo(v.Line, v.Column);
                   o.TextLocation = v;

               }
           },
           new TextLocation(0, 0),
           Avalonia.Data.BindingMode.TwoWay);

        public TextLocation TextLocation
        {
            get { return base.Document.GetLocation(SelectionStart); }
            set
            {
                SetValue(TextLocationProperty, value);
                OnPropertyChanged(nameof(TextLocation));
            }
        }

        public static readonly DirectProperty<CodeEditor, int> CaretOffsetProperty = AvaloniaProperty.RegisterDirect<CodeEditor, int>(
              nameof(CaretOffset),
              o => o.CaretOffset,
              (o, v) =>
              {
                  if (o.CaretOffset != v)
                      o.CaretOffset = v;
              },
              0,
              Avalonia.Data.BindingMode.TwoWay);

        public new int CaretOffset
        {
            get { return base.CaretOffset; }
            set { SetValue(CaretOffsetProperty, value); }
        }

        public static readonly DirectProperty<CodeEditor, int> SelectionLengthProperty = AvaloniaProperty.RegisterDirect<CodeEditor, int>(
            nameof(SelectionLength),
            o => o.SelectionLength,
            (o, v) =>
            {
                if (o.SelectionLength != v)
                    o.SelectionLength = v;
            },
            0,
            Avalonia.Data.BindingMode.TwoWay);

        public new int SelectionLength
        {
            get { return base.SelectionLength; }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public static readonly DirectProperty<CodeEditor, int> SelectionStartProperty = AvaloniaProperty.RegisterDirect<CodeEditor, int>(
            nameof(SelectionStart),
            o => o.SelectionStart,
            (o, v) =>
            {
                if (o.SelectionStart != v)
                    o.SelectionStart = v;
            },
            0,
            Avalonia.Data.BindingMode.TwoWay);

        public new int SelectionStart
        {
            get { return base.SelectionStart; }
            set { SetValue(SelectionStartProperty, value); }
        }

        public static object VisualLine { get; private set; }

        public static readonly DirectProperty<CodeEditor, string> FilePathProperty = AvaloniaProperty.RegisterDirect<CodeEditor, string>(
           nameof(FilePath),
           o => o.FilePath,
           (o, v) =>
           {
               if (string.Compare(o.FilePath, v) != 0)
                   o.FilePath = v;
           },
           "",
           Avalonia.Data.BindingMode.TwoWay);

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }


        #region INotifyPropertyChanged Members

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // If you raise PropertyChanged and do not specify a property name,
            // all properties on the object are considered to be changed by the binding system.
            if (String.IsNullOrEmpty(propertyName))
                return;

            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new ArgumentException(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #endregion // INotifyPropertyChanged Members
    }
}
