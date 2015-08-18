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

namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonQuotes_Click(object sender, RoutedEventArgs e)
        {
            QuotesWindow bw = new QuotesWindow();
            bw.Show();
        }

        private void ButtonPreAnalyzingStrategy_Click(object sender, RoutedEventArgs e)
        {
            PreAnalyzingStrategyWindow bw = new PreAnalyzingStrategyWindow();
            bw.Show();
        }

        private void ButtonMarginEquityAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            MarginEquityAnalyzerWindow cw = new MarginEquityAnalyzerWindow();
            cw.Show();
        }

        private void ButtonCalculatePortfolio_Click(object sender, RoutedEventArgs e)
        {
            CalculatePortfolioWindow cw = new CalculatePortfolioWindow();
            cw.Show();
        }

        private void ButtonBuildMarginEquity_Click(object sender, RoutedEventArgs e)
        {
            BuildMarginEquityWindow window = new BuildMarginEquityWindow();
            window.Show();
        }
        private void ButtonTools_Click(object sender, RoutedEventArgs e)
        {
            ToolsWindow window = new ToolsWindow();
            window.Show();
        }
    }
}
