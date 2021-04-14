using dnncore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Convnet.Dialogs
{
    public partial class TestParameters : Window
    {
        public DNNTrainingRate Rate { get; set; }
        public Model Model { get; set; }
        public string Path { get; set; }

        public TestParameters()
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
            textBoxBatchSize.Focus();
            radioButtonTestSet.IsChecked = true;
            radioButtonTestSet.IsEnabled = false;
            radioButtonTrainSet.IsChecked = false;
            radioButtonTrainSet.IsEnabled = false;

            radioButtonCubic.IsChecked = Rate.Interpolation == DNNInterpolation.Cubic;
            radioButtonLinear.IsChecked = Rate.Interpolation == DNNInterpolation.Linear;
            radioButtonNearest.IsChecked = Rate.Interpolation == DNNInterpolation.Nearest;
            textBoxColorAngle.IsEnabled = Rate.ColorCast > 0;
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

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid(this))
            {

                if (Model.BatchNormalizationUsed() && Rate.BatchSize == 1)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Your model uses batch normalization.\r\nThe batch size cannot be equal to 1 in this case.", "Warning", MessageBoxButton.OK);
                    return;
                }

                Properties.Settings.Default.TestRate = Rate;
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
            if (radioButtonLinear.IsChecked.HasValue && radioButtonLinear.IsChecked.Value)
                Rate.Interpolation = DNNInterpolation.Linear;

            if (radioButtonNearest.IsChecked.HasValue && radioButtonNearest.IsChecked.Value)
                Rate.Interpolation = DNNInterpolation.Nearest;

            if (radioButtonCubic.IsChecked.HasValue && radioButtonCubic.IsChecked.Value)
                Rate.Interpolation = DNNInterpolation.Cubic;
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
