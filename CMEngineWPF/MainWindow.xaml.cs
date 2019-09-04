using CMEngineCore;
using CMEngineCore.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMEngineWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            Log.Info("**********Algo trading engine start************");
            InitializeComponent();
        }

        private void btn_conn_Click(object sender, RoutedEventArgs e)
        {
            if (!TradeManager.Instance.IsConnected)
            {
                try
                {
                    string ip = ConfigurationManager.AppSettings["IBIPAddress"];
                    int port = int.Parse(ConfigurationManager.AppSettings["IBPort"]);
                    TradeManager.Instance.Init(Broker.IB);
                    TradeManager.Instance.Connect(ip, port, 1);
                    MessageBox.Show("IB is connected");
                }
                catch(Exception ex)
                {
                    Log.Error("Failed to Connect to IB. Message: "+ ex.Message);
                    Log.Error(ex.StackTrace);
                    MessageBox.Show("Failed to Connect to IB.Message: "+ ex.Message);
                }
            }else
            {
                Log.Info("IB is already connected");
                MessageBox.Show("IB is already connected");
            }
        }

        private void btn_disconn_Click(object sender, RoutedEventArgs e)
        {
            if (TradeManager.Instance.IsConnected)
            {
                try
                {
                    TradeManager.Instance.Disconnect();
                    Log.Info("IB is disconnected");
                    MessageBox.Show("IB is disconnected");

                }
                catch(Exception ex)
                {
                    Log.Error("Disconnect IB error. Message: " + ex.Message);
                    Log.Error(ex.StackTrace);
                    MessageBox.Show("Disconnect IB error. Message: " + ex.Message);
                }
            }
        }

        private void btn_resume_Click(object sender, RoutedEventArgs e)
        {
            //if (!TradeManager.Instance.IsConnected)
            //{
            //    Log.Error("IB is not connected, please reconnect it first");
            //    MessageBox.Show("IB is not connected, please reconnect it first");
            //    return;
            //}

            StateManager.Resume();

            Log.Info(string.Format("Request Global cancel for all orders on resume"));

            TradeManager.Instance.RequestGlobalCancel();
            Thread.Sleep(3000);
            ParentOrderManager.Instance.SetAllOrdertoCancelStatus();

            ParentOrderManager.Instance.Start();
            MessageBox.Show("System is resumed.");
        }

        private void btn_submit_Click(object sender, RoutedEventArgs e)
        {
            if (!TradeManager.Instance.IsConnected)
            {
                MessageBox.Show("IB is not connected, please reconnect first!");
                Log.Error("IB is not connected, please reconnect first");
                return;
            }

            List<ITradeRule> rules = new List<ITradeRule>() { new RollingAlgoBuyRule(), new RollingAlgoSellRule() };
            RollingAlgo algo = null;
            string symbol;
            double beginPrice;

            try
            {
                symbol = txt_symbol.Text.Trim().ToUpper();
                beginPrice = Convert.ToDouble(txt_price.Text);
                double scaleFactor = Convert.ToDouble(txt_scaleFactor.Text);
                int scaleLevel = Convert.ToInt32(txt_scalelvl.Text);
                double shareOrDollarAmt = Convert.ToDouble(txt_shareAmt.Text);
                bool isPctScaleFactor = chk_scaleFactor.IsChecked.Value;
                bool isShare = chk_isShare.IsChecked.Value;
                bool buyBackLvlZero = chk_buyback.IsChecked.Value;


                if(beginPrice<=0 || scaleLevel<=0 || scaleFactor<=0 || (isPctScaleFactor && scaleFactor >= 100))
                {
                    MessageBox.Show("Invalid price, scale inputs. scale factor pct must be between 0% and 100%");
                    return;
                }

                algo = new RollingAlgo(beginPrice, scaleLevel, scaleFactor, isPctScaleFactor, shareOrDollarAmt, isShare, buyBackLvlZero);


                //Establish rules
                double pctGain = Convert.ToDouble(txt_pricegain.Text)/100;
                double pctSell = Convert.ToDouble(txt_pctsell.Text)/100;
                RollingAlgoFirstLevelSellRule sellRule = new RollingAlgoFirstLevelSellRule(pctSell,pctGain);

                rules.Add(sellRule);

                if (chk_buyback.IsChecked.Value)
                    rules.Add(new RollingAlgoFirstLevelBuyRule());

                algo.TradeRules = rules;

            }
            catch(Exception ex)
            {
                MessageBox.Show("Invalid input parameters, Error: "+ ex.Message);
                return;
            }

            ParentOrder order = ParentOrderManager.Instance.CreateParentOrder(symbol, 0, algo);
            order.IsActive = true;
            ParentOrderManager.Instance.AddParentOrder(order);

            if (!ParentOrderManager.Instance.IsStarted)
            {
                ParentOrderManager.Instance.Start();
            }
            MessageBox.Show(string.Format("Parent order created. Symbol {0}, Begine price {1}", symbol, beginPrice));
        }


        private void btn_getparent_Click(object sender, RoutedEventArgs e)
        {
            //if (!TradeManager.Instance.IsConnected)
            //{
            //    MessageBox.Show("IB is not connected, please reconnect first!");
            //    Log.Error("IB is not connected, please reconnect first");
            //    return;
            //}

            try
            {
                List<ParentOrder> parents = ParentOrderManager.Instance.GetAllParentOrders();
                dg_ParentOrders.ItemsSource = null;
                dg_ParentOrders.ItemsSource = parents;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_getchild_Click(object sender, RoutedEventArgs e)
        {
            //if (!TradeManager.Instance.IsConnected)
            //{
            //    MessageBox.Show("IB is not connected, please reconnect first!");
            //    Log.Error("IB is not connected, please reconnect first");
            //    return;
            //}

            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                    dg_Details.ItemsSource = parent.TradeOrders;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_getExecution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                    dg_Details.ItemsSource = parent.Executions;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_getAlgo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                    dg_Details.ItemsSource = new List<RollingAlgo> (){ (RollingAlgo)parent.Algo};

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_StartParent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                {
                    ParentOrderManager.Instance.StartParentOrder(parent.ID);
                    dg_ParentOrders.Items.Refresh();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_StopParent_Click(object sender, RoutedEventArgs e)
        {
            //if (!TradeManager.Instance.IsConnected)
            //{
            //    MessageBox.Show("IB is not connected, please reconnect first!");
            //    Log.Error("IB is not connected, please reconnect first");
            //    return;
            //}

            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                {
                    ParentOrderManager.Instance.StopParentOrder(parent.ID);
                    dg_ParentOrders.Items.Refresh();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_RemoveParent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                {
                    ParentOrderManager.Instance.RemoveParentOrderByID(parent.ID);
                    dg_ParentOrders.Items.Refresh();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_getTrademap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder po = (ParentOrder)dg_ParentOrders.SelectedItem;
                RollingAlgo algo = (RollingAlgo)po.Algo;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (algo == null)
                    MessageBox.Show("Please select parent order");
                else
                {
                    dg_Trademap.ItemsSource = null;
                    if (algo.TradeMap != null && algo.TradeMap.Count > 0)
                    {
                        var list = algo.TradeMap.Values.OrderBy(o => o.Level).ToList();
                        dg_Trademap.ItemsSource = list;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void btn_CloseParent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                {
                    ParentOrderManager.Instance.CloseParentOrderByID(parent.ID);
                    dg_ParentOrders.Items.Refresh();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            TDClient tdclient = TDClient.Instance;
            //tdclient.GetAccessToken();
            int id = tdclient.PlaceOrder("GDX", 28.89, 150, TradeType.Buy,null, OrderType.LMT);

            //int id = 675583175;
            tdclient.CancelOrder(id);
        }
    }
}
