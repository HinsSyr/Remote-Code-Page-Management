///////////////////////////////////////////////////////////////////////
// SelectionWindow.xaml.cs -    Selection Window surface GUI         //
//                                                                   //
// ver 1.1                                                           //
// Author: Bo Qiu          Master in Computer Engineering,           //
//                         Syracuse University                       //
//                         (315) 278-2362, bqiu03@syr.edu            //
///////////////////////////////////////////////////////////////////////
/*
*  Package Operations:
*  -------------------
*  This package implements the SelectionWindow class and create a GUI surface
*
*  Required Files:
*  ---------------
*  SelectionWindow.xaml SelectionWindow.xaml.cs
*
*  Maintenance History:
*  --------------------
*  ver 1.1 : 30 April 2019
*  - second edition
*  ver 1.0 : 09 April 2019
*  - first release
*/


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
using System.IO;
using MsgPassingCommunication;
namespace Navigator
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
  
    /*<--------------------SelectionWindow class defination--------------------->*/
    public partial class SelectionWindow : Window
    {
        List<string> selectedFiles { get; set; } = new List<string>();
        List<string> ConvertedFiles { get; set; } = new List<string>();
        List<string> cmdline { get; set; } = new List<string>();

        private string storageroot = "../../../";

        MainWindow mwin { get; set; } = null;
        /*<--------------------Getting back the MainWindow handle--------------------->*/
        public void connectMainwindow(MainWindow min)
        {
            mwin = min;
        }
        /*<--------------------Method for MainWindow use just for passing value to SelectionWindow--------------------->*/
        public void PassSlectedFiles(List<string> ele)
        {
            selectedFiles = ele;
        }
        /*<--------------------SelectionWindow class construction--------------------->*/
        public SelectionWindow()
        {
            this.Left = 50;
            this.Top = 50;
            CmdLineToList();
            InitializeComponent();
        }
        /*<--------------------Method for getting the SelectionFiles value--------------------->*/
        public void loadfiles()
        {
            SelectedFiles.Items.Clear();
            foreach (string ele in selectedFiles)
                SelectedFiles.Items.Add("Project4" +ele.ToString());
        }
        /*<--------------------Method to execute when SelectionWindo loaded--------------------->*/
        public void SelectionLoaded(object sender, RoutedEventArgs e)
        {
            loadfiles();
        }
        /*<--------------------Method to get back the directory--------------------->*/

        public string finddict(string path)
        {
            int pos=path.LastIndexOf('/');
            path=path.Substring(0, pos);
                return path;
        }
        /*<--------------------Convert button event--------------------->*/
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            Textbox.Text = "";
            Textbox.Text = "Please wait, I am converting these files";
            foreach (string file in selectedFiles)
            {
                Console.WriteLine("file value: " + file);
                string fileName = System.IO.Path.GetFileName(file);
                Console.WriteLine("file name value: " + fileName);

                string srcFile = System.IO.Path.GetFullPath(storageroot + "/" + file);
                string dstFile = mwin.sendFilesPath + "\\" + fileName;
                string dic = finddict(file);
                Console.WriteLine("dict: " + dic);

                Console.WriteLine("source: " + srcFile);
                Console.WriteLine("destination: " + dstFile);

                CsEndPoint serverEndPoint = new CsEndPoint();
                serverEndPoint.machineAddress = "localhost";
                serverEndPoint.port = 8080;
                CsMessage msg = new CsMessage();
                string lstscmd = "";
                foreach (string ele in cmdline)
                {
                    lstscmd += ele + '@';
                }
                msg.add("to", CsEndPoint.toString(serverEndPoint));
                msg.add("from", CsEndPoint.toString(mwin.endPoint_));
                msg.add("command", "convertfiles");
                msg.add("fileName", fileName);
                msg.add("path", dic);
                msg.add("argc",cmdline.Count.ToString());
                msg.add("argv", lstscmd);
                mwin.translater.postMessage(msg);
            }
            mwin.LoadConvertedFiles(); 
        }

        /*<------------------------Extract the Commandline and change to List type-------------->*/
        private List<String> CmdLineToList()
        {
            string[] cmd = Environment.GetCommandLineArgs();
            for (int i = 0; i < cmd.Length; i++)
            {
                cmdline.Add(cmd[i]);
            }
            return cmdline;
        }
        /*<------------------------MouseDoubleClick for deleting files-------------->*/
        private void Deletefiles(object sender, MouseEventArgs e)
        {
            string delitem = SelectedFiles.SelectedItem.ToString();
            selectedFiles.Remove(delitem);
            loadfiles();
        }
        /*<------------------------Method to execute when SelectionWindow unloaded-------------->*/
        private void SelectionUnloaded(object sender, RoutedEventArgs e)
        {
            mwin.setexist(false);
        }
    }
}
