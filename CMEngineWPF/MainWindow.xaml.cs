﻿using CMEngineCore;
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
                    TradeManager.Instance.Init();
                    TradeManager.Instance.Connect(ip, port, 1);
                }
                catch(Exception ex)
                {
                    Log.Error("Failed to Connect to IB. Message: "+ ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }else
            {
                Log.Info("IB is already connected");
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

                }catch(Exception ex)
                {
                    Log.Error("Disconnect IB error. Message: " + ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
        }

        private void btn_resume_Click(object sender, RoutedEventArgs e)
        {
            if (!TradeManager.Instance.IsConnected)
            {
                Log.Error("IB is not connected, please reconnect it first");
                return;
            }

            StateManager.Resume();

            Log.Info(string.Format("Request Global cancel for all orders on resume"));

            TradeManager.Instance.RequestGlobalCancle();
            Thread.Sleep(3000);
            ParentOrderManager.Instance.SetAllOrdertoCancelStatus();

            ParentOrderManager.Instance.Start();
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

            try
            {
                symbol = txt_symbol.Text.Trim().ToUpper();
                double beginPrice = Convert.ToDouble(txt_price.Text);
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
        }
    }
}
