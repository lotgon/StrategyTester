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
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.IO;

namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for SymbolChooser.xaml
    /// </summary>
    public partial class SymbolChooser : UserControl
    {
        public class SymbolViewItem
        {
            public string Symbol { get; internal set; }
            public string FullPath { get; internal set; }
            public SymbolViewItem()
            {
            }

        }
        private List<SymbolViewItem> sListViewItemCollection = new List<SymbolViewItem>();
        public List<SymbolViewItem> SListViewItemCollection
        {
            get { return sListViewItemCollection; }
            set { sListViewItemCollection = value; }
        }

        public string SelectedItemPath
        {
            get
            {
                return (ListViewSymbol.SelectedItem as SymbolViewItem).FullPath;
            }
        }
        public string SelectedItemName
        {
            get
            {
                return (ListViewSymbol.SelectedItem as SymbolViewItem).Symbol;
            }
        }
        public IEnumerable<SymbolViewItem> SymbolViewItems
        {
            get
            {
                return sListViewItemCollection;
            }
        }
        public string RootPath { get; private set; }


        public SymbolChooser()
        {
            InitializeComponent();

            InitialFolder(Settings1.Default.DefaultSourcePath);
        }
        public void InitialFolder(string path)
        {
            if( string.IsNullOrEmpty( path))
            {
                path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "sourcedata");
            }
            RootPath = path;
            try
            {
                string dir = path;
                sListViewItemCollection.Clear();
                foreach (string currDir in Directory.GetDirectories(dir))
                {
                    SymbolViewItem n = new SymbolViewItem();
                    n.Symbol = System.IO.Path.GetFileName(currDir);
                    n.FullPath = currDir;
                    sListViewItemCollection.Add(n);
                }
            }
            catch (System.IO.DirectoryNotFoundException exc)
            {
            }
        }

        private void This_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}

