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
using System.IO;
using System.Configuration;

namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for MarginEquityAnalyzerWindow.xaml
    /// </summary>
    public partial class MarginEquityAnalyzerWindow : Window
    {
        public MarginEquityAnalyzerWindow()
        {
            InitializeComponent();

            string path = ConfigurationManager.AppSettings["ResultSource"];
            if( string.IsNullOrEmpty( path) )
                path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "result");
            symbolChooser.InitialFolder(path);
        }

        private void ButtonShow3DStudents_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> allSymbols = symbolChooser.SymbolViewItems.Select(p => p.Symbol.Trim().ToLower());
            string baseResultPath = symbolChooser.RootPath;

            foreach (string symbol in allSymbols)
            {
                string symPath = Path.Combine( baseResultPath, symbol);
                if (!Directory.Exists(symPath))
                    continue;
                
                MarginEquityAnalyzerViewModel m = new MarginEquityAnalyzerViewModel();
                m.BuildGraph3D(symPath, "TP", "OpenOrderShift");
            }


        }
    }
}
