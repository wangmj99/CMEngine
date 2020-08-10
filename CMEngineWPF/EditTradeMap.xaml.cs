using CMEngineCore;
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
    /// Interaction logic for EditTradeMap.xaml
    /// </summary>
    public partial class EditTradeMap : Window
    {
        private ParentOrder parent;
        private Dictionary<int, TradeMapEntry> dict;
        private int currLvl = -99;


        public EditTradeMap(ParentOrder p)
        {
            InitializeComponent();
            this.parent = p;

            RefreshTM();

            txt_editTM_lvl.Text = ((RollingAlgo)p.Algo).CurrentLevel.ToString();

            lbl_EditTradeMap.Content = String.Format("Parent Order {0}, Ticker {1}, Qty {2}, CurrentLvl {3}", p.ID, p.Symbol, p.Qty, ((RollingAlgo)p.Algo).CurrentLevel);

        }

        private void btn_saveTM_Click(object sender, RoutedEventArgs e)
        {
            //Validate Map

            bool valid = true;

            double totalQty = 0;
            double previousQty = 0;
            double currQty = 0;
            int lvl = int.Parse(txt_editTM_lvl.Text);

            for (int i = 0; i<dict.Keys.Count; i++)
            {
                previousQty = currQty;
                currQty = dict[i].CurrentQty;
                totalQty += currQty;
                 
                if(totalQty > parent.Qty || (i>0 && currQty>0 && previousQty == 0))
                {
                    valid = false;
                    MessageBox.Show(string.Format("Invalid TradeMap level. \r\nParentQty: {0}, TradeMapTotalQty: {1}, \r\nCurrLvlQty: {2}, PreviousLvlQty: {3}", parent.Qty, totalQty,currQty, previousQty ));
                    break;
                }

                if (currQty > 0) lvl = i;

                if(dict[i].TargetSellPrice<dict[i].TargetBuyPrice)
                {
                    valid = false;
                    MessageBox.Show(string.Format("Target Sell Price is less than Target Buy Price. Sell: {0}, Buy: {1}", dict[i].TargetBuyPrice, dict[i].TargetSellPrice));
                    break;
                }

            }

            if (!int.TryParse(txt_editTM_lvl.Text, out currLvl) || currLvl != lvl)
            {
                
                MessageBox.Show(string.Format("Invalid Algo level: {0}, TradeMap Lvl: {1}", txt_editTM_lvl.Text, lvl));
                valid = false;
            }


            //Save Map
            if (valid)
            {
                if (dict != null && dict.Count > 0)
                {
                    TradeMapManager.Instance.UpdateTradeMapByParentID(parent.ID, dict, currLvl);
                }

                StateManager.Save();
                MessageBox.Show("TradeMap is updated.");
            }

        }

        private void btn_closeTM_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
        private void btn_refreshTM_Click(object sender, RoutedEventArgs e)
        {
            RefreshTM();
        }

        private void RefreshTM()
        {
            var algo = (RollingAlgo)parent.Algo;

            dict = new Dictionary<int, TradeMapEntry>();

            foreach(int key in algo.TradeMap.Keys)
            {
                TradeMapEntry o = algo.TradeMap[key];

                TradeMapEntry entry = new TradeMapEntry();
                entry.Level = o.Level;
                entry.TargetBuyPrice = o.TargetBuyPrice;
                entry.TargetSellPrice = o.TargetSellPrice;
                entry.TargetQty = o.TargetQty;
                entry.CurrentQty = o.CurrentQty;
                entry.LastBuyPrice = o.LastBuyPrice;
                entry.WasFilledSellOnPartial = o.WasFilledSellOnPartial;

                dict[key] = entry;
            }

            dg_editTM.ItemsSource = null;
            if (dict != null && dict.Count > 0)
            {
                var list = dict.Values.OrderBy(o => o.Level).ToList();
                dg_editTM.ItemsSource = list;

            }

            currLvl = algo.CurrentLevel;
            txt_editTM_lvl.Text = currLvl.ToString();
        }
    }
}
