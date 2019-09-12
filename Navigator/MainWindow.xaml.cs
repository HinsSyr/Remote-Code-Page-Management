///////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs -    Main window surface GUI                   //
//                                                                   //
// ver 1.1                                                           //
// Author: Bo Qiu          Master in Computer Engineering,           //
//                         Syracuse University                       //
//                         (315) 278-2362, bqiu03@syr.edu            //
///////////////////////////////////////////////////////////////////////
/*
*  Package Operations:
*  -------------------
*  This package implements the MainWindow class and create a GUI surface
*
*  Required Files:
*  ---------------
*  MainWindow.xaml MainWindow.xaml.cs
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using System.Reflection;
using System.Windows.Interop;
using MsgPassingCommunication;
using System.Threading;

namespace Navigator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /*<-----------------------------------MainWindow class defination--------------------->*/
    public partial class MainWindow : Window
    {
        private string path { get; set; }

        private string storageroot;
        internal Stack<string> pathStack_ = new Stack<string>();

        private List<string> patterns { get; set; } = new List<string>();

        private string brosepath;

        private string cmdpath;
        private List<string> selectedFiles { get; set; } = new List<string>();

        private List<string> remotefiles { get; set; } = new List<string>();

        private bool selectexist = false;
        private bool webunselect = false;

        private int Download = 0;
        private SelectionWindow sw { get; set; } = null;

        internal CsEndPoint endPoint_;

        internal Translater translater;

        private Thread rcvThrd = null;

        internal string saveFilesPath;
        internal string sendFilesPath;

        private Dictionary<string, Action<CsMessage>> dispatcher_
    = new Dictionary<string, Action<CsMessage>>();

        List<string> cmdline { get; set; } = new List<string>();
        /*<-----------------------------------Method for setting the value of logic design-------------->*/
        public void setexist(bool logic)
        {
            selectexist = logic;
        }
        /*<-----------------------------------MainWindow class construction-------------->*/
        public MainWindow()
        {
            this.Left = 200;
            this.Top = 50;
            CmdLineToList();
            connectinitialize();
            while (storageroot == null)
            {
                sendGetCmdMsg();
                System.Threading.Thread.Sleep(2000);
            }
            sendGetRemoteFilesmsg();
            InitializeComponent();
        }
        /*<-----------------------------------Set the endpoint of client the initialize them-------------->*/

        private void connectinitialize()
        {
            endPoint_ = new CsEndPoint();
            endPoint_.machineAddress = "localhost";
            endPoint_.port = 8082;

            translater = new Translater();
            translater.listen(endPoint_);

            // start processing messages
            processMessages();

            // load dispatcher
            loadDispatcher();

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
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
        /*<------------------------Method for getting back to up-directory-------------->*/
        string getAncesPath(string path)
        {
            DirectoryInfo info = Directory.GetParent(path);
            path=info.FullName.ToString();
            return path;
        }
        /*<------------------------MouseDoubleCLick event to response interupt-------------->*/
        private void Dirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                webunselect = true;
                if (Dirs.SelectedItem.ToString() == "..")
                {
                    if (pathStack_.Count > 1)
                    {
                        pathStack_.Pop();
                        
                    }
                    else
                        return;
                }
                else
                {
                    path = pathStack_.Peek() + "/" + Dirs.SelectedItem.ToString();
                    pathStack_.Push(path);
                }
                CurrPath.Text = "Project4" + pathStack_.Peek();
                LoadNavi(path);        
            }
            catch
            {
            }
        }
        /*<-----------------------------------Remote files download in the localstorage-------------->*/

        private void DownLoad_Click(object sender, RoutedEventArgs e)
        {
            webunselect = true;
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(this.endPoint_));
            msg.add("count", Download.ToString());
            msg.add("command", "DownLoad");
            this.translater.postMessage(msg);
            Download++;
        }

            /*<-----------------------Send the request to get command line message-------------->*/

            private void sendGetCmdMsg()
        {
            string lstscmd = "";
            foreach (string ele in cmdline)
            {
                lstscmd += ele + '@';
            }
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(this.endPoint_));
            msg.add("count", cmdline.Count.ToString());
            msg.add("cmdline", lstscmd.ToString());
            msg.add("command", "getCmd");
            this.translater.postMessage(msg);
        }
        /*<-----------------------------------Set the regex patterns for selecting files-------------->*/

        private void setPattern()
        {
            Pattern.Text = "";
            foreach (string ele in patterns)
            {
                Pattern.Text += " ";
                Pattern.Text += ele;
            }
        }
        /*<------------------------Method for real-time reloading Navigator tab-------------->*/
        private void LoadNavi(string path)
        {
            Dirs.Items.Clear();
            path = storageroot + pathStack_.Peek();
            path = System.IO.Path.GetFullPath(path);
            string[] dirs = System.IO.Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if (dir != "." && dir != "..")
                {
                    string itemDir = System.IO.Path.GetFileName(dir);
                    Dirs.Items.Add(itemDir);
                }
            }
            Dirs.Items.Insert(0, "..");

            Files.Items.Clear();
            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string file in files)
            {
                FileInfo finfo = new FileInfo(file);
                foreach (string ch in patterns)
                {
                    System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(ch);
                    System.Text.RegularExpressions.Match result = reg.Match(finfo.Extension);
                    if (result.Success && finfo.Extension != ".html")
                        Files.Items.Add(finfo.Name);
                }
            }
        }
        /*<------------------------Method to execute when loading main window-------------->*/
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            pathStack_.Push("");
            saveFilesPath = translater.setSaveFilePath("../../../LocalStorage");
            sendFilesPath = translater.setSendFilePath("../../../RemoteFiles");
            CurrPath.Text = "Project4";
            
            path = pathStack_.Peek();
            LoadNavi(path);
            
            LoadConvertedFiles();
        }
        private void loadDispatcher()
        {
            DispatcherLoadSendFile();
            DispatchergetCmd();
            DispatcherConvert();
            DispatcherDownLoad();
            DispatchergetRemoteFiles();
            DispatchersingleDownload();
        }
        /*<-----------------------------------Method for handling the received message of singleDownload-------------->*/

        private void DispatchersingleDownload()
            {
            Action<CsMessage> singleDownload = (CsMessage rcvMsg) =>
            {
                Console.WriteLine("\n  processing incoming command : singleDownload ");

                Action act = () => { LoadConvertedFiles(); };
                Dispatcher.Invoke(act, new object[] { });
            };
            addClientProc("singleDownload", singleDownload);
        }
        /*<-----------------------------------Method for handling the received message of getRemoteFiles-------------->*/

        private void DispatchergetRemoteFiles()
        {
            Action<CsMessage> getRemoteFiles = (CsMessage rcvMsg) =>
            {
                Console.WriteLine("\n  processing incoming command : getRemoteFiles ");
                string temp = "";
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("files"))
                    {
                        temp = enumer.Current.Value;
                        remotefiles.Clear();
                        Console.WriteLine("this is files value:  ----------------------" + temp);
                        break;
                    }
                }
                int index = 0;
                for (int i = 0; i < temp.Count(); i++)
                {
                    if (temp[i] == '@')
                    {
                        string temp2 = temp.Substring(index, i - index);
                        remotefiles.Add(temp2);
                        index = i + 1;
                    }
                }
                Action act = () => { ShowRemoteWindow(); };
                Dispatcher.Invoke(act, new Object[] { });
            };
            addClientProc("getRemoteFiles", getRemoteFiles);
        }

        /*<-----------------------------------Method for handling the received message of DownLoad-------------->*/

        private void DispatcherDownLoad()
        {
            Action<CsMessage> DownLoad = (CsMessage rcvMsg) =>
            {
                Console.WriteLine("\n  processing incoming command Download:");
                string end = "";
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("end"))
                    {
                        end = enumer.Current.Value;
                        if (end == "yes")
                        {
                            Download = 0;
                            Action act = () => { LoadConvertedFiles(); };
                            Dispatcher.Invoke(act, new object[] { });
                        }
                        else
                        {
                            CsEndPoint serverEndPoint = new CsEndPoint();
                            serverEndPoint.machineAddress = "localhost";
                            serverEndPoint.port = 8080;
                            CsMessage msg = new CsMessage();
                            msg.add("to", CsEndPoint.toString(serverEndPoint));
                            msg.add("from", CsEndPoint.toString(this.endPoint_));
                            msg.add("count", Download.ToString());
                            msg.add("command", "DownLoad");
                            this.translater.postMessage(msg);
                            Download++;
                        }
                    }
                }
            };
            addClientProc("DownLoad", DownLoad);
        }
        /*<-----------------------------------Method for handling the received message of convertfiles-------------->*/

        private void DispatcherConvert()
        {
            Action<CsMessage> convertfiles = (CsMessage rcvMsg) =>
            {
                Console.WriteLine("\n  processing incoming data with command: convertfiles");
                Action act = () => { sendGetRemoteFilesmsg(); };
                Dispatcher.Invoke(act, new Object[] { });
            };
            addClientProc("convertfiles", convertfiles);
        }
        /*<-----------------------------------Method for handling the received message of getCmd-------------->*/

        private void DispatchergetCmd()
        {
            Action<CsMessage> getCmd = (CsMessage rcvMsg) =>
            {
                Console.WriteLine("\n  processing incoming data with command: getCmd");
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("browsepath"))
                    {
                        brosepath = enumer.Current.Value;
                        
                    }
                    if (key.Contains("cmdpath"))
                    {
                        cmdpath = enumer.Current.Value;
                        storageroot = cmdpath;
                    }
                    if (key.Contains("patterns"))
                    {
                        string pat = enumer.Current.Value;
                        int index = 0;
                        for (int i = 0; i < pat.Count(); i++)
                        {
                            if (pat[i] == '@')
                            {
                                string temp = pat.Substring(index, i - index);
                                patterns.Add(temp);
                                index = i + 1;
                            }
                        }     
                    }
                }
                Action act = () => { setPattern(); };
               Dispatcher.Invoke(act, new Object[] { });
            };
            addClientProc("getCmd", getCmd);
        }

        /*<-----------------------------------Method for handling the received message of sendFile-------------->*/

        private void DispatcherLoadSendFile()
        {
            Action<CsMessage> sendFile = (CsMessage rcvMsg) =>
            {
                Console.Write("\n  processing incoming file");
                string fileName = "";
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("sendingFile"))
                    {
                        fileName = enumer.Current.Value;
                        break;
                    }
                }
                if (fileName.Length > 0)
                {
                    Action<string> act = (string fileNm) => { LoadConvertedFiles(); };
                    Dispatcher.Invoke(act, new object[] { fileName });
                }
            };
            addClientProc("sendFile", sendFile);
        }

        /*<-----------------------------------Add thread start function to dispatcher-------------->*/

        private void addClientProc(string key, Action<CsMessage> clientProc)
        {
            dispatcher_[key] = clientProc;
        }



        //----< process incoming messages on child thread >----------------

        private void processMessages()
        {
            ThreadStart thrdProc = () => {
                while (true)
                {
                    CsMessage msg = translater.getMessage();
                    try
                    {
                        if (msg.attributes.Count() == 0) continue;
                        string msgId = msg.value("command");
                        Console.Write("\n  client getting message \"{0}\"", msgId);
                        if (dispatcher_.ContainsKey(msgId))
                            dispatcher_[msgId].Invoke(msg);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n  {0}", ex.Message);
                        msg.show();
                    }
                }
            };
            rcvThrd = new Thread(thrdProc);
            rcvThrd.IsBackground = true;
            rcvThrd.Start();
        }
        /*<--------------Loading the LocalStorage tableitem-------------->*/
        public void LoadConvertedFiles()
        {
            this.LocalStorage.Items.Clear();
            string ConvertedDirectory = "../../../";
            ConvertedDirectory = System.IO.Path.Combine(ConvertedDirectory + "LocalStorage");
            ConvertedDirectory = System.IO.Path.GetFullPath(ConvertedDirectory);
            Console.WriteLine(ConvertedDirectory);
            if (Directory.Exists(ConvertedDirectory))
            {
                string[] files_ls = Directory.GetFiles(ConvertedDirectory);
                foreach (string ele in files_ls)
                    this.LocalStorage.Items.Add(ele);
            }
        }
        /*<---------------Method for resposing single click interupt-------------->*/
        private void Files_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ( selectexist==false)
                {
                    webunselect = true;
                    sw = new SelectionWindow();
                    sw.connectMainwindow(this);
                    setexist(true);                  
                }
                string filepath = path+'/'+ Files.SelectedItem.ToString();
                if (!selectedFiles.Contains(filepath))
                {
                    selectedFiles.Add(filepath);
                }
                sw.PassSlectedFiles(selectedFiles);
                sw.loadfiles();
                sw.Show();
            }
            catch
            {

            }
        }

        /*<-----------------MouseDoubleClick event to response and open files in browser-------------->*/
        private void Showfiles(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(brosepath, LocalStorage.SelectedItem.ToString());
        }
        /*<-----------------MouseDoubleClick event to response and download all files from server-------------->*/

        private void DownLoadSingleFile(object sender, MouseButtonEventArgs e)
        {
            webunselect = true;
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(this.endPoint_));
            msg.add("file", RemoteFilesListBox.SelectedItem.ToString());
            msg.add("command", "singleDownload");
            this.translater.postMessage(msg);

        }
        /*<-----------------Loading the RemoteFiles tabitem-------------->*/

        private void ShowRemoteWindow()
        {
            RemoteFilesListBox.Items.Clear();
            foreach (string ele in remotefiles)
            
                RemoteFilesListBox.Items.Add(ele);
        }
        /*<-----------------Method for sending message of getting remotefiles-------------->*/

        private void sendGetRemoteFilesmsg()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(this.endPoint_));
            msg.add("command", "getRemoteFiles");
            this.translater.postMessage(msg);
        }

        /*<----------------Event for response opening files in browser-------------------->*/
        private void Showfiles_popup(object sender, SelectionChangedEventArgs e)
        {
            if (webunselect)
            {
                webunselect = false;
                return;
            }
            Paragraph paragraph = new Paragraph();
            string fileSpec = LocalStorage.SelectedItem.ToString();
            string fileText = File.ReadAllText(fileSpec);
            paragraph.Inlines.Add(new Run(fileText));
            CodePopupWindow popUp = new CodePopupWindow();
            popUp.codeView.Blocks.Clear();
            popUp.codeView.Blocks.Add(paragraph);
            popUp.Show();
        }
     
    }
}
