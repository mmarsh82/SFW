using SFW.Model.Production;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SFW.Admin
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : UserControl
    {
        #region Properties

        private Point _startPoint = new Point();

        #endregion


        public View()
        {
            InitializeComponent();
        }

        #region Source List Drag and Drop

        private void SourceListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void SourceListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var _listView = (ListView)sender;
            if (int.TryParse(Machine.GetNumber(_listView.SelectedItem.ToString()), out int _machNumber) && _machNumber > 0)
            {
                var _machine = new Machine(_machNumber);
                var _index = ((BindingList<UserConfig>)TargetListView.ItemsSource).Count + 1;
                var _newUC = new UserConfig { MachineNumber = _machine.MachineNumber, MachineName = _machine.MachineName, Position = _index, SiteNumber = App.SiteNumber };
                if (((BindingList<UserConfig>)TargetListView.ItemsSource).Count(o => o.MachineName == _newUC.MachineName) == 0)
                {
                    ((BindingList<UserConfig>)TargetListView.ItemsSource).Add(_newUC);
                    ((ObservableCollection<string>)SourceListView.ItemsSource).Remove(_listView.SelectedItem.ToString());
                }
            }
        }

        private void SourceListView_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var _listView = (ListView)sender;
                var _listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (_listViewItem != null)
                {
                    var _item = (string)_listView?.ItemContainerGenerator.ItemFromContainer(_listViewItem);
                    if (_item != null)
                    {
                        DataObject dragData = new DataObject(typeof(string), _item);
                        DragDrop.DoDragDrop(_listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
                    }
                }
            }
        }

        private void SourceListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(string)) || sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void SourceListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)) && sender == e.Source)
            {
                if(e.Data != null)
                {
                    var _machName = e.Data.GetData(typeof(string)).ToString();
                    if (((ObservableCollection<string>)SourceListView.ItemsSource).Count(o => o == _machName) == 0)
                    {
                        ((ObservableCollection<string>)SourceListView.ItemsSource).Add(_machName);
                        var _item = ((BindingList<UserConfig>)TargetListView.ItemsSource).Where(o => o.MachineName == _machName).FirstOrDefault();
                        ((BindingList<UserConfig>)TargetListView.ItemsSource).Remove(_item);
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        #endregion

        #region Target List Drag and Drop

        private void TargetListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void TargetListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var _listView = (ListView)sender;
            if (_listView.SelectedItem != null)
            {
                var _machName = ((UserConfig)_listView.SelectedItem).MachineName;
                if (!string.IsNullOrEmpty(_machName))
                {
                    ((ObservableCollection<string>)SourceListView.ItemsSource).Add(_machName);
                    var _item = ((BindingList<UserConfig>)TargetListView.ItemsSource).Where(o => o.MachineName == _machName).FirstOrDefault();
                    ((BindingList<UserConfig>)TargetListView.ItemsSource).Remove(_item);
                }
            }
        }

        private void TargetListView_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var _listView = (ListView)sender;
                var _listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (_listViewItem != null)
                {
                    var _item = (UserConfig)_listView?.ItemContainerGenerator.ItemFromContainer(_listViewItem);
                    if (_item != null)
                    {
                        DataObject dragData = new DataObject(typeof(string), _item.MachineName);
                        DragDrop.DoDragDrop(_listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
                    }
                }
            }
        }

        private void TargetListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(string)) || sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TargetListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)) && sender == e.Source)
            {
                if (int.TryParse(Machine.GetNumber(e.Data.GetData(typeof(string)).ToString()), out int _machNumber) && _machNumber > 0)
                {
                    var _machine = new Machine(_machNumber);
                    var _index = ((BindingList<UserConfig>)((ListView)sender).ItemsSource).Count + 1;
                    var _newUC = new UserConfig { MachineNumber = _machine.MachineNumber, MachineName = _machine.MachineName, Position = _index, SiteNumber = App.SiteNumber };
                    if (((BindingList<UserConfig>)((ListView)sender).ItemsSource).Count(o => o.MachineName == _newUC.MachineName) == 0)
                    {
                        ((BindingList<UserConfig>)((ListView)sender).ItemsSource).Add(_newUC);
                        ((ObservableCollection<string>)SourceListView.ItemsSource).Remove(e.Data.GetData(typeof(string)).ToString());
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        #endregion

        //Helper Method for mouse movement
        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
