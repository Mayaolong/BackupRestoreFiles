using Microsoft.Win32;
using ReplaceFile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ReplaceFile
{
    class ReplaceViewModel: ViewModelBase,IDisposable
    {

        #region Field
        private string  _configPath = System.IO.Directory.GetCurrentDirectory()+@"\TestFiles.xml";
        #endregion

        #region Properties
        private string _srcFilePath = System.IO.Directory.GetCurrentDirectory();
        public string SrcFilePath
        {
            get { return _srcFilePath; }
            set
            {
                _srcFilePath = value;
                RaisePropertyChanged("SrcFilePath");
            }
        }
        private string _tarFilePath = System.IO.Directory.GetCurrentDirectory();
        public string TarFilePath
        {
            get { return _tarFilePath; }
            set
            {
                _tarFilePath = value;
                RaisePropertyChanged("TarFilePath");
            }
        }

        private int _historyTotal;
        public int HistoryTotal
        {
            get { return _historyTotal; }
            set
            {
                _historyTotal = value;
                RaisePropertyChanged("HistoryTotal");
            }
        }

        private Visibility _historyVisibility = Visibility.Visible;
        public Visibility HistoryVisibility
        {
            get { return _historyVisibility; }
            set
            {
                _historyVisibility = value;
                RaisePropertyChanged("HistoryVisibility");
            }
        }
        private string _switchHistoryBtnName;
        public string SwitchHistoryBtnName
        {
            get { return _switchHistoryBtnName; }
            set
            {
                _switchHistoryBtnName = value;
                RaisePropertyChanged("SwitchHistoryBtnName");
            }
        }

        private bool _enableRestoreHistory;
        public bool EnableRestoreHistory
        {
            get { return _enableRestoreHistory; }
            set
            {
                _enableRestoreHistory = value;
                RaisePropertyChanged("EnableRestoreHistory");
            }
        }

        private bool _enableReplaceButton;
        public bool EnableReplaceButton
        {
            get { return _enableReplaceButton; }
            set
            {
                _enableReplaceButton = value;
                RaisePropertyChanged("EnableReplaceButton");
            }
        }

        private bool _enableRestoreLastButton;
        public bool EnableRestoreLastButton
        {
            get { return _enableRestoreLastButton; }
            set
            {
                _enableRestoreLastButton = value;
                RaisePropertyChanged("EnableRestoreLastButton");
            }
        }
        private ObservableDictionary<string, ObservableDictionary<string, string>> SelectedRestoreBackUpFiles;
        public ObservableCollection<ReplaceFile.Models.FileMode> SrcFiles { get; set; } = new ObservableCollection<ReplaceFile.Models.FileMode>();
        public ObservableCollection<ReplaceFile.Models.FileMode> TargetFiles { get; set; } = new ObservableCollection<ReplaceFile.Models.FileMode>();
        public ObservableCollection<ReplaceFile.Models.HistoryFileMode> HistoryFileInfos { get; set; } = new ObservableCollection<ReplaceFile.Models.HistoryFileMode>();

        public IEnumerable<ReplaceFile.Models.FileMode> SelectedToReplaceFiles = new List<ReplaceFile.Models.FileMode>();

        public IEnumerable<ReplaceFile.Models.FileMode> SelectedLastBackUpFiles = new List<ReplaceFile.Models.FileMode>();

        Dictionary<DateTime, Dictionary<string, string>> _lastReplaceFileinfos = new Dictionary<DateTime, Dictionary<string, string>>();

        #endregion

        public ReplaceViewModel()
        {
            SelectedRestoreBackUpFiles = new ObservableDictionary<string, ObservableDictionary<string, string>>();
            SelectedRestoreBackUpFiles.DictionaryCountChanged -= SelectedRestoreFilesCountChanged;
            SelectedRestoreBackUpFiles.DictionaryCountChanged += SelectedRestoreFilesCountChanged;
            HistoryFileInfos.CollectionChanged -= HistoryFilesChangedEventHandler;
            HistoryFileInfos.CollectionChanged += HistoryFilesChangedEventHandler;
            SrcFiles.CollectionChanged -= SrcFileChanged;
            SrcFiles.CollectionChanged += SrcFileChanged;
            TargetFiles.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => { EnableRestoreLastButton = TargetFiles.Count > 0; };
            _switchHistoryBtnName = HistoryVisibility == Visibility.Visible ? "隐藏历史" : "显示历史";
            InitConfig();
        }

       

        #region Commands

        public ICommand SelectFiles
        { 
            get
            {
                return new CommandBase(new Action<object>(SelectFile), new Func<object, bool>(CanSelectFiles)); 
            } 
        }
        public ICommand SelectTarPath
        {
            get
            {
                return new CommandBase(new Action<object>(SelectPatch), new Func<object, bool>(CanSelectFiles));
            }
        }

        public ICommand ReplaceFilesCommand
        {
            get
            {
                return new CommandBase(new Action<object>(ReplaceFiles),(parameter)=> { return true; });
            }
        }

        public ICommand UndoLastReplaceCommand
        {
            get
            {
                return new CommandBase(new Action<object>(UndoLastReplace), (parameter) => { return true; });
            }
        }
        public ICommand RestoreCommand
        {
            get
            {
                return new CommandBase(new Action<object>(RestoreSelectedFiles), (parameter) => { return true; });
                //return new CommandBase(new Action<object>(RestoreSelectedFiles), CanExecute);

            }
        }
        public ICommand SwitchHistoryVisibleCommand
        {
            get
            {
                return new CommandBase(new Action<object>(SwitchHistoryVisible), (parameter) => { return true; });
            }
        }
        #endregion
        #region Methods
        public bool CanSelectFiles(object param)
        {
            return true;
        }
        private void SelectFile(object param)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = _srcFilePath;
            openFileDialog.Filter = "dll|*.dll*|xml|*.xml*|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = true;
            var result = openFileDialog.ShowDialog().Value;
            if (result)
            {
                var fileNames = openFileDialog.FileNames;
                SrcFilePath = System.IO.Path.GetDirectoryName(fileNames[0]);//路径
                SrcFiles.Clear();
                foreach (var filename in fileNames)
                {
                    var fileMode = new ReplaceFile.Models.FileMode()
                    {
                        Name = System.IO.Path.GetFileName(filename),
                        FilePath = SrcFilePath,
                        IsCheck = true,
                    };
                    fileMode.FilterSelectedFiles -= FilterFiles;
                    fileMode.FilterSelectedFiles += FilterFiles;
                    fileMode.IsCheck = true;
                    SrcFiles.Add(fileMode);
                    Console.WriteLine("fileMode "+ fileMode.Name);
                }
            }
            else
            {
                Console.WriteLine("No files are selected.");
            }
        }
        private void SelectPatch(object obj)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择要替换的文件路径";
            folderBrowserDialog.SelectedPath = TarFilePath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("要替换的文件路径："+ folderBrowserDialog.SelectedPath);
                TarFilePath = folderBrowserDialog.SelectedPath;
            }
            //if (FolderBrowserDialogHelper.ShowFolderBrowser(folderBrowserDialog) == DialogResult.OK)
            //{
            //    Console.WriteLine("要替换的文件路径：" + folderBrowserDialog.SelectedPath);
            //    TarFilePath = folderBrowserDialog.SelectedPath;
            //}

        }
        private void FilterFiles(object param)
        {
            SelectedToReplaceFiles = SrcFiles.Where(x=>x.IsCheck);
            EnableReplaceButton = SelectedToReplaceFiles.Count() > 0;
            Console.WriteLine("SelectedToReplaceFiles count:"+ SelectedToReplaceFiles.Count());

            SelectedRestoreBackUpFiles.Clear();
            foreach (var HistoryFileInfo in HistoryFileInfos)
            {
                ObservableDictionary<string, string> fileModes = new ObservableDictionary<string, string>();
                foreach (var HistoryFile in HistoryFileInfo.HistoryFiles)
                {
                    if (HistoryFile.IsCheck)
                    {
                        fileModes.Add(HistoryFileInfo.ReplacedPath + "\\" +HistoryFile.Backupname, HistoryFileInfo.ReplacedPath + "\\" + HistoryFile.Name);
                    }
                }
                if (fileModes.Count > 0)
                {
                    SelectedRestoreBackUpFiles.Add(HistoryFileInfo.ReplacedTime, fileModes);
                }
                else
                {
                    fileModes = null;
                }
            }
            Console.WriteLine("SelectedRestoreBackUpFiles count:" + SelectedRestoreBackUpFiles.Count);
        }

        private void SelectedRestoreFilesCountChanged()
        {
            //CommandManager./*InvalidateRequerySuggested*/();
            EnableRestoreHistory = SelectedRestoreBackUpFiles?.Count > 0;
            Console.WriteLine("[SelectedRestoreFilesCountChanged] EnableRestoreHistory:"+ EnableRestoreHistory);
        }

        private void SrcFileChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EnableReplaceButton = SrcFiles?.Count > 0;
            Console.WriteLine("SrcFileChanged  EnableReplaceButton:"+ EnableReplaceButton);
        }
        private void HistoryFilesChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("HistoryFilesChangedEventHandler action " + e.Action);
            SelectedRestoreBackUpFiles.Clear();
            foreach (var HistoryFileInfo in HistoryFileInfos)
            {
                ObservableDictionary<string, string> fileModes = new ObservableDictionary<string, string>();
                foreach (var HistoryFile in HistoryFileInfo.HistoryFiles)
                {
                    if (HistoryFile.IsCheck)
                    {
                        fileModes.Add(HistoryFileInfo.ReplacedPath + "\\" + HistoryFile.Backupname, HistoryFileInfo.ReplacedPath + "\\" + HistoryFile.Name);
                    }
                }
                if (fileModes.Count > 0)
                {
                    SelectedRestoreBackUpFiles.Add(HistoryFileInfo.ReplacedTime, fileModes);
                }
                else
                {
                    fileModes = null;
                }
            }
            Console.WriteLine("SelectedRestoreBackUpFiles count:" + SelectedRestoreBackUpFiles.Count);
        }
        private void FilterTargetFiles(object param)
        {
            SelectedLastBackUpFiles = TargetFiles.Where(x => x.IsCheck);
            EnableRestoreLastButton = TargetFiles.Where(x => x.IsCheck).Count() > 0;
            Console.WriteLine("[FilterTargetFiles] EnableRestoreLastButton: " + EnableRestoreLastButton);

        }
        private void ReplaceFiles(object param)
        {
            CopyFiles(SrcFilePath,TarFilePath);
        }

        private void UndoLastReplace(object param)
        {
            if (_lastReplaceFileinfos.Values != null && _lastReplaceFileinfos.Values.Count > 0)
            {
                var time = _lastReplaceFileinfos.First().Key;
                Console.WriteLine("[UndoLastReplace] path:"+ TarFilePath);
                Console.WriteLine("[UndoLastReplace] replaceFilesTime:"+ time.ToString("yyyyMMddHHmmss.fff"));
                foreach (var ReplaceFileinfo in _lastReplaceFileinfos.Values.First())
                {
                    var srcfileFullName = TarFilePath+"\\"+ ReplaceFileinfo.Key;
                    var tarfileFullName = TarFilePath + "\\" + ReplaceFileinfo.Value;

                    if (File.Exists(srcfileFullName))
                    {
                        //恢复
                        File.Delete(tarfileFullName);
                        File.Move(srcfileFullName, tarfileFullName);//需要先删除已经存在的文件
                    }
                    else if (!ReplaceFileinfo.Key.Contains("+")) //在替换时，目的路径没有对应的文件，因此没有备份文件。因此在还原时，直接删除之前替换进来的文件即可
                    {
                        File.Delete(tarfileFullName);
                    }
                    else
                    {
                        Console.WriteLine($"File {srcfileFullName} don't exist.");
                    }
                }
                TargetFiles.Clear();
                DeleteOneTimeReplace(_lastReplaceFileinfos.Keys.First());
            }
           
        }
        private void Restore(object param)
        {
            if (SelectedRestoreBackUpFiles.Values != null && _lastReplaceFileinfos.Values.Count > 0)
            {
                var path = _lastReplaceFileinfos.First().Key;
                Console.WriteLine("[UndoLastReplace] path:" + TarFilePath);
                foreach (var ReplaceFileinfo in _lastReplaceFileinfos.Values.First())
                {
                    var srcfileFullName = TarFilePath + "\\" + ReplaceFileinfo.Key;
                    var tarfileFullName = TarFilePath + "\\" + ReplaceFileinfo.Value;
                    if (File.Exists(srcfileFullName))
                    {
                        //恢复
                        File.Delete(tarfileFullName);
                        File.Move(srcfileFullName, tarfileFullName);//需要先删除已经存在的文件
                    }
                    else
                    {
                        Console.WriteLine($"File {srcfileFullName} don't exist.");
                    }

                    TargetFiles.Clear();
                }
                DeleteOneTimeReplace(_lastReplaceFileinfos.Keys.First());
            }

        }
        private void SwitchHistoryVisible(object param)
        {
            HistoryVisibility = HistoryVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            SwitchHistoryBtnName = HistoryVisibility == Visibility.Visible ? "隐藏历史" : "显示历史";
        }
        private void RestoreSelectedFiles(object param)
        {

            // 1.还原
            if (SelectedRestoreBackUpFiles.Values != null && SelectedRestoreBackUpFiles.Values.Count > 0)
            {
                foreach (var backUpFile in SelectedRestoreBackUpFiles)
                {
                    foreach (var file in backUpFile.Value)
                    {
                        if (File.Exists(file.Key))
                        {
                            //恢复
                            File.Delete(file.Value);
                            File.Move(file.Key, file.Value);//需要先删除已经存在的文件
                        }
                        else if (!file.Key.Contains("+")) //在替换时，目的路径没有对应的文件，因此没有备份文件。因此在还原时，直接删除之前替换进来的文件即可
                        {
                            Console.WriteLine("删除后，目标路径下将不存在运行时所需文件,file:"+ file.Value);
                            File.Delete(file.Value);
                        }
                        else
                        {
                            Console.WriteLine($"File {file.Key} don't exist.");
                        }
                    }
                }
                TargetFiles.Clear();
                _lastReplaceFileinfos.Clear();
                RestoreFiles(SelectedRestoreBackUpFiles);
                ReadConfig();
            }
        }
        private void CopyFiles(string srcPath,string tarPath)
        {
            //判断相同路径
            Console.WriteLine("[CopyFiles] srcPath:" + srcPath);
            Console.WriteLine("[CopyFiles] tarPath:" + tarPath);
            if ( !Directory.Exists(srcPath) || !Directory.Exists(tarPath) || Directory.Equals(srcPath, tarPath))
            {
                Console.WriteLine("file path is error,src is the same to target");
                return;
            }
            if (SrcFiles.Where(x => x.IsCheck).Count() < 0)
            {
               Console.WriteLine("[CopyFiles] no files is selected ");
                return;

            }
            if (!Directory.Exists(tarPath))
            {
                Directory.CreateDirectory(tarPath);
            }
            Dictionary<DateTime, Dictionary<string, string>> FileinfosWithTime = new Dictionary<DateTime, Dictionary<string, string>>();
            DateTime dateTime = DateTime.Now;
            Dictionary<string, string> Fileinfos = new Dictionary<string, string>();
            TargetFiles.Clear();
            foreach (var fileMode in SrcFiles.Where(x => x.IsCheck))
            {
                var tarFile = tarPath +"\\"+ fileMode.Name;
                FileInfo file = new FileInfo(tarFile);

                string backupFileName = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
                if (file.Exists)
                {
                    //备份
                    backupFileName = string.Format("{0}+{1}", fileMode.Name, DateTime.Now.ToString("yyyyMMddHHmmss.fff"));
                    var backupFile = string.Format("{0}{1}", tarPath + "\\", backupFileName);
                    File.Copy(tarFile, backupFile, true);
                }

                var srcFile = fileMode.FilePath + "\\" + fileMode.Name;
                var replaceFile = tarPath + "\\" + fileMode.Name;
                Console.WriteLine("srcFile:"+ srcFile);
                Console.WriteLine("replaceFile:" + replaceFile);
                Fileinfos.Add(backupFileName, fileMode.Name);

                File.Copy(srcFile, replaceFile,true);

                Console.WriteLine("Success to replace,fileName:"+ fileMode.Name);

                var targetFileMode = new ReplaceFile.Models.FileMode()
                {
                    Name = System.IO.Path.GetFileName(replaceFile),
                    FilePath = tarPath,
                };
                targetFileMode.FilterSelectedFiles -= FilterTargetFiles;
                targetFileMode.FilterSelectedFiles += FilterTargetFiles;
                targetFileMode.IsCheck = true;
                TargetFiles.Add(targetFileMode);
                Console.WriteLine("targetFileMode Name: " + targetFileMode.Name);
                
            }
            if (Fileinfos.Count > 0)
            {
                FileinfosWithTime.Add(dateTime, Fileinfos);
                _lastReplaceFileinfos = FileinfosWithTime;
                WriteConfig(FileinfosWithTime);
            }

        }

        private void InitConfig()
        {
            Console.WriteLine("curPath:"+ _configPath);
            if (!File.Exists(_configPath))
            {
                //WriteConfig();

            }
            else
            {
                Console.WriteLine("Config Exists");

                ReadConfig();
            }
        }
        private bool WriteConfig(Dictionary<DateTime, Dictionary<string, string>> FileinfosWithTime)
        {
            XmlDocument doc = new XmlDocument();

            if (!File.Exists(_configPath))
            {
                FileStream fs = File.Create(_configPath);//创建文件
                fs.Close();

                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
                XmlElement root = doc.CreateElement("Root");

                XmlElement replacedFiles = doc.CreateElement("ReplacedFiles");
                root.AppendChild(replacedFiles);

                if (doc.DocumentElement == null)
                {
                    doc.AppendChild(root);
                }

                XmlTextWriter xtr = new XmlTextWriter(_configPath, Encoding.UTF8);
                xtr.Formatting = Formatting.Indented;
                doc.WriteContentTo(xtr);
                xtr.Close();
            }
            doc.Load(_configPath);
            //获取xml根节点
            XmlNode xmlRoot = doc.DocumentElement;
            //根据节点顺序逐步读取//读取第一个name节点
            //var name = xmlRoot.SelectSingleNode("student/name").InnerText;
            var replacedFilesNode = xmlRoot.SelectSingleNode("ReplacedFiles");

            if (replacedFilesNode.HasChildNodes && replacedFilesNode.ChildNodes.Count >= 6)
            {
                //从config中删除
                var result = replacedFilesNode.RemoveChild(replacedFilesNode.FirstChild);
                var time = result.Attributes["time"].InnerText;
                //从History List中删除
                var lastReplaceFiles = HistoryFileInfos.FirstOrDefault(x => x.ReplacedTime == time);
                if (null != lastReplaceFiles)
                {
                    HistoryFileInfos.Remove(lastReplaceFiles);
                }
                Console.WriteLine("Delete history file time:"+ time);
            }
            if (FileinfosWithTime.Count > 0)
            {
                XmlElement files = doc.CreateElement("Files");
                files.SetAttribute("tarPath",TarFilePath);//替换的文件路径

                var replaceTime = FileinfosWithTime.Keys.First().ToString("yyyyMMddHHmmss.fff");
                files.SetAttribute("time", replaceTime);
                //加到history List
                HistoryFileMode historyFileMode = new HistoryFileMode();
                historyFileMode.ReplacedTime = replaceTime;
                historyFileMode.ReplacedPath = TarFilePath;
                foreach (var fileInfo in FileinfosWithTime.Values.First())
                {
                    XmlElement file = doc.CreateElement("File");
                    file.SetAttribute("oldName", fileInfo.Key); //替换之前的文件名

                    if (null != fileInfo.Value)
                    {
                        file.SetAttribute("newName", fileInfo.Value);//备份的新生成的文件名
                    }
                    files.AppendChild(file);
                    replacedFilesNode.AppendChild(files);

                    //加到history List
                    Models.FileMode fileMode = new Models.FileMode();
                    fileMode.Name = fileInfo.Value;
                    fileMode.Backupname = fileInfo.Key;
                    fileMode.FilePath = TarFilePath;
                    fileMode.IsCheck = false;
                    fileMode.FilterSelectedFiles -= FilterFiles;
                    fileMode.FilterSelectedFiles += FilterFiles;
                    historyFileMode.HistoryFiles.Add(fileMode);
                }
                Console.WriteLine("ReplaceTime:" + historyFileMode.ReplacedTime);
                HistoryFileInfos.Add(historyFileMode);
                replacedFilesNode.AppendChild(files);
                XmlTextWriter tr = new XmlTextWriter(_configPath, Encoding.UTF8);
                tr.Formatting = Formatting.Indented;
                doc.WriteContentTo(tr);

                tr.Close();
            }
            HistoryTotal = HistoryFileInfos.Count;
            return true;
        }

        private bool DeleteOneTimeReplace(DateTime dateTime)
        {
            Console.WriteLine("replace's file will be restore,replace files time is:" + dateTime.ToString("yyyyMMddHHmmss.fff"));

            //XmlDocument读取xml文件
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_configPath);
            //获取xml根节点
            XmlNode xmlRoot = xmlDoc.DocumentElement;
            //根据节点顺序逐步读取//读取第一个name节点
            //var name = xmlRoot.SelectSingleNode("student/name").InnerText;
            var replacedFiles = xmlRoot.SelectSingleNode("ReplacedFiles");
            if (replacedFiles.HasChildNodes)
            {
                var replaceFileList = replacedFiles.ChildNodes;
                for (var index = 0; index < replaceFileList.Count; index++)
                {
                    var fileGroup = replaceFileList.Item(index);
                    var time = fileGroup.Attributes["time"].InnerText;
                    Console.WriteLine("replace files time is:" + time);

                    if (time == dateTime.ToString("yyyyMMddHHmmss.fff"))
                    {
                        replacedFiles.RemoveChild(fileGroup);
                        Console.WriteLine("success delete node,replace time is:"+ time);
                        XmlTextWriter xtr = new XmlTextWriter(_configPath, Encoding.UTF8);
                        xtr.Formatting = Formatting.Indented;
                        xmlDoc.WriteContentTo(xtr);
                        xtr.Close();

                        //从History List中删除
                        var lastReplaceFiles = HistoryFileInfos.FirstOrDefault(x => x.ReplacedTime == time);
                        if (null != lastReplaceFiles)
                        {
                           HistoryFileInfos.Remove(lastReplaceFiles);
                            HistoryTotal = HistoryFileInfos.Count;

                        }
                        break;
                    }
                }
            }
            return true;
        }

        private bool RestoreFiles(ObservableDictionary<string, ObservableDictionary<string, string>> FileinfosWithTime)
        {
            //foreach (var Fileinfo in FileinfosWithTime.Keys)
            //{
            //    foreach (var File in FileinfosWithTime[Fileinfo])
            //    {
            //        Console.WriteLine($"[RestoreFiles] time:{Fileinfo},oldName:{File.Key},newName:{File.Value} ");
            //    }
            //}

            //XmlDocument读取xml文件
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_configPath);
            //获取xml根节点
            XmlNode xmlRoot = xmlDoc.DocumentElement;
            var replacedFiles = xmlRoot.SelectSingleNode("ReplacedFiles");
            if (replacedFiles.HasChildNodes)
            {
                var replaceFileList = replacedFiles.ChildNodes;
                for (var index = 0; index < replaceFileList.Count; index++)
                {
                    var fileGroup = replaceFileList.Item(index);
                    var time = fileGroup.Attributes["time"].InnerText;
                    var path = fileGroup.Attributes["tarPath"].InnerText;
                    Console.WriteLine("replace files time is:" + time);

                    var fileList = fileGroup.ChildNodes;
                    for (var i = fileList.Count-1; i >=0; i--)
                    {
                        var file = fileList.Item(i);
                        var oldName = file.Attributes["oldName"].InnerText;
                        if (FileinfosWithTime.ContainsKey(time) && FileinfosWithTime[time].ContainsKey(path + "\\" + oldName))
                        {
                            fileGroup.RemoveChild(file);
                        }
                    }
                    if (fileGroup.ChildNodes.Count ==0)
                    {
                        replacedFiles.RemoveChild(fileGroup);
                        Console.WriteLine("success delete node,replace time is:" + time);
                    }
                }
                XmlTextWriter xtr = new XmlTextWriter(_configPath, Encoding.UTF8);
                xtr.Formatting = Formatting.Indented;
                xmlDoc.WriteContentTo(xtr);
                xtr.Close();
            }
            return true;
        }
        private bool ReadConfig()
        {
            HistoryFileInfos.Clear();
            //XmlDocument读取xml文件
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_configPath);
            //获取xml根节点
            XmlNode xmlRoot = xmlDoc.DocumentElement;
            //根据节点顺序逐步读取//读取第一个name节点
            //var name = xmlRoot.SelectSingleNode("student/name").InnerText;
            var replacedFiles = xmlRoot.SelectSingleNode("ReplacedFiles");
            if(replacedFiles.HasChildNodes)
            {
                var replaceFileList = replacedFiles.ChildNodes;
                for (var index = 0;index< replaceFileList.Count;index++)
                {
                    var fileGroup = replaceFileList.Item(index);
                    var tarPath = fileGroup.Attributes["tarPath"].InnerText;
                    var time = fileGroup.Attributes["time"].InnerText;
                    HistoryFileMode historyFileMode = new HistoryFileMode();
                    historyFileMode.ReplacedTime = time;
                    historyFileMode.ReplacedPath = tarPath;
                   
                    Console.WriteLine("tarPath:" + tarPath);
                    Console.WriteLine("time:" + time);
                    if (fileGroup.HasChildNodes)
                    {
                        var files = fileGroup.ChildNodes;

                        for (var i = 0; i < files.Count; i++)
                        {
                            var file = files.Item(i);
                            var oldName = file.Attributes["oldName"].InnerText;
                            var newName = file.Attributes["newName"].InnerText;
                            Console.WriteLine("oldName:"+ oldName);
                            Console.WriteLine("newName:" + newName);
                            Models.FileMode fileMode = new Models.FileMode();
                            fileMode.Name = newName;
                            fileMode.Backupname = oldName;
                            fileMode.FilePath = tarPath;
                            fileMode.FilterSelectedFiles -= FilterFiles;
                            fileMode.FilterSelectedFiles += FilterFiles;
                            historyFileMode.HistoryFiles.Add(fileMode);
                        }
                        HistoryFileInfos.Add(historyFileMode);
                    }
                }
            }
            HistoryTotal = HistoryFileInfos.Count;

            //读取节点的id属性
            //var id = xmlRoot.SelectSingleNode("student/name").Attributes["id"].InnerText;
            //输出：id:1,name:张三Console.WriteLine("id:{0},name:{1}", id, name);
            //读取所有的name节点
            //foreach (XmlNode node in xmlRoot.SelectNodes("student/name"))
            //{//循环输出
            // Console.WriteLine("id:{0},name:{1}", node.Attributes["id"].InnerText, node.InnerText);
            //}
            return true;
        }

        public void Dispose()
        {
        }


        #endregion


    }
}
