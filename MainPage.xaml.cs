using System.Text;
using LabXml.Services;
using LabXml.Strategies;

namespace LabXml;

public partial class MainPage : ContentPage
{
    private const string Any = "(будь-яке)";
    private readonly List<ISearchStrategy> _strategies =
        new() { new SaxStrategy(), new DomStrategy(), new LinqStrategy() };
    private readonly XmlAnalyzer _analyzer;
    private readonly Dictionary<string, Picker> _pickers = new();
    private string? _path;
    private List<Dictionary<string, string>> _last = new();

    public MainPage()
    {
        InitializeComponent();
        _analyzer = new XmlAnalyzer(_strategies[0]);
        StrategyPicker.ItemsSource = _strategies.Select(s => s.Name).ToList();
        StrategyPicker.SelectedIndex = 0;
    }
    // фіксація вибраной стратегії
    private void OnStrategyChanged(object? s, EventArgs e)
    {
        if (StrategyPicker.SelectedIndex >= 0)
            _analyzer.SetStrategy(_strategies[StrategyPicker.SelectedIndex]);
    }

    // ----- Підгрузка даних: поля пошуку будуються з атрибутів, що є у файлі -----
    private async void OnOpenFile(object? s, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Виберіть XML-файл",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.WinUI] = new[] { ".xml" },
                    [DevicePlatform.Android] = new[] { "text/xml", "application/xml" },
                    [DevicePlatform.iOS] = new[] { "public.xml" },
                    [DevicePlatform.MacCatalyst] = new[] { "public.xml" },
                })
            });
            if (file == null) return;

            _path = file.FullPath;
            FileLabel.Text = file.FileName;
            BuildCriteriaPanel(XmlAnalyzer.LoadAttributeValues(_path));
            OnClear(null, EventArgs.Empty);
        }
        catch (Exception ex) { await DisplayAlert("Помилка", ex.Message, "OK"); }
    }

    private void BuildCriteriaPanel(SortedDictionary<string, SortedSet<string>> attrs)
    {
        CriteriaPanel.Clear();
        _pickers.Clear();
        foreach (var (name, values) in attrs)
        {
            var picker = new Picker { Title = name };
            picker.ItemsSource = new[] { Any }.Concat(values).ToList();
            picker.SelectedIndex = 0;
            _pickers[name] = picker;
            CriteriaPanel.Add(new Label { Text = name, FontSize = 12 });
            CriteriaPanel.Add(picker);
        }
    }

    private Dictionary<string, string> CollectCriteria() =>
        _pickers.Where(p => p.Value.SelectedItem is string v && v != Any)
                .ToDictionary(p => p.Key, p => (string)p.Value.SelectedItem);

    private async void OnSearch(object? s, EventArgs e)
    {
        if (_path == null) { await DisplayAlert("Увага", "Спочатку відкрийте XML-файл", "OK"); return; }
        try
        {
            _last = _analyzer.Search(_path, CollectCriteria());
            var sb = new StringBuilder($"Спосіб: {StrategyPicker.SelectedItem}\nЗнайдено: {_last.Count}\n\n");
            int i = 1;
            foreach (var rec in _last)
            {
                sb.AppendLine($"--- Запис {i++} ---");
                foreach (var kv in rec) sb.AppendLine($"{kv.Key}: {kv.Value}");
                sb.AppendLine();
            }
            Output.Text = sb.ToString();
        }
        catch (Exception ex) { await DisplayAlert("Помилка", ex.Message, "OK"); }
    }

    private async void OnToHtml(object? s, EventArgs e)
    {
        if (_last.Count == 0) { await DisplayAlert("Увага", "Немає результатів для перетворення", "OK"); return; }
        try
        {
            var html = await XmlAnalyzer.ToHtmlAsync(_last);
            await Launcher.Default.OpenAsync(new OpenFileRequest("Результат", new ReadOnlyFile(html)));
        }
        catch (Exception ex) { await DisplayAlert("Помилка", ex.Message, "OK"); }
    }

    // ----- Clear: очищає поле виводу і задані параметри пошуку -----
    private void OnClear(object? s, EventArgs e)
    {
        Output.Text = string.Empty;
        _last.Clear();
        foreach (var p in _pickers.Values) p.SelectedIndex = 0;
    }

    // ----- Підтвердження виходу -----
    private async void OnExit(object? s, EventArgs e) => await ConfirmExit();

    private async Task ConfirmExit()
    {
        if (await DisplayAlert("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні"))
            Application.Current?.Quit();
    }

    // Android: кнопка "Назад" теж запитує підтвердження
    protected override bool OnBackButtonPressed()
    {
        _ = ConfirmExit();
        return true;
    }
}
