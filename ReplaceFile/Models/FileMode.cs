using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceFile.Models
{
    public class FileMode : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");

            }
        }

        private string _backupname;

        public string Backupname
        {
            get { return _backupname; }
            set
            {
                _backupname = value;
                RaisePropertyChanged("Backupname");

            }
        }
        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                RaisePropertyChanged("FilePath");

            }
        }

        private bool _isCheck;

        public bool IsCheck
        {
            get { return _isCheck; }
            set 
            { 
                _isCheck = value;
                RaisePropertyChanged("IsCheck");
                if (null != FilterSelectedFiles)
                {
                    FilterSelectedFiles.Invoke(null);
                }
            }
        }

        public override string ToString()
        {
            return Name.ToString();
        }

        public Action<object> FilterSelectedFiles;
    }

    public class HistoryFileMode : ViewModelBase
    {
        private string _replacedTime;

        public string ReplacedTime
        {
            get { return _replacedTime; }
            set
            {
                _replacedTime = value;
                RaisePropertyChanged("ReplacedTime");

            }
        }

        private string _replacedPath;

        public string ReplacedPath
        {
            get { return _replacedPath; }
            set
            {
                _replacedPath = value;
                RaisePropertyChanged("ReplacedPath");

            }
        }
        private bool _isCheck;

        public bool IsCheck
        {
            get { return _isCheck; }
            set
            {
                _isCheck = value;
                foreach (var item in HistoryFiles)
                {
                    Console.WriteLine($"IsCheck:{value},name:{item.Backupname}");
                    item.IsCheck = _isCheck;
                }
                RaisePropertyChanged("IsCheck");
            }
        }
        public Action<object> FilterSelectedHistory;

        public override string ToString()
        {
            return ReplacedTime.ToString();
        }

        public ObservableCollection<ReplaceFile.Models.FileMode> HistoryFiles { get; set; } = new ObservableCollection<ReplaceFile.Models.FileMode>();

    }
}
