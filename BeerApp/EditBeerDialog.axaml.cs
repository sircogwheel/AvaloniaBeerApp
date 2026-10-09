using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Globalization;

namespace BeerApp;

public partial class EditBeerDialog : Window
{
    public Beer? Result { get; private set; }

    public EditBeerDialog()
        : this(null)
    {
    }

    public EditBeerDialog(Beer? beer)
    {
        InitializeComponent();

        Title = beer is null ? "New beer" : "Edit beer";
        if (beer is not null)
        {
            NameInput.Text = beer.Name;
            PriceInput.Text = beer.Price.ToString(CultureInfo.CurrentCulture);
            AlcoholContentInput.Text = beer.AlcoholContent.ToString(CultureInfo.CurrentCulture);
        }
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ValidationMessage.Text = "Enter a name.";
            return;
        }

        if (!int.TryParse(PriceInput.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var price))
        {
            ValidationMessage.Text = "Enter a valid whole-number price.";
            return;
        }

        if (!double.TryParse(
                AlcoholContentInput.Text,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture,
                out var alcoholContent) ||
            !double.IsFinite(alcoholContent))
        {
            ValidationMessage.Text = "Enter a valid alcohol content.";
            return;
        }

        Result = new Beer(name, price, alcoholContent);
        Close();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
