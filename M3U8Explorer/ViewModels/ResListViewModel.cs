using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using M3U8Explorer.Models;

namespace M3U8Explorer.ViewModels
{
    public class ResListViewModel : ObservableObject
    {
        public ResListViewModel()
        {
            ResourceInfos = new ObservableCollection<M3U8ResourceInfo>();
        }
        public ObservableCollection<M3U8ResourceInfo> ResourceInfos { get; private set; }
    }
}
