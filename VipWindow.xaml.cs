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

namespace Tibia_Chat
{
    /// <summary>
    /// Interaction logic for vip.xaml
    /// </summary>
    public partial class VipWindow : Window
    {
        public VipWindow()
        {
            InitializeComponent();
            GradientStopCollection gsc = new GradientStopCollection();
            gsc.Add(new GradientStop(Color.FromArgb(255, 204, 232, 255), 0.3));
            gsc.Add(new GradientStop(Color.FromArgb(255, 125, 175, 255), 1));
            this.Background = new LinearGradientBrush(gsc);
        }

        public delegate void DoubleClick(string name);

        public DoubleClick ListBoxItemDoubleClicked; 

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                (System.Windows.Threading.DispatcherOperationCallback)delegate(object o)
            {
                Hide();
                return null;
            }, null);
            e.Cancel = true;
        }

        public void ClearVips()
        {
            this.vipListBox.Items.Clear();
        }

        public void AddVip(Vip vip)
        {
            ListBoxItem lbi=new ListBoxItem();
            lbi.Content=vip.Name;
            lbi.MouseDoubleClick +=new MouseButtonEventHandler(lbi_MouseDoubleClick);
            if (vip.Online)
                lbi.Foreground = new SolidColorBrush(Color.FromRgb(0, 200, 0));
            else
                lbi.Foreground = new SolidColorBrush(Color.FromRgb(230, 0, 0));
            vipListBox.Items.Add(lbi);
        }

        public void RemoveVip(string vipname)
        {
            foreach (ListBoxItem lbi in vipListBox.Items)
                if (lbi.Content.ToString() == vipname) vipListBox.Items.Remove(lbi);
        }

        public void UpdateVip(Vip vip)
        {
            foreach (ListBoxItem lbi in vipListBox.Items)
                if (lbi.Content.ToString() == vip.Name)
                {
                    if (vip.Online)
                        lbi.Foreground = new SolidColorBrush(Color.FromRgb(0, 200, 0));
                    else
                        lbi.Foreground = new SolidColorBrush(Color.FromRgb(230, 0, 0));
                }
        }

        private void vipListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                txtVipname.Text = ((ListBoxItem)vipListBox.SelectedItem).Content.ToString();
            }
            catch { }
        }

        private void lbi_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (ListBoxItemDoubleClicked != null)
                ListBoxItemDoubleClicked(((ListBoxItem)sender).Content.ToString());
        }
    }
}
