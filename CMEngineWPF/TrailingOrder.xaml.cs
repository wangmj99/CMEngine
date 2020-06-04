using CMEngineCore;
using CMEngineCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CMEngineWPF
{
    /// <summary>
    /// Interaction logic for TrailingOrder.xaml
    /// </summary>
    public partial class TrailingOrder : Window
    {
        public ParentOrder parent;

        public TrailingOrder(ParentOrder p)
        {
            InitializeComponent();
            this.parent = p;

            lbl_Place_TrailOrder.Content = String.Format("Sell Parent {0}, Ticker {1}, Qty {2}",
                                p.ID, p.Symbol, p.Qty);

        }

        private void btn_Trail_OK_Click(object sender, RoutedEventArgs e)
        {
            double price = Convert.ToDouble(txt_TrailPx.Text);
            double pct = Convert.ToDouble(txt_TrailPct.Text);

            TradeManager.Instance.PlaceTrailStopOrder(parent.ID, TradeType.Sell, parent.Symbol, parent.Qty, price, pct);

            MessageBox.Show("Trailing order placed!");

            this.Close();
        }

        private void btn_Trail_Cxl_Click(object sender, RoutedEventArgs e)
        {
            //Close dialog
            this.Close();
        }
    }
}
