using dnncore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Convnet.Dialogs
{
    public partial class TrainParameters : Window
    {
        public DNNTrainingRate Rate { get; set; }
        public Model Model { get; set; }
        public string Path { get; set; }

        public TrainParameters()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (Model.Dataset)
            {
                case DNNDatasets.cifar10:
                case DNNDatasets.cifar100:
                case DNNDatasets.tinyimagenet:
                    break;

                case DNNDatasets.fashionmnist:
                case DNNDatasets.mnist:
                    Rate.AutoAugment = 0.0f;
                    Rate.ColorCast = 0;
                    Rate.ColorAngle = 0;
                    textBoxAutoAugment.IsEnabled = false;
                    textBoxColorCast.IsEnabled = false;
                    textBoxColorAngle.IsEnabled = false;
                    break;
            }

            DataContext = Rate;

            comboBoOptimizer.SelectedIndex = Properties.Settings.Default.Optimizer;
            comboBoOptimizer.Focus();
            radioButtonCubic.IsChecked = Rate.Interpolation == (uint)DNNInterpolation.Cubic;
            radioButtonLinear.IsChecked = Rate.Interpolation == (uint)DNNInterpolation.Linear;
            radioButtonNearest.IsChecked = Rate.Interpolation == (uint)DNNInterpolation.Nearest;
            textBoxGoToEpoch.Text = Properties.Settings.Default.GoToEpoch.ToString();
            textBoxColorAngle.IsEnabled = Rate.ColorCast > 0;

            //switch (Model.Optimizer)
            //{
            //    case DNNOptimizers.SGD:
            //        {
            //            textBlockL2penalty.Opacity = 1;
            //            textBoxL2penalty.IsEnabled = true;
            //            textBlockMomentum.Opacity = 0.5;
            //            textBoxMomentum.IsEnabled = false;
            //        }
            //        break;

            //    case DNNOptimizers.AdaDelta:
            //    case DNNOptimizers.Adam:
            //    case DNNOptimizers.Adamax:
            //    case DNNOptimizers.RMSProp:
            //        {
            //            textBlockL2penalty.Opacity = 0.5;
            //            textBoxL2penalty.IsEnabled = false;
            //            textBlockMomentum.Opacity = 1;
            //            textBoxMomentum.IsEnabled = true;
            //        }
            //        break;
            //    case DNNOptimizers.AdaGrad:
            //        {
            //            textBlockL2penalty.Opacity = 0.5;
            //            textBoxL2penalty.IsEnabled = false;
            //            textBlockMomentum.Opacity = 0.5;
            //            textBoxMomentum.IsEnabled = false;
            //        }
            //        break;

            //    case DNNOptimizers.NAG:
            //    case DNNOptimizers.SGDMomentum:
            //    case DNNOptimizers.RAdam:
            //        {
            //            textBlockL2penalty.Opacity = 1;
            //            textBoxL2penalty.IsEnabled = true;
            //            textBlockMomentum.Opacity = 1;
            //            textBoxMomentum.IsEnabled = true;
            //        }
            //        break;
            //}
        }

        bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached 
                bool isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    // If a child dependency object is invalid, return false immediately,
                    // otherwise keep checking
                    if (IsValid((DependencyObject)subnode) == false) return false;
                }

            }

            // All dependency objects are valid
            return true;
        }

        private void ButtonTrain_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid(this))
            {
                if (Model.BatchNormalizationUsed() && Rate.BatchSize == 1)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Your model uses batch normalization.\r\nThe batch size cannot be equal to 1 in this case.", "Warning", MessageBoxButton.OK);
                    return;
                }

                uint.TryParse(textBoxGoToEpoch.Text, out uint gotoEpoch);
                if (gotoEpoch > Rate.Epochs || gotoEpoch < 1)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Your value for go to Epochs is incorrect.", "Warning", MessageBoxButton.OK);
                    return;
                }

                Properties.Settings.Default.GoToEpoch = gotoEpoch;
                Properties.Settings.Default.TrainRate = Rate;
                Properties.Settings.Default.Save();

                DialogResult = true;
                Close();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            buttonCancel.Focus();
        }

        private void RadioButtonInterpolation_Checked(object sender, RoutedEventArgs e)
        {
            if (radioButtonCubic.IsChecked.HasValue && radioButtonCubic.IsChecked.Value)
                Rate.Interpolation = (uint)DNNInterpolation.Cubic;

            if (radioButtonLinear.IsChecked.HasValue && radioButtonLinear.IsChecked.Value)
                Rate.Interpolation = (uint)DNNInterpolation.Linear;

            if (radioButtonNearest.IsChecked.HasValue && radioButtonNearest.IsChecked.Value)
                Rate.Interpolation = (uint)DNNInterpolation.Nearest;
        }

        private void TextBoxDistortions_TextChanged(object sender, TextChangedEventArgs e)
        {
            var enabled = (float.TryParse(textBoxDistortions.Text, out float result) && result > 0.0f);

            radioButtonCubic.IsEnabled = enabled;
            radioButtonLinear.IsEnabled = enabled;
            radioButtonNearest.IsEnabled = enabled;
            textBoxRotation.IsEnabled = enabled;
            textBoxScaling.IsEnabled = enabled;
        }

        private void TextBoxColorCast_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxColorAngle.IsEnabled = (float.TryParse(textBoxColorCast.Text, out float result) && result > 0.0f);
        }
    }
}
