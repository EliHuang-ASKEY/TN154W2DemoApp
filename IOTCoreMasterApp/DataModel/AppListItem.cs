using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace IOTCoreMasterApp.DataModel
{
    public class AppListItem : INotifyPropertyChanged
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged(); }
        }

        private BitmapImage _ImgSrc;
        public BitmapImage ImageSrc
        {
                get { return _ImgSrc; }
                set { _ImgSrc = value; OnPropertyChanged(); }
        }

        private string _PackageFullName;
        public string PackageFullName
        {
            get { return _PackageFullName; }
            set { _PackageFullName = value; OnPropertyChanged(); }
        }

        private AppListEntry _AppEntry;
        public AppListEntry AppEntry
        {
            get { return _AppEntry; }
            set { _AppEntry = value; OnPropertyChanged(); }
        }




        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
