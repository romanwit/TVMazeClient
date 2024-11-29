using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoviesApp
{
    public partial class MainWindow : Window
    {
        public HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        public void OnSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnSearchClick(sender, e); 
            }
        }

        public void OnSummaryLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebBrowser webBrowser && webBrowser.DataContext is Movie movie)
            {
                string htmlContent = $"<html><body>{movie.Show.Summary}</body></html>";
                webBrowser.NavigateToString(htmlContent);
            }
        }

        public async void OnSearchClick(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(query))
            {
                await LoadMovies(query);
            }
        }

        public async Task LoadMovies(string searchQuery)
        {
            try
            {
                string url = $"http://api.tvmaze.com/search/shows?q={searchQuery}";
                string response = await _httpClient.GetStringAsync(url);
                var movies = JsonConvert.DeserializeObject<List<Movie>>(response);
                MoviesDataGrid.ItemsSource = movies;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }

    public class Movie
    {
        public Show Show { get; set; }
    }

    public class Show
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Premiered { get; set; }
        public Image Image { get; set; }
        public string Summary { get; set; }
    }

    public class Image
    {
        public string Medium { get; set; }
    }
}
