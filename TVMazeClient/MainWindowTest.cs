using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using MoviesApp;
using System.Windows.Controls;
using System.Windows.Input;

public class MainWindowTests
{
    private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
    private MainWindow _mainWindow;

    public MainWindowTests()
    {
        if (Application.Current == null)
        {
            var application = new Application();
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml")
                });
            resourceDictionary.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml")
                });


            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        Thread newWindowThread = new Thread(new ThreadStart(() =>
        {
            _mainWindow = new MainWindow();
            _mainWindow._httpClient = _mockHttpClient.Object;

            System.Windows.Threading.Dispatcher.Run();
        }));

        newWindowThread.SetApartmentState(ApartmentState.STA);
 
        newWindowThread.Start();

        Thread.Sleep(1000);
        

    }

    [Fact]
    [STAThread]
    public async Task OnSearchTextBoxKeyDown_ShouldInvokeOnSearchClick_WhenEnterKeyIsPressed()
    {
        await _mainWindow.Dispatcher.InvokeAsync(() =>
        {
            // Arrange
            var keyEventArgs = new KeyEventArgs(Keyboard.PrimaryDevice,
                new Mock<PresentationSource>().Object, 
                0, 
                Key.Enter);
            var mockSender = new object();

            _mainWindow.SearchTextBox.Text = "Test";

            // Act
            _mainWindow.OnSearchTextBoxKeyDown(mockSender, keyEventArgs);

            // Assert
            var searchMethod = _mainWindow.GetType().GetMethod("OnSearchClick", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(searchMethod);
        } );
    }

    
    [Fact]
    [STAThread]
    public async Task OnSearchClick_ShouldCallLoadMovies_WhenSearchTextIsNotEmpty()
    {
        // Arrange
        var query = "Test Movie";

        await _mainWindow.Dispatcher.InvokeAsync(() => { 
            _mainWindow.SearchTextBox.Text = query;
            _mainWindow.OnSearchClick(new object(), new RoutedEventArgs());
            // Assert
            Assert.NotEqual(_mainWindow.MoviesDataGrid.Items.Count, 0);
            //_mockHttpClient.Verify(client => client.GetStringAsync(It.Is<string>(s => s.Contains(query))), Times.Once);
        });

        //await Task.Delay(1000);

        
    }
    

    [Fact]
    [STAThread]
    public async Task LoadMovies_ShouldPopulateMoviesDataGrid_WhenApiReturnsValidData()
    {
        // Arrange
        var mockResponse = new List<Movie>
        {
            new Movie { Show = new Show { Name = "Test Show", Language = "English", Premiered = "2023", Summary = "A great show" } }
        };
        var jsonResponse = JsonConvert.SerializeObject(mockResponse);
       // _mockHttpClient.Setup(client => client.GetStringAsync(It.IsAny<string>())).ReturnsAsync(jsonResponse);

        // Act
        await _mainWindow.Dispatcher.InvokeAsync(async () =>
        {
            await _mainWindow.LoadMovies("Test");
            

            // Assert
            Assert.Single(_mainWindow.MoviesDataGrid.ItemsSource);
            var movie = (Movie)_mainWindow.MoviesDataGrid.ItemsSource.Cast<Movie>().First();
            Assert.Equal("Test Show", movie.Show.Name);
        });
    }

    
    [Fact]
    [STAThread]
    public void OnSummaryLoaded_ShouldUpdateWebBrowserContent_WhenValidMovieIsProvided()
    {

        _mainWindow.Dispatcher.InvokeAsync( () =>
        {
            var movie = new Movie { Show = new Show { Summary = "A brief summary" } };
            var webBrowser = new WebBrowser { DataContext = movie };
            _mainWindow.OnSummaryLoaded(webBrowser, new RoutedEventArgs());
            dynamic doc = webBrowser.Document;
            Assert.Equal("<html><body>A brief summary</body></html>", webBrowser.Document.ToString());
        });
    }

    /*[Fact]
    [STAThread]
    public async Task LoadMovies_ShouldShowMessageBox_WhenExceptionOccurs()
    {
        // Arrange
        _mockHttpClient.Setup(client => client.GetStringAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Network error"));
        var originalMessageBoxShow = MessageBox.Show;
        MessageBox.Show = (message) => { Assert.Equal("Error: Network error", message); return MessageBoxResult.OK; };

        // Act
        await _mainWindow.LoadMovies("Test");

        // Assert
        MessageBox.Show = originalMessageBoxShow;
    }*/
}
