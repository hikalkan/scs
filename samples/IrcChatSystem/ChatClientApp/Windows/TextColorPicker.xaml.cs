using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hik.Samples.Scs.IrcChat.Windows
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class TextColorPicker : Window
    {
        public Color SelectedColor { get; private set; }

        public TextColorPicker()
        {
            InitializeComponent();
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedLabel = sender as Label;
            if (selectedLabel == null)
            {
                return;
            }

            var brush = selectedLabel.Foreground as SolidColorBrush;
            if(brush == null)
            {
                return;
            }

            SelectedColor = brush.Color;

            DialogResult = true;
        }
    }
}
