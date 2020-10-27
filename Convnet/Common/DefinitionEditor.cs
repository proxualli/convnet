using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Convnet.Common
{
    public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        private readonly TextEditor editor;

        public HighlightCurrentLineBackgroundRenderer(TextEditor editor)
        {
            this.editor = editor;
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Background; }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (editor.Document == null)
                return;

            textView.EnsureVisualLines();
            var currentLine = editor.Document.GetLineByOffset(editor.CaretOffset);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                drawingContext.DrawRectangle(
                    new SolidColorBrush(Color.FromArgb(0x50, 0xFF, 0xCF, 0)), null,
                    new Rect(rect.Location, new Size(textView.ActualWidth - 32, rect.Height)));
            }
        }
    }

    public class DefinitionEditor : TextEditor, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DefinitionEditor()
        {
            FontSize = 13;
            FontFamily = new FontFamily("Consolas");
            Options = new TextEditorOptions
            {
                IndentationSize = 1,
                ConvertTabsToSpaces = true
            };

            TextArea.TextView.BackgroundRenderers.Add(new HighlightCurrentLineBackgroundRenderer(this));
            TextArea.Caret.PositionChanged += (sender, e) => TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        }

        public static readonly DependencyProperty DefinitionProperty =
            DependencyProperty.Register("Definition", typeof(string), typeof(DefinitionEditor),
            new PropertyMetadata("", OnDefinitionChanged));

        private static void OnDefinitionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (DefinitionEditor)sender;
            if (string.Compare(control.Definition, e.NewValue.ToString()) != 0)
            {
                //avoid undo stack overflow
                control.Definition = e.NewValue.ToString();
            }
        }

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

        public static readonly DependencyProperty TextLocationProperty =
             DependencyProperty.Register("TextLocation", typeof(TextLocation), typeof(DefinitionEditor),
             new PropertyMetadata((obj, args) =>
             {
                 DefinitionEditor target = (DefinitionEditor)obj;
                 TextLocation loc = (TextLocation)args.NewValue;
                 if (loc != target.TextLocation)
                 {
                     target.TextArea.Caret.Line = loc.Line;
                     target.TextArea.Caret.Column = loc.Column;
                     target.TextArea.Caret.BringCaretToView();
                     target.TextArea.Caret.Show();
                     target.ScrollTo(loc.Line, loc.Column);
                     target.TextLocation = loc;
                 }
             }));

        public TextLocation TextLocation
        {
            get { return base.Document.GetLocation(SelectionStart); }
            set
            {
                SetValue(TextLocationProperty, value);
                OnPropertyChanged(nameof(TextLocation));
            }
        }

        public static DependencyProperty CaretOffsetProperty =
            DependencyProperty.Register("CaretOffset", typeof(int), typeof(DefinitionEditor),
            new PropertyMetadata((obj, args) =>
            {
                DefinitionEditor target = (DefinitionEditor)obj;
                if (target.CaretOffset != (int)args.NewValue)
                    target.CaretOffset = (int)args.NewValue;
            }));

        public new int CaretOffset
        {
            get { return base.CaretOffset; }
            set { SetValue(CaretOffsetProperty, value); }
        }

        public static readonly DependencyProperty SelectionLengthProperty =
             DependencyProperty.Register("SelectionLength", typeof(int), typeof(DefinitionEditor),
             new PropertyMetadata((obj, args) =>
             {
                 DefinitionEditor target = (DefinitionEditor)obj;
                 if (target.SelectionLength != (int)args.NewValue)
                 {
                     target.SelectionLength = (int)args.NewValue;
                     target.Select(target.SelectionStart, (int)args.NewValue);
                 }
             }));

        public new int SelectionLength
        {
            get { return base.SelectionLength; }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public static readonly DependencyProperty SelectionStartProperty =
             DependencyProperty.Register("SelectionStart", typeof(int), typeof(DefinitionEditor),
             new PropertyMetadata((obj, args) =>
             {
                 DefinitionEditor target = (DefinitionEditor)obj;
                 if (target.SelectionStart != (int)args.NewValue)
                 {
                     target.SelectionStart = (int)args.NewValue;
                     target.Select((int)args.NewValue, target.SelectionLength);
                 }
             }));

        public static object VisualLine { get; private set; }

        public new int SelectionStart
        {
            get { return base.SelectionStart; }
            set { SetValue(SelectionStartProperty, value); }
        }

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static PropertyChangedCallback OnFilePathChanged { get; private set; }

        // Using a DependencyProperty as the backing store for FilePath. 
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(DefinitionEditor), new PropertyMetadata(String.Empty, OnFilePathChanged));

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

    public class CodeEditor : TextEditor, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        public static readonly DependencyProperty SourceCodeProperty = DependencyProperty.Register("SourceCode", typeof(string), typeof(CodeEditor), new PropertyMetadata("", OnSourceCodeChanged));

        private static void OnSourceCodeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (CodeEditor)sender;
            if (string.Compare(control.SourceCode, e.NewValue.ToString()) != 0)
            {
                //avoid undo stack overflow
                control.SourceCode = e.NewValue.ToString();
            }
        }

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

        public static readonly DependencyProperty TextLocationProperty =
             DependencyProperty.Register("TextLocation", typeof(TextLocation), typeof(CodeEditor),
             new PropertyMetadata((obj, args) =>
             {
                 CodeEditor target = (CodeEditor)obj;
                 TextLocation loc = (TextLocation)args.NewValue;
                 if (loc != target.TextLocation)
                 {
                     target.TextArea.Caret.Line = loc.Line;
                     target.TextArea.Caret.Column = loc.Column;
                     target.TextArea.Caret.BringCaretToView();
                     target.TextArea.Caret.Show();
                     target.ScrollTo(loc.Line, loc.Column);
                     target.TextLocation = loc;
                 }
             }));

        public TextLocation TextLocation
        {
            get { return base.Document.GetLocation(SelectionStart); }
            set
            {
                SetValue(TextLocationProperty, value);
                OnPropertyChanged(nameof(TextLocation));
            }
        }

        public static DependencyProperty CaretOffsetProperty =
            DependencyProperty.Register("CaretOffset", typeof(int), typeof(CodeEditor),
            new PropertyMetadata((obj, args) =>
            {
                CodeEditor target = (CodeEditor)obj;
                if (target.CaretOffset != (int)args.NewValue)
                    target.CaretOffset = (int)args.NewValue;
            }));

        public new int CaretOffset
        {
            get { return base.CaretOffset; }
            set { SetValue(CaretOffsetProperty, value); }
        }

        public static readonly DependencyProperty SelectionLengthProperty =
             DependencyProperty.Register("SelectionLength", typeof(int), typeof(CodeEditor),
             new PropertyMetadata((obj, args) =>
             {
                 CodeEditor target = (CodeEditor)obj;
                 if (target.SelectionLength != (int)args.NewValue)
                 {
                     target.SelectionLength = (int)args.NewValue;
                     target.Select(target.SelectionStart, (int)args.NewValue);
                 }
             }));

        public new int SelectionLength
        {
            get { return base.SelectionLength; }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public static readonly DependencyProperty SelectionStartProperty =
             DependencyProperty.Register("SelectionStart", typeof(int), typeof(CodeEditor),
             new PropertyMetadata((obj, args) =>
             {
                 CodeEditor target = (CodeEditor)obj;
                 if (target.SelectionStart != (int)args.NewValue)
                 {
                     target.SelectionStart = (int)args.NewValue;
                     target.Select((int)args.NewValue, target.SelectionLength);
                 }
             }));

        public static object VisualLine { get; private set; }

        public new int SelectionStart
        {
            get { return base.SelectionStart; }
            set { SetValue(SelectionStartProperty, value); }
        }

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static PropertyChangedCallback OnFilePathChanged { get; private set; }

        // Using a DependencyProperty as the backing store for FilePath. 
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilePathProperty =
             DependencyProperty.Register("FilePath", typeof(string), typeof(CodeEditor),
             new PropertyMetadata(String.Empty, OnFilePathChanged));


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
