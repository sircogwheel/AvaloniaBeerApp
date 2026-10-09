using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace BeerApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Rows.ItemsSource = Beers;
    }

    private ObservableCollection<Beer> Beers { get; } = [];

    private async void NewBeerButton_Click(object? sender, RoutedEventArgs e)
    {
        await OpenBeerDialogAsync(null);
    }

    private async void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Beer beer })
        {
            await OpenBeerDialogAsync(beer);
        }
    }

    private async Task OpenBeerDialogAsync(Beer? beer)
    {
        var dialog = new EditBeerDialog(beer);
        await dialog.ShowDialog(this);
        var editedBeer = dialog.Result;

        if (editedBeer is null)
        {
            return;
        }

        if (beer is null)
        {
            Beers.Add(editedBeer);
        }
        else
        {
            Beers[Beers.IndexOf(beer)] = editedBeer;
        }
    }

    private async void ReadFile_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a beer file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Text files")
                {
                    Patterns = ["*.txt"]
                }
            ]
        });
        
        if (files.Count == 0)
        {
            return;
        }

        await using var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream);

        Beers.Clear();
        while (await reader.ReadLineAsync() is { } line)
        {
            Beers.Add(new Beer(line.Split(';')));
        }
    }

    private async void SaveFile_Click(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save beer file",
            SuggestedFileName = "beer.txt",
            DefaultExtension = "txt",
            FileTypeChoices =
            [
                new FilePickerFileType("Text files")
                {
                    Patterns = ["*.txt"]
                }
            ]
        });

        if (file is null)
        {
            return;
        }

        await using var stream = await file.OpenWriteAsync();
        using var writer = new StreamWriter(stream);

        foreach (var beer in Beers)
        {
            await writer.WriteLineAsync(
                $"{beer.Name};" +
                beer.Price.ToString(CultureInfo.InvariantCulture) + ";" +
                beer.AlcoholContent.ToString(CultureInfo.InvariantCulture));
        }
    }    
    
    private void DeleteButton_Click(object? sender, RoutedEventArgs e)
         {
             if (sender is Button { Tag: Beer beer })
             {
                 Beers.Remove(beer);
             }
         }

}

public class Beer
{
    public Beer(string[] items)
        : this(
            items[0],
            int.Parse(items[1], CultureInfo.InvariantCulture),
            double.Parse(items[2], CultureInfo.InvariantCulture))
    {
    }

    public Beer(string name, int price, double alcoholContent)
    {
        Name = name;
        Price = price;
        AlcoholContent = alcoholContent;
    }

    public string Name { get; }
    public int Price { get; }
    public double AlcoholContent { get; }
}
