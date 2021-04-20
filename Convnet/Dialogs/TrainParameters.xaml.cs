using dnncore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Convnet.Dialogs
{
    public partial class TrainParameters : Window
    {
        public static IEnumerable<DNNOptimizers> GetOptimizers => Enum.GetValues(typeof(DNNOptimizers)).Cast<DNNOptimizers>();
        public static IEnumerable<DNNInterpolation> GetInterpolations => Enum.GetValues(typeof(DNNInterpolation)).Cast<DNNInterpolation>();
        public DNNTrainingRate Rate { get; set; }
        public Model Model { get; set; }
        public string Path { get; set; }

        public TrainParameters()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
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

            textBoxGoToEpoch.Text = Properties.Settings.Default.GoToEpoch.ToString();
            textBoxColorAngle.IsEnabled = Rate.ColorCast > 0;

            comboBoOptimizer.Focus();
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

        void ButtonTrain_Click(object sender, RoutedEventArgs e)
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
                Properties.Settings.Default.Optimizer = (int)Rate.Optimizer;
                Properties.Settings.Default.Save();
               
                DialogResult = true;
                Close();
            }
        }

        void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            /*
            uint.TryParse(textBoxGoToEpoch.Text, out uint gotoEpoch);
            if (gotoEpoch > Rate.Epochs || gotoEpoch < 1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Your value for go to Epochs is incorrect.", "Warning", MessageBoxButton.OK);
                return;
            }
           
            Properties.Settings.Default.GoToEpoch = gotoEpoch;
            Properties.Settings.Default.TrainRate = Rate;
            Properties.Settings.Default.Optimizer = (int)Rate.Optimizer;
            Properties.Settings.Default.Save();
            */

            DialogResult = false;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            buttonCancel.Focus();
        }

        void TextBoxDistortions_TextChanged(object sender, TextChangedEventArgs e)
        {
            var enabled = (float.TryParse(textBoxDistortions.Text, out float result) && result > 0.0f);

            comboBoInterpolation.IsEnabled = enabled;
            textBoxRotation.IsEnabled = enabled;
            textBoxScaling.IsEnabled = enabled;
        }

        void TextBoxColorCast_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxColorAngle.IsEnabled = (float.TryParse(textBoxColorCast.Text, out float result) && result > 0.0f);
        }

        void comboBoOptimizer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBoOptimizer.SelectedIndex)
            {
                case 6: // DNNOptimizers.SGD:
                    {
                        textBlockL2penalty.Opacity = 1;
                        textBoxL2penalty.IsEnabled = true;
                        textBlockMomentum.Opacity = 0.5;
                        textBoxMomentum.IsEnabled = false;
                    }
                    break;

                case 0: // DNNOptimizers.AdaDelta:
                case 2: // DNNOptimizers.Adam:
                case 3: // DNNOptimizers.Adamax:
                case 5: // DNNOptimizers.RMSProp:
                    {
                        textBlockMomentum.Opacity = 1;
                        textBoxMomentum.IsEnabled = true;
                        textBlockL2penalty.Opacity = 0.5;
                        textBoxL2penalty.IsEnabled = false;
  
                    }
                    break;
                case 1: // DNNOptimizers.AdaGrad:
                    {
                        textBlockL2penalty.Opacity = 0.5;
                        textBoxL2penalty.IsEnabled = false;
                        textBlockMomentum.Opacity = 0.5;
                        textBoxMomentum.IsEnabled = false;
                    }
                    break;

                case 4: // DNNOptimizers.NAG:
                case 7: // DNNOptimizers.SGDMomentum:
                    {
                        textBlockL2penalty.Opacity = 1;
                        textBoxL2penalty.IsEnabled = true;
                        textBlockMomentum.Opacity = 1;
                        textBoxMomentum.IsEnabled = true;
                    }
                    break;
            }

            switch (comboBoOptimizer.SelectedIndex)
            {
                case 0: // DNNOptimizers.AdaDelta:
                case 1: // DNNOptimizers.AdaGrad:
                case 4: // DNNOptimizers.NAG:
                case 6: // DNNOptimizers.SGD:
                case 7: // DNNOptimizers.SGDMomentum:
                    {
                        textBlockBeta2.Opacity = 0.5;
                        textBoxBeta2.IsEnabled = false;
                    }
                    break;

                case 2: // DNNOptimizers.Adam:
                    {
                        textBlockBeta2.Opacity = 1;
                        textBoxBeta2.IsEnabled = true;
                    }
                    break;

                case 3: // DNNOptimizers.Adamax:
                case 5: // DNNOptimizers.RMSProp:
                    {
                        textBlockBeta2.Opacity = 1;
                        textBoxBeta2.IsEnabled = true;
                    }
                    break;
            }
        }
    }
}
