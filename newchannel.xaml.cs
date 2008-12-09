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
        string header;
        List<Tibia.Objects.Channel> available;

        public newchannel(List<Tibia.Objects.Channel> AvailableChannels)
        {
            InitializeComponent();
            this.Loaded+=new RoutedEventHandler(newchannel_Loaded);
            GradientStopCollection gsc = new GradientStopCollection();
            gsc.Add(new GradientStop(Color.FromArgb(255, 204, 232, 255), 0.3));
            gsc.Add(new GradientStop(Color.FromArgb(255, 125, 175, 255), 1));
            this.Background = new LinearGradientBrush(gsc);
            chantxt.Focus();
            available = AvailableChannels;
        }

        private void newchannel_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (Tibia.Objects.Channel channel in available)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = channel.Name;
                lbi.MouseDoubleClick += new MouseButtonEventHandler(lbi_MouseDoubleClick);
                channelListBox.Items.Add(lbi);
            }
        }
        public static string ShowBox(List<Tibia.Objects.Channel> AvailableChannels)
        {
            newchannel dialog = new newchannel(AvailableChannels);
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
            if (chantxt.Text.Length>0)
            {
                foreach (object o in channelListBox.Items)
                {
                    if (o.ToString().ToLower() == chantxt.Text.ToString().ToLower())
                    {
                        header = ((ListBoxItem)o).Content.ToString();
                        return;
                    }
                }
                header = chantxt.Text;
            }
        }

        private void lbi_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            TryOpen();
            this.Close();
        }
    }
}
