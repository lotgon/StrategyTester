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
using System.Windows.Shapes;

namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for Logger.xaml
    /// </summary>
    public partial class Logger : UserControl, IAbstractLogger
    {
        public Logger()
        {
            InitializeComponent();
        }
            
        public void AddLog(string message, IAbstractLoggerMessageType messageType)
        {
             // Create a paragraph with text
            //Paragraph para = new Paragraph();
            //if( messageType == IAbstractLoggerMessageType.Error )
            //    para.Inlines.Add(new Bold(new Run(message)));
            //else
            //    para.Inlines.Add(new Run(message));
            string convMessage = message;
            if (messageType == IAbstractLoggerMessageType.Error)
                convMessage = Environment.NewLine + message;

            convMessage += Environment.NewLine;

            RichTextBoxMain.Dispatcher.Invoke(new Action(delegate() { RichTextBoxMain.Text += convMessage; }));
        }
    }
}
