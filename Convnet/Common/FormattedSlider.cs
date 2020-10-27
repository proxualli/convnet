// Written by Josh Smith - 9/2007
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Convnet.Common
{
    /// <summary>
    /// A Slider which provides a way to modify the 
    /// auto tooltip text by using a format string.
    /// </summary>
    public class FormattedSlider : Slider
    {
        private ToolTip _autoToolTip;

        /// <summary>
        /// Gets/sets a format string used to modify the auto tooltip's content.
        /// Note: This format string must contain exactly one placeholder value,
        /// which is used to hold the tooltip's original content.
        /// </summary>
        public string AutoToolTipFormat { get; set; }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            this.FormatAutoToolTipContent();
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            this.FormatAutoToolTipContent();
        }

        private void FormatAutoToolTipContent()
        {
            if (!string.IsNullOrEmpty(this.AutoToolTipFormat))
            {
                if (this.AutoToolTipFormat == "Priority")
                {
                    switch (this.AutoToolTip.Content.ToString())
                    {
                        case "1":
                            this.AutoToolTip.Content = "Low";
                            break;
                        case "2":
                            this.AutoToolTip.Content = "Below Normal";
                            break;
                        case "3":
                            this.AutoToolTip.Content = "Normal";
                            break;
                        case "4":
                            this.AutoToolTip.Content = "Above Normal";
                            break;
                        case "5":
                            this.AutoToolTip.Content = "High";
                            break;
                        case "6":
                            this.AutoToolTip.Content = "Realtime";
                            break;
                    }
                }
                else if (this.AutoToolTipFormat == "Pixels")
                    this.AutoToolTip.Content = (this.AutoToolTip.Content.ToString() == "1") ? "1 Pixel" : this.AutoToolTip.Content.ToString() + " Pixels";
                else
                    this.AutoToolTip.Content = string.Format(this.AutoToolTipFormat, this.AutoToolTip.Content);
            }
        }

        private ToolTip AutoToolTip
        {
            get
            {
                if (_autoToolTip == null)
                {
                    FieldInfo field = typeof(Slider).GetField(
                        "_autoToolTip",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    _autoToolTip = field.GetValue(this) as ToolTip;
                }

                return _autoToolTip;
            }
        }
    }
}