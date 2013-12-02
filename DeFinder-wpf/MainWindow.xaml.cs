using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using denolk.DeFinder;
using denolk.DeFinder.Services;
using System.Reactive.Linq;
using System.IO;
using System.Diagnostics;

namespace denolk.DeFinder
{
    public partial class MainWindow : Window
    {
        public bool IsLoadingResults { get; set; }

        enum ContextMenuSelection
        {
            OPEN = 0, OPENWITH = 1, DIRECTORY = 2
        }

        ObservableCollection<string> listSource = new ObservableCollection<string>();
        FileService _searchService;

        public MainWindow()
        {
            InitializeComponent();
            _searchService = new FileService();
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Bind();
        }

        private void Bind()
        {
            IsLoadingResults = false;

            listResult.ItemsSource = listSource;

            //var directoryLoad =
            Observable
                .FromEventPattern<KeyEventArgs>(txtDirectory, "KeyUp")
                .Select(evt => evt.EventArgs)
                .Where(evt => evt.Key == Key.Enter && txtDirectory.Text.Length > 0)
                .Select(evt =>
                {
                    IsLoadingResults = true;
                    var directories = txtDirectory.Text.ToLowerInvariant().Split(';').ToList();
                    var rebuild = Keyboard.Modifiers == ModifierKeys.Control;
                    var list = _searchService.GetFiles(directories, rebuild);
                    return list;
                })
                .ObserveOnDispatcher()
                .Subscribe(result =>
                {
                    listSource.Clear();
                    result.ForEach(r => listSource.Add(r));
                    IsLoadingResults = false;
                });

            //var directorySearch =
            Observable
                .FromEventPattern<TextChangedEventArgs>(txtKeyword, "TextChanged")
                .Select(evt => ((TextBox)evt.Sender).Text)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .Select(keyword =>
                {
                    IsLoadingResults = true;
                    var list = _searchService.Filter(keyword);
                    return list;
                })
                .ObserveOnDispatcher()
                .Subscribe(result =>
                {
                    listSource.Clear();
                    result.ForEach(r => listSource.Add(r));
                    IsLoadingResults = false;
                });

            //var listItemSelectedByMouse =
            Observable
                .FromEventPattern<MouseButtonEventArgs>(listResult, "MouseDoubleClick")
                .Subscribe(_ =>
                {
                    var selected = listResult.SelectedItems.OfType<string>().ToList();
                    var openDirectory = Keyboard.Modifiers == ModifierKeys.Control;
                    var openWith = Keyboard.Modifiers == ModifierKeys.Shift;
                    _searchService.Open(selected, openDirectory, openWith);
                }
                );

            Observable
                .FromEventPattern<MouseButtonEventArgs>(listResult, "MouseRightButtonUp")
                .Subscribe(evt =>
                {
                    evt.EventArgs.Handled = listResult.Items.Count == 0;
                }
                );

            //var listItemSelectedByKeyboard =
            Observable
                .FromEventPattern<KeyEventArgs>(listResult, "KeyUp")
                .Select(evt => evt.EventArgs)
                .Where(evt => evt.Key == Key.Enter && listResult.Items.Count > 0)
                .Subscribe(_ =>
                {
                    var selected = listResult.SelectedItems.OfType<string>().ToList();
                    var openDirectory = Keyboard.Modifiers == ModifierKeys.Control;
                    var openWith = Keyboard.Modifiers == ModifierKeys.Shift;
                    _searchService.Open(selected, openDirectory, openWith);
                }
                );

        }

        private void ContextMenu_Selected(object sender, RoutedEventArgs e)
        {
            var selected = listResult.SelectedItems.OfType<string>().ToList();
            var tag = int.Parse((sender as MenuItem).Tag.ToString());
            var openWith = tag == (int)ContextMenuSelection.OPENWITH;
            var openDirectory = tag == (int)ContextMenuSelection.DIRECTORY;
            _searchService.Open(selected, openDirectory, openWith);
        }

    }
}
