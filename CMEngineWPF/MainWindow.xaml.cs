using CMEngineCore;
using CMEngineCore.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
        private System.Timers.Timer m_timer;
        private volatile bool m_inProcess = false;

        public MainWindow()
        {
            Log.Info("**********Algo trading engine start************");
            InitializeComponent();

            Init();
            m_timer.Start();
        }

        private void Init()
        {
            StateManager.Resume();
            Log.Info("IB is Resumed");

            if (!TradeManager.Instance.IsConnected)
            {
                try
                {
                    string ip = ConfigurationManager.AppSettings["IBIPAddress"];
                    int port = int.Parse(ConfigurationManager.AppSettings["IBPort"]);
                    TradeManager.Instance.Init(Broker.IB);
                    TradeManager.Instance.Connect(ip, port, 1);
                    //MessageBox.Show("IB is connected");
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to Connect to IB. Message: " + ex.Message);
                    Log.Error(ex.StackTrace);
                    MessageBox.Show("Failed to Connect to IB.Message: " + ex.Message);
                }
            }
            else
            {
                Log.Info("IB is already connected");
                //MessageBox.Show("IB is already connected");
            }

            Thread.Sleep(1500);
            if (TradeManager.Instance.IsConnected)
            {
                lab_con_stats.Background = Brushes.LightGreen;
                lab_con_stats.Content = "Connected";


                ParentOrderManager.Instance.Init();
                MessageBox.Show("System is resumed. All Parent Orders are stopped!");

            }
            else
            {
                lab_con_stats.Background = Brushes.LightCoral;
                lab_con_stats.Content = "Disconnected";
                Log.Info("IB is not connected, failed to Resume system");
            }

            StartTimer();
        }

        public void StartTimer()
        {
            if (m_timer == null)
            {
                m_timer = new System.Timers.Timer();
                m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeElapsed);
                m_timer.Interval = 10 * 1000;
            }

        }

        private void OnTimeElapsed(object sender, ElapsedEventArgs e)
        {
            if (m_inProcess) return;

            m_inProcess = true;
            if (!TradeManager.Instance.IsConnected)
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (lab_con_stats != null)
                    {
                        lab_con_stats.Content = "Disconnected";
                        lab_con_stats.Background = Brushes.LightCoral;
                    }
                });

                try
                {
                    string ip = ConfigurationManager.AppSettings["IBIPAddress"];
                    int port = int.Parse(ConfigurationManager.AppSettings["IBPort"]);
                    TradeManager.Instance.Init(Broker.IB);
                    TradeManager.Instance.Connect(ip, port, 1);



                    Thread.Sleep(500);

                    if (!ParentOrderManager.Instance.IsInit)
                    {
                        Log.Info("IB is connected, start to init parent order manager");
                        ParentOrderManager.Instance.Init();
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Failed to Connect to IB. Message: " + ex.Message);
                    Log.Error(ex.StackTrace);

                }
            }else
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (lab_con_stats != null)
                    {
                        lab_con_stats.Content = "Connected";
                        lab_con_stats.Background = Brushes.LightGreen;
                    }
                });
            }
            m_inProcess = false;
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
            if (!TradeManager.Instance.IsConnected || !ParentOrderManager.Instance.IsInit)
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

                double adjQty = Convert.ToDouble(txt_adj.Text);
                bool isAdjPct = comb_adj.SelectedIndex == 0;


                if (beginPrice<=0 || scaleLevel<=0 || scaleFactor<=0 || (isPctScaleFactor && scaleFactor >= 100) ||(isAdjPct && Math.Abs(adjQty)>=100))
                {
                    MessageBox.Show("Invalid price, scale inputs. scale factor pct must be between 0% and 100%. adj Pct must between 0% and 100%");
                    return;
                }

                algo = new RollingAlgo(beginPrice, scaleLevel, scaleFactor, isPctScaleFactor, shareOrDollarAmt, isShare, buyBackLvlZero, adjQty, isAdjPct);


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

            if(dg_ParentOrders.ItemsSource!=null ) dg_ParentOrders.Items.Refresh();
            if (dg_Details.ItemsSource != null)  dg_Details.Items.Refresh();
            if (dg_Trademap.ItemsSource != null)  dg_Trademap.Items.Refresh();

            Dispatcher.InvokeAsync(() => 
            MessageBox.Show(string.Format("Parent order created. Symbol {0}, Begine price {1}", symbol, beginPrice)));
        }


        ObservableCollection<ParentOrder> parentOrderList = new ObservableCollection<ParentOrder>();
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
                parentOrderList.Clear();
                List<ParentOrder> parents = ParentOrderManager.Instance.GetAllParentOrders();
                foreach (var p in parents)
                    parentOrderList.Add(p);

                dg_ParentOrders.ItemsSource = parentOrderList;


                //ParentOrderList = ParentOrderManager.Instance.GetAllParentOrders();
                //dg_ParentOrders.ItemsSource = null;
                //dg_ParentOrders.ItemsSource = parents;

                //if (dg_ParentOrders.ItemsSource != null) dg_ParentOrders.Items.Refresh();
                if (dg_Details.ItemsSource != null) dg_Details.Items.Refresh();
                if (dg_Trademap.ItemsSource != null) dg_Trademap.Items.Refresh();

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        ObservableCollection<TradeOrder> childOrderList = new ObservableCollection<TradeOrder>();
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
                {
                    Dispatcher.InvokeAsync(() => MessageBox.Show("Please select parent order"));
                }
                else
                {
                    childOrderList.Clear();
                    foreach (var to in parent.TradeOrders)
                        childOrderList.Add(to);


                    dg_Details.ItemsSource = childOrderList;

                    //dg_Details.ItemsSource = parent.TradeOrders;

                    if (dg_ParentOrders.ItemsSource != null) dg_ParentOrders.Items.Refresh();
                    //if (dg_Details.ItemsSource != null) dg_Details.Items.Refresh();
                    if (dg_Trademap.ItemsSource != null) dg_Trademap.Items.Refresh();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        ObservableCollection<TradeExecution> executionList = new ObservableCollection<TradeExecution>();
        private void btn_getExecution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    Dispatcher.InvokeAsync(() => MessageBox.Show("Please select parent order"));

                else
                {
                    executionList.Clear();
                    foreach (var ex in parent.Executions)
                        executionList.Add(ex);

                    dg_Details.ItemsSource = executionList;

                    //dg_Details.ItemsSource = parent.Executions;
                    if (dg_ParentOrders.ItemsSource != null) dg_ParentOrders.Items.Refresh();
                    //if (dg_Details.ItemsSource != null) dg_Details.Items.Refresh();
                    if (dg_Trademap.ItemsSource != null) dg_Trademap.Items.Refresh();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MessageBox.Show("Fail to retrieve parent order, error: " + ex.Message);
            }
        }

        ObservableCollection<RollingAlgo> algoList = new ObservableCollection<RollingAlgo>();
        private void btn_getAlgo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;
                //ParentOrder parent = ParentOrderManager.Instance.GetParentOrderByParentID(po.ID);

                if (parent == null)
                    Dispatcher.InvokeAsync(() => MessageBox.Show("Please select parent order"));
                else
                {
                    algoList.Clear();

                    algoList.Add((RollingAlgo)parent.Algo);

                    dg_Details.ItemsSource = algoList;

                    //dg_Details.ItemsSource = new List<RollingAlgo>() { (RollingAlgo)parent.Algo };

                    if (dg_ParentOrders.ItemsSource != null) dg_ParentOrders.Items.Refresh();
                    //if (dg_Details.ItemsSource != null) dg_Details.Items.Refresh();
                    if (dg_Trademap.ItemsSource != null) dg_Trademap.Items.Refresh();
                }

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
                    Dispatcher.InvokeAsync(() => MessageBox.Show("Please select parent order"));
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
                    Dispatcher.InvokeAsync(() => MessageBox.Show("Please select parent order"));
                else
                {
                    dg_Trademap.ItemsSource = null;
                    if (algo.TradeMap != null && algo.TradeMap.Count > 0)
                    {
                        var list = algo.TradeMap.Values.OrderBy(o => o.Level).ToList();
                        dg_Trademap.ItemsSource = list;

                        if (dg_ParentOrders.ItemsSource != null) dg_ParentOrders.Items.Refresh();
                        if (dg_Details.ItemsSource != null) dg_Details.Items.Refresh();
                       // if (dg_Trademap.ItemsSource != null) dg_Trademap.Items.Refresh();
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

        private void btn_CloseParentTrailing_Click(object sender, RoutedEventArgs e)
        {
            //TODO open dialog and place trailing order

            try
            {
                ParentOrder parent = (ParentOrder)dg_ParentOrders.SelectedItem;

                if (parent == null)
                    MessageBox.Show("Please select parent order");
                else
                {
                    ParentOrderManager.Instance.StopParentOrder(parent.ID);
                    Thread.Sleep(500);

                    TrailingOrder trailingOrder = new TrailingOrder(parent);

                    trailingOrder.ShowDialog();
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

            var o = tdclient.GetOrderByID(675583167);
            
            var exe = TradeManager.Instance.GetTDTradeExecution(o);
            exe.Sort();
            
            //tdclient.GetAccessToken();

            //var orders = tdclient.GetOpenOrders();

            List<int> ids = new List<int>();
            int count = 1;
            for (int i = 0; i < count; i++)
            {
                //double price = Math.Round(26.89 + i / (double)100,2);
                double price = 26.01;
                int qty = 150;
                int id = tdclient.PlaceOrder("GDX", price, qty, TradeType.Buy, null, OrderType.LMT);
                Thread.Sleep(200);
                ids.Add(id);
            }


            tdclient.RequestGlobalCancel();
            //int id = 675583175;
            //foreach(int id in ids)
            //    tdclient.CancelOrder(id);
        }

    }
}
