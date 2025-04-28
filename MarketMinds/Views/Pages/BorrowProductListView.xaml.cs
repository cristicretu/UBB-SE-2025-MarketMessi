using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DomainLayer.Domain;
using ViewModelLayer.ViewModel;
using BusinessLogicLayer.ViewModel;
using MarketMinds;
using MarketMinds.Helpers;
using MarketMinds.Views.Pages;
using MarketMinds.Services.BorrowSortTypeConverterService;

namespace UiLayer
{
    public sealed partial class BorrowProductListView : Window
    {
        private readonly BorrowProductsViewModel borrowProductsViewModel;
        private readonly SortAndFilterViewModel sortAndFilterViewModel;
        private ObservableCollection<BorrowProduct> borrowProducts;
        private CompareProductsViewModel compareProductsViewModel;
        private readonly BorrowProductListViewModelHelper borrowProductListViewModelHelper;
        private readonly BorrowSortTypeConverterService sortTypeConverterService;

        // Pagination variables
        private int CURRENT_PAGE = 1; // Current page number, default to 1
        private int TOTAL_PAGE_COUNT = 1;
        private const int FIRST_PAGE = 1;
        private List<BorrowProduct> currentFullList;

        public BorrowProductListView()
        {
            this.InitializeComponent();

            // Initialize view models and services
            borrowProductsViewModel = MarketMinds.App.BorrowProductsViewModel;
            sortAndFilterViewModel = MarketMinds.App.BorrowProductSortAndFilterViewModel;
            compareProductsViewModel = MarketMinds.App.CompareProductsViewModel;
            borrowProductListViewModelHelper = new BorrowProductListViewModelHelper();
            sortTypeConverterService = new BorrowSortTypeConverterService();

            borrowProducts = new ObservableCollection<BorrowProduct>();
            currentFullList = borrowProductsViewModel.GetAllProducts();
            RefreshProductList();
        }

        private void BorrowListView_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            var selectedProduct = itemClickEventArgs.ClickedItem as BorrowProduct;
            if (selectedProduct != null)
            {
                // Create and show the detail view
                var detailView = new BorrowProductView(selectedProduct);
                detailView.Activate();
            }
        }

        private void RefreshProductList()
        {
            var (pageItems, newTotalPages, fullList) = borrowProductListViewModelHelper.GetBorrowProductsPage(
                borrowProductsViewModel, sortAndFilterViewModel, CURRENT_PAGE);
            currentFullList = fullList;
            TOTAL_PAGE_COUNT = newTotalPages;
            borrowProducts.Clear();
            foreach (var item in pageItems)
            {
                borrowProducts.Add(item);
            }
            // Update UI elements
            EmptyMessageTextBlock.Visibility = borrowProducts.Count == 0 ?
                Visibility.Visible : Visibility.Collapsed;
            UpdatePaginationDisplay();
        }

        private void UpdatePaginationDisplay()
        {
            PaginationTextBlock.Text = borrowProductListViewModelHelper.GetPaginationText(CURRENT_PAGE, TOTAL_PAGE_COUNT);
            var (hasPrevious, hasNext) = borrowProductListViewModelHelper.GetPaginationState(CURRENT_PAGE, TOTAL_PAGE_COUNT);
            PreviousButton.IsEnabled = hasPrevious;
            NextButton.IsEnabled = hasNext;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (CURRENT_PAGE > FIRST_PAGE)
            {
                CURRENT_PAGE--;
                RefreshProductList();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (CURRENT_PAGE < TOTAL_PAGE_COUNT)
            {
                CURRENT_PAGE++;
                RefreshProductList();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs routedEventArgs)
        {
            sortAndFilterViewModel.HandleSearchQueryChange(SearchTextBox.Text);
            CURRENT_PAGE = FIRST_PAGE; // Reset to first page on new search
            RefreshProductList();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            FilterDialog filterDialog = new FilterDialog(sortAndFilterViewModel);
            filterDialog.XamlRoot = Content.XamlRoot;
            var result = await filterDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                CURRENT_PAGE = FIRST_PAGE; // Reset to first page on new filter
                RefreshProductList();
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            SortingComboBox.Visibility = SortingComboBox.Visibility == Visibility.Visible ?
                                         Visibility.Collapsed : Visibility.Visible;
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (SortingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var sortTag = selectedItem.Tag.ToString();
                var sortType = sortTypeConverterService.Convert(sortTag);
                if (sortType != null)
                {
                    sortAndFilterViewModel.HandleSortChange(sortType);
                    CURRENT_PAGE = FIRST_PAGE; // Reset to first page on new sort
                    RefreshProductList();
                }
            }
        }

        private void AddToCompare_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var button = sender as Button;
            var selectedProduct = button.DataContext as Product;
            if (selectedProduct != null)
            {
                bool twoAdded = compareProductsViewModel.AddProductForCompare(selectedProduct);
                if (twoAdded == true)
                {
                    // Create a compare view
                    var compareProductsView = new CompareProductsView(compareProductsViewModel);
                    // Create a window to host the CompareProductsView page
                    var compareWindow = new Window();
                    compareWindow.Content = compareProductsView;
                    compareProductsView.SetParentWindow(compareWindow);
                    compareWindow.Activate();
                }
            }
        }
    }
}
