using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DXGrid_AssignComboBoxToColumn {
    public class SampleSource1 : INotifyPropertyChanged {
        SampleItem currentItem;
        public ObservableCollection<SampleItem> Items { get; set; }
        public SampleItem CurrentItem {
            get { return currentItem; }
            set {
                if(currentItem == value) return;
                currentItem = value;
                RaisePropertyChanged("CurrentItem");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public SampleSource1() {
            Items = new ObservableCollection<SampleItem>();
            InitItems();
        }
        void InitItems() {
            for(int i = 0; i < 100; i++) {
                SampleItem item = new SampleItem() { Id = i, Name = "item " + i.ToString() };
                Items.Add(item);
            }
        }
        void RaisePropertyChanged(string propertyName) {
            if(PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SampleItem : INotifyPropertyChanged {
        int id;
        string name;
        public int Id {
            get { return id; }
            set {
                if(id == value) return;
                id = value;
                RaisePropertyChanged("Id");
            }
        }
        public string Name {
            get { return name; }
            set {
                if(name == value) return;
                name = value;
                RaisePropertyChanged("Name");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string fieldName) {
            if(PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(fieldName));
        }
    }
}
