using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DiagramDesigner
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        

        public ObservableCollection<DesignerItem> SelectedItemsList { get; set; }
        public DesignerCanvas designerCanvas;
        public MainViewModel()
        {
            designerCanvas = new DesignerCanvas();
        }
    }
}
