using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Material.Icons.Avalonia;

using System.Reflection.Metadata;
using ActiproSoftware.UI.Avalonia.Controls;
using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using SkiaSharp;
using System.Reflection;

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
                if (textView.Bounds.Width >= 32)
                    drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0x50, 0xCF, 0xCF, 0xCF)), null, new Rect(rect.Position, new Size(textView.Bounds.Width - 32, rect.Height)));
            }
        }
    }

  
    public class DefinitionEditor : TextEditor, INotifyPropertyChanged, IStyleable
    {
        Type IStyleable.StyleKey => typeof(AvaloniaEdit.TextEditor);

        public new event PropertyChangedEventHandler? PropertyChanged;

        //AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ScrollChanged));

        public static KeyModifiers GetPlatformCommandKey()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return KeyModifiers.Meta;
            }

            return KeyModifiers.Control;
        }

        public DefinitionEditor()
        {
            //VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            //HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            FontSize = 14;
            FontFamily = new FontFamily("Consolas");
            //FontFamily = new FontFamily("Cascadia Code,Consolas,Menlo,Monospace");
            Options = new TextEditorOptions
            {
                IndentationSize = 4,
                ConvertTabsToSpaces = false,
                AllowScrollBelowDocument = false,
                EnableVirtualSpace = false
            };

            //TextArea.TextView.BackgroundRenderers.Add(new HighlightCurrentLineBackgroundRenderer(this));
            //TextArea.Caret.PositionChanged += (sender, e) => TextArea.TextView.InvalidateLayer(KnownLayer.Background);
            //this.Options.ShowBoxForControlCharacters = true;
            //this.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy(this.Options);
            //this.TextArea.RightClickMovesCaret = true;

            var cmdKey = GetPlatformCommandKey();

            var cm = new Avalonia.Controls.ContextMenu();

            var cut = new Avalonia.Controls.MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, cmdKey) };
            var copy = new Avalonia.Controls.MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, cmdKey) };
            var paste = new Avalonia.Controls.MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, cmdKey) };
            var delete = new Avalonia.Controls.MenuItem { Header = "Delete", InputGesture = new KeyGesture(Key.Delete) };
            var selectall = new Avalonia.Controls.MenuItem { Header = "Select All", InputGesture = new KeyGesture(Key.A, cmdKey) };
            var undo = new Avalonia.Controls.MenuItem { Header = "Undo", InputGesture = new KeyGesture(Key.Z, cmdKey) };
            var redo = new Avalonia.Controls.MenuItem { Header = "Redo", InputGesture = new KeyGesture(Key.Y, cmdKey) };

            //var bitmap = new Bitmap(AssetLoader.Open(new Uri("../Resources/Cut.png", UriKind.Relative)));

            //var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://ConvnetAvalonia/Resources/Cut.png")));
            //var img = new AdvancedImage(new Uri("avares://ConvnetAvalonia/Resources/Cut.png", UriKind.RelativeOrAbsolute));
            //img.Source = new AdvancedImage(new Uri("../Resources/Cut.png", UriKind.RelativeOrAbsolute)).CurrentImage;
            //img.Width = 16;
            //img.Height = 16;
            //img.IsEnabled = true;
            //img.IsVisible = true;
            //using (MemoryStream memoryStream = new MemoryStream(Properties.Resources.Cut))
            //{
            //    cut.Icon = new Bitmap(PixelFormat.Rgba8888, AlphaFormat.Unpremul, Properties.Resources.Cut[0], new PixelSize(16,16), new Vector(96.0, 96.0), 4);
            //}
            //cut.Icon = new Avalonia.Controls.WindowIcon(bitmap);

            cut.Command = ApplicationCommands.Cut;
            paste.Command = ApplicationCommands.Paste;
            copy.Command = ApplicationCommands.Copy;
            delete.Command = ApplicationCommands.Delete;
            selectall.Command = ApplicationCommands.SelectAll;
            undo.Command = ApplicationCommands.Undo;
            redo.Command = ApplicationCommands.Redo;

            cut.Click += (s, e) => { if (CanCut) Dispatcher.UIThread.Post(() => Cut()); };
            paste.Click += (s, e) => { if (CanPaste) Dispatcher.UIThread.Post(() => Paste()); };
            copy.Click += (s, e) => { if (CanCopy) Dispatcher.UIThread.Post(() => Copy()); };
            delete.Click += (s, e) => { if (CanDelete) Dispatcher.UIThread.Post(() => Delete()); };
            selectall.Click += (s, e) => { if (CanSelectAll) Dispatcher.UIThread.Post(() => SelectAll()); };
            undo.Click += (s, e) => { if (CanUndo) Dispatcher.UIThread.Post(() => Undo()); };
            redo.Click += (s, e) => { if (CanRedo) Dispatcher.UIThread.Post(() => Redo()); };

            cm.Items.Add(cut);
            cm.Items.Add(copy);
            cm.Items.Add(paste);
            cm.Items.Add(delete);
            cm.Items.Add(new Avalonia.Controls.Separator());
            cm.Items.Add(selectall);
            cm.Items.Add(new Avalonia.Controls.Separator());
            cm.Items.Add(undo);
            cm.Items.Add(redo);

            ContextMenu = cm;
        }

        //public static readonly StyledProperty<string> DefinitionProperty =  AvaloniaProperty.Register<DefinitionEditor, string>(nameof(Definition), defaultValue: string.Empty, false, Avalonia.Data.BindingMode.TwoWay);

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
                //if (value != Text)
                {
                    Text = value;
                    //Document.Text = value;
                    //AppendText(value);
                    //this.RaisePropertyChanged<string>(DefinitionProperty, Text, value);
                    //base.Text = value;
                    SetValue(DefinitionProperty, value);
                    OnPropertyChanged(nameof(Definition));
                    //OnTextChanged(new EventArgs());
                    //OnPropertyChanged(nameof(Text));
                }
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

        public static readonly StyledProperty<TextLocation> TextLocationProperty = AvaloniaProperty.Register<DefinitionEditor, TextLocation>(nameof(TextLocation), defaultValue: new TextLocation(), false, Avalonia.Data.BindingMode.TwoWay);

        //public static readonly DirectProperty<DefinitionEditor, TextLocation> TextLocationProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, TextLocation>(
        //    nameof(TextLocation),
        //    o => o.TextLocation,
        //    (o, v) =>
        //    {
        //        if (o.TextLocation != v)
        //        {
        //            o.TextArea.Caret.Line = v.Line;
        //            o.TextArea.Caret.Column = v.Column;
        //            o.TextArea.Caret.BringCaretToView();
        //            o.TextArea.Caret.Show();
        //            o.ScrollTo(v.Line, v.Column);
        //            o.TextLocation = v;

        //        }
        //    },
        //    new TextLocation(0, 0),
        //    Avalonia.Data.BindingMode.TwoWay);

        public TextLocation TextLocation
        {
            get { return base.Document.GetLocation(SelectionStart); }
            set
            {
                if (GetValue<TextLocation>(TextLocationProperty) != value)
                {
                    TextArea.Caret.Line = value.Line;
                    TextArea.Caret.Column = value.Column;
                    TextArea.Caret.BringCaretToView();
                    TextArea.Caret.Show();
                    ScrollTo(value.Line, value.Column);
                    TextLocation = value;
                    SetValue(TextLocationProperty, value);
                    OnPropertyChanged(nameof(TextLocation));
                }
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
            set { SetValue<int>(CaretOffsetProperty, value); OnPropertyChanged(nameof(CaretOffset)); }
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
            set { SetValue<int>(SelectionLengthProperty, value); OnPropertyChanged(nameof(SelectionLength)); }
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
            set { SetValue<int>(SelectionStartProperty, value); OnPropertyChanged(nameof(SelectionStart)); }
        }

        public static object? VisualLine { get; private set; }

        public static readonly StyledProperty<string> FilePathProperty = AvaloniaProperty.Register<DefinitionEditor, string>(nameof(FilePath), defaultValue: string.Empty, false, Avalonia.Data.BindingMode.TwoWay);

        //public static readonly DirectProperty<DefinitionEditor, string> FilePathProperty = AvaloniaProperty.RegisterDirect<DefinitionEditor, string>(
        //  nameof(FilePath),
        //  o => o.FilePath,
        //  (o, v) =>
        //  {
        //      if (string.Compare(o.FilePath, v) != 0)
        //          o.FilePath = v;
        //  },
        //  string.Empty,
        //  Avalonia.Data.BindingMode.TwoWay);

        public string FilePath
        {
            get { return GetValue(FilePathProperty); }
            set 
            { 
                SetValue(FilePathProperty, value); 
                OnPropertyChanged(nameof(FilePath));
            }
        }
        
        //public static PropertyChangedCallback OnFilePathChanged { get; private set; }


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

    public class CodeEditor : AvaloniaEdit.TextEditor, INotifyPropertyChanged, IStyleable
    {
        public new event PropertyChangedEventHandler? PropertyChanged;
        Type IStyleable.StyleKey => typeof(AvaloniaEdit.TextEditor);

        public static KeyModifiers GetPlatformCommandKey()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return KeyModifiers.Meta;
            }

            return KeyModifiers.Control;
        }

        public CodeEditor()
        {
            //VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            //HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            FontSize = 14;
            FontFamily = new FontFamily("Consolas");
            //FontFamily = new FontFamily("Cascadia Code,Consolas,Menlo,Monospace");
            Options = new TextEditorOptions
            {
                IndentationSize = 4,
                ConvertTabsToSpaces = false,
                AllowScrollBelowDocument = false,
                EnableVirtualSpace = true
            };

            //TextArea.TextView.BackgroundRenderers.Add(new HighlightCurrentLineBackgroundRenderer(this));
            //TextArea.Caret.PositionChanged += (sender, e) => TextArea.TextView.InvalidateLayer(KnownLayer.Background);

            var cmdKey = GetPlatformCommandKey();

            var cm = new Avalonia.Controls.ContextMenu();

            var cut = new Avalonia.Controls.MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, cmdKey) };
            var copy = new Avalonia.Controls.MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, cmdKey) };
            var paste = new Avalonia.Controls.MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, cmdKey) };
            var delete = new Avalonia.Controls.MenuItem { Header = "Delete", InputGesture = new KeyGesture(Key.Delete) };
            var selectall = new Avalonia.Controls.MenuItem { Header = "Select All", InputGesture = new KeyGesture(Key.A, cmdKey) };
            var undo = new Avalonia.Controls.MenuItem { Header = "Undo", InputGesture = new KeyGesture(Key.Z, cmdKey) };
            var redo = new Avalonia.Controls.MenuItem { Header = "Redo", InputGesture = new KeyGesture(Key.Y, cmdKey) };

            cut.Command = ApplicationCommands.Cut;
            paste.Command = ApplicationCommands.Paste;
            copy.Command = ApplicationCommands.Copy;
            delete.Command = ApplicationCommands.Delete;
            selectall.Command = ApplicationCommands.SelectAll;
            undo.Command = ApplicationCommands.Undo;
            redo.Command = ApplicationCommands.Redo;

            cut.Click += (s, e) => { if (CanCut) Dispatcher.UIThread.Post(() => Cut()); };
            paste.Click += (s, e) => { if (CanPaste) Dispatcher.UIThread.Post(() => Paste()); };
            copy.Click += (s, e) => { if (CanCopy) Dispatcher.UIThread.Post(() => Copy()); };
            delete.Click += (s, e) => { if (CanDelete) Dispatcher.UIThread.Post(() => Delete()); };
            selectall.Click += (s, e) => { if (CanSelectAll) Dispatcher.UIThread.Post(() => SelectAll()); };
            undo.Click += (s, e) => { if (CanUndo) Dispatcher.UIThread.Post(() => Undo()); };
            redo.Click += (s, e) => { if (CanRedo) Dispatcher.UIThread.Post(() => Redo()); };

            cm.Items.Add(cut);
            cm.Items.Add(copy);
            cm.Items.Add(paste);
            cm.Items.Add(delete);
            cm.Items.Add(new Avalonia.Controls.Separator());
            cm.Items.Add(selectall);
            cm.Items.Add(new Avalonia.Controls.Separator());
            cm.Items.Add(undo);
            cm.Items.Add(redo);

            ContextMenu = cm;
        }

        //public static readonly StyledProperty<string> SourceCodeProperty = AvaloniaProperty.Register<CodeEditor, string>(nameof(SourceCode), defaultValue: string.Empty, false, Avalonia.Data.BindingMode.TwoWay);

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
                SetValue(SourceCodeProperty, value);
                OnPropertyChanged(nameof(SourceCode));
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            SetCurrentValue(SourceCodeProperty, base.Text);
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
            set { SetValue(CaretOffsetProperty, value); OnPropertyChanged(nameof(CaretOffset)); }
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
            set { SetValue(SelectionLengthProperty, value); OnPropertyChanged(nameof(SelectionLength)); }
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
            set { SetValue(SelectionStartProperty, value); OnPropertyChanged(nameof(SelectionStart)); }
        }

        public static object? VisualLine { get; private set; }

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
            set { SetValue(FilePathProperty, value); OnPropertyChanged(nameof(FilePath)); }
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
