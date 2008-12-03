using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Tibia_Chat
{
    /// <summary>
    /// Interaction logic for newchannel.xaml
    /// </summary>
    public partial class newchannel : Window
    {
        ObservableCollection<TabItem> tabs;
        string header;
        public newchannel(ObservableCollection<TabItem> tabItemColl)
        {
            InitializeComponent();
            tabs = tabItemColl;
            GradientStopCollection gsc = new GradientStopCollection();
            gsc.Add(new GradientStop(Color.FromArgb(255, 204, 232, 255), 0.3));
            gsc.Add(new GradientStop(Color.FromArgb(255, 125, 175, 255), 1));
            this.Background = new LinearGradientBrush(gsc);
            chantxt.Focus();
        }

        public static string ShowBox(ObservableCollection<TabItem> tabItemColl)
        {
            newchannel dialog = new newchannel(tabItemColl);
            dialog.ShowDialog();
            return dialog.header;
        }

        private void OpenChannel(object sender, RoutedEventArgs e)
        {
            TryOpen();
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SelectionChange(object sender, RoutedEventArgs e)
        {
            chantxt.Text = ((ListBoxItem)channelListBox.SelectedItem).Content.ToString();
        }

        private void KeyDownE(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryOpen();
                this.Close();
            }
        }

        private void TryOpen()
        {
            if (chantxt.Text != string.Empty)
            {
                header = chantxt.Text;
            }
        }
    }
}
