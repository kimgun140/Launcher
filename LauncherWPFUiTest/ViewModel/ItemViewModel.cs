using LauncherWPFUiTest.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherWPFUiTest.ViewModel
{
    class ItemViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ItemModel> _items;
        public ObservableCollection<ItemModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ItemViewModel()
        {
            Items = new ObservableCollection<ItemModel>();

            // 더미 데이터 추가
            for (int i = 1; i <= 20; i++)
            {
                Items.Add(new ItemModel
                {
                    Text = $"Patch Note {i}",
                    ImagePath = "Images/note.png" // 프로젝트 내 이미지 경로
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
