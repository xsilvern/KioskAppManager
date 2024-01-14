using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KioskAppServer.Service;
using System.Net.Http;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace KioskAppServer.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        public readonly string _fileName = "KioskData";
        public ICommand? UploadCommand { get; }
        public ICommand? CategoryRemoveComand { get; }
        public ICommand? ImageUploadCommand { get; }
        public ICommand? GetDriveFilesCommand { get; }
        public ICommand? MenuAddCommand { get; }

        public ICommand? CategoryFixCommand { get; }
        public ICommand? CategoryDeleteCommand { get; }
        public ICommand? MenuInCurrentCategoryDeleteCommand { get; }

        public int SelectedIndex { get; set; }
        public Visibility MainVisibility { get; set; } = Visibility.Hidden;
        public string CategoryName { get; set; } = string.Empty;

        public string MenuName { get; set; } = string.Empty;
        public string MenuImagePath { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;


        private BitmapImage? _menuImage;
        public BitmapImage? MenuImage
        {
            get => _menuImage;
            set
            {
                _menuImage = value;
                OnPropertyChanged(nameof(MenuImage));
            }
        }

        private KioskDataSet? _kioskData;
        public KioskDataSet? KioskData
        {
            get => _kioskData;
            set
            {
                _kioskData = value;
                OnPropertyChanged(nameof(KioskData));
                OnPropertyChanged(nameof(KioskData.CategoryList));
                OnPropertyChanged(nameof(KioskData.MenuList));
            }
        }
        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                Menus = SelectedCategory?.Menus ?? new ObservableCollection<Menu>();
            }
        }

        private ObservableCollection<Menu>? _menus;
        public ObservableCollection<Menu>? Menus
        {
            get => _menus;
            set
            {
                _menus = value;
                OnPropertyChanged(nameof(Menus));
            }
        }
        GoogleDriveDataService _googleDriveDataService;

        LoadingWindow _loadingWindow;
        public MainViewModel(GoogleDriveDataService googleDriveDataService)
        {
            _googleDriveDataService = googleDriveDataService;
            _loadingWindow = new LoadingWindow();

            MainVisibility = Visibility.Hidden;
            OnPropertyChanged(nameof(MainVisibility));

            //커맨드 초기화
            ImageUploadCommand = new RelayCommand(OpenImageDialog);
            UploadCommand = new RelayCommand(Upload);
            MenuAddCommand = new RelayCommand(AddMenuInCurrentCategory);
            CategoryFixCommand = new RelayCommand<object>(FixCategory);
            CategoryDeleteCommand = new RelayCommand<object>(DeleteCategory);
            MenuInCurrentCategoryDeleteCommand = new RelayCommand<object>(DeleteMenuInCurrentCategory);


            InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            await GetData();

            //연동 되고 난 뒤 보여주기
            MainVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(MainVisibility));
        }
        private async Task GetData()
        {
            KioskData = await _googleDriveDataService.GetKioskDataFromDriveAsync(_fileName);
        }
        private async void FixCategory(object? param)
        {
            if(param is Category category)
            {
                var dialogViewModel = new CategoryEditDialogViewModel();
                var dialog = new CategoryEditDialog { DataContext=dialogViewModel };
                if(dialog.ShowDialog()==true)
                {
                    category.Name = dialogViewModel.CategoryName;
                    await Loading(() => { _googleDriveDataService.UploadToDrive(_fileName,KioskData); });

                    
                }
            }
        }
        private async void DeleteCategory(object? param)
        {
            if (param is Category category)
            {
                KioskData.CategoryList.Remove(category);
                await Loading(() =>
                    {
                        if (SelectedCategory == category)
                        {
                            SelectedCategory = null;
                        }
                        _googleDriveDataService.UploadToDrive(_fileName, KioskData);
                    }
                );
                OnPropertyChanged(nameof(KioskData));
                OnPropertyChanged(nameof(KioskData.CategoryList));
            }

        }
        private async void DeleteMenuInCurrentCategory(object? param)
        {
            if (param is Menu menu)
            {
                Menus?.Remove(menu);
                await Loading(() =>
                    {
                        _googleDriveDataService.UploadToDrive(_fileName, KioskData);
                    }
                );

                OnPropertyChanged(nameof(KioskData));
                OnPropertyChanged(nameof(KioskData.CategoryList));
                OnPropertyChanged(nameof(Menus));
            }
        }
        private void DeleteMenu()
        {

        }
        public async Task Loading(Action action)
        {
            _loadingWindow.Show();

            await Task.Run(() =>
            {

                action();

            });
            _loadingWindow.Hide();
        }



        private void OpenImageDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.jfif)|*.png;*.jpeg;*.jpg;*.jfif|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;  // 파일 잠금 방지, 안 하면 프로세스 접근 못함
                bitmap.EndInit();
                MenuImage = bitmap;

                MenuImagePath = openFileDialog.FileName;
                OnPropertyChanged(nameof(MenuImagePath));
            }
        }





        private async void AddMenuInCurrentCategory()
        {
            var dialogViewModel = new MenuSelectionViewModel(KioskData.MenuList);
            var dialog = new MenuSelectionWindow
            {
                DataContext = dialogViewModel
            };

            if (dialog.ShowDialog() == true)
            {
                var selectedMenu = dialogViewModel.SelectedMenu;
                if (SelectedCategory != null && selectedMenu != null)
                {
                    SelectedCategory.Menus.Add(selectedMenu);
                    await Loading(() =>
                    {
                        _googleDriveDataService.UploadToDrive(_fileName, KioskData);
                    });
                }
            }
        }
        private async void Upload()
        {
            try
            {
                switch (SelectedIndex)
                {
                    case 0:
                        Category category = new Category();
                        category.Name = CategoryName;
                        KioskData.CategoryList.Add(category);
                        await Loading(() =>
                        {
                            _googleDriveDataService.UploadToDrive(_fileName, KioskData);
                        });
                        OnPropertyChanged(nameof(KioskData));
                        OnPropertyChanged(nameof(KioskData.CategoryList));
                        //초기화
                        CategoryName = string.Empty;
                        OnPropertyChanged(CategoryName);
                        break;
                    case 1:
                        Menu menu = new Menu();
                        menu.Name = MenuName;
                        menu.Price = decimal.Parse(Price);
                        if (MenuImage != null)
                        {
                            menu.ImageId = await _googleDriveDataService.UploadImageAsync(MenuImagePath);
                        }
                        KioskData.MenuList.Add(menu);
                        await Loading(() => { _googleDriveDataService.UploadToDrive(_fileName, KioskData); });

                        OnPropertyChanged(nameof(KioskData));
                        OnPropertyChanged(nameof(KioskData.MenuList));

                        //초기화
                        MenuName = string.Empty;
                        Price = string.Empty;
                        MenuImage = null;
                        MenuImagePath = string.Empty;
                        OnPropertyChanged(nameof(MenuName));
                        OnPropertyChanged(nameof(Price));
                        OnPropertyChanged(nameof(MenuImage));
                        OnPropertyChanged(MenuImagePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
            }


        }
    }
}
