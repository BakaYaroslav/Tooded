using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Tooded_2.Shop;

/// <summary>WinForms localization: resources + UI culture + user-scoped Settings.</summary>
public static class LanguageManager
{
    private static readonly ConditionalWeakTable<Control, Dictionary<string, Func<string>>> bindings = new();
    private static readonly ConditionalWeakTable<Control, List<Action>> refreshers = new();
    public static event EventHandler? LanguageChanged;
    public static bool IsApplying { get; private set; }
    public static void OnRefresh(Control control, Action refresh) => refreshers.GetOrCreateValue(control).Add(refresh);
    public static string Language => Normalize(CultureInfo.CurrentUICulture.Name);
    public static string Normalize(string? language) => language is "en" or "en-US" ? "en-US" : "et-EE";

    public static void Initialize()
    {
        string language;
        try { language = Properties.Settings.Default.UserLanguage; }
        catch (ConfigurationErrorsException) { language = "et-EE"; }
        SetCulture(language);
    }

    private static void SetCulture(string? language)
    {
        var culture = CultureInfo.GetCultureInfo(Normalize(language));
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
    }

    public static void ChangeLanguage(string language, bool save = true)
    {
        string normalized = Normalize(language);
        SetCulture(normalized);
        if (save)
        {
            Properties.Settings.Default.UserLanguage = normalized;
            try { Properties.Settings.Default.Save(); }
            catch (Exception ex) when (ex is ConfigurationErrorsException or IOException or UnauthorizedAccessException)
            { MessageBox.Show(UiText.Get("LanguageSaveFailed"), "shoping67"); }
        }
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static TControl Bind<TControl>(TControl control, string property, Func<string> value) where TControl : Control
    {
        bindings.GetOrCreateValue(control)[property] = value;
        SetProperty(control, property, value());
        return control;
    }

    private static void SetProperty(Control control, string property, string value)
    {
        switch (property)
        {
            case "Text": control.Text = value; break;
            case "PlaceholderText" when control is TextBox text: text.PlaceholderText = value; break;
            case "AccessibleName": control.AccessibleName = value; break;
        }
    }

    // Same recursive ApplyResources pattern as the assignment. Resources contain captions,
    // never user-entered TextBox.Text, checked values, selection indexes or numeric values.
    public static void ApplyResourcesToControl(Control control, ComponentResourceManager resources)
    {
        if (!string.IsNullOrEmpty(control.Name)) resources.ApplyResources(control, control.Name);
        if (control is DataGridView grid)
            foreach (DataGridViewColumn column in grid.Columns)
                if (!string.IsNullOrEmpty(column.Name)) resources.ApplyResources(column, column.Name);
        if (bindings.TryGetValue(control, out var values))
            foreach (var pair in values.ToArray()) SetProperty(control, pair.Key, pair.Value());
        if (control is NumericUpDown number)
            number.Text = number.Value.ToString((number.ThousandsSeparator ? "N" : "F") + number.DecimalPlaces, CultureInfo.CurrentCulture);
        control.Invalidate();
        if (refreshers.TryGetValue(control, out var actions)) foreach (var action in actions) action();
        foreach (Control child in control.Controls) ApplyResourcesToControl(child, resources);
    }

    public static void Apply(Form form)
    {
        bool previous = IsApplying; IsApplying = true;
        form.SuspendLayout();
        try
        {
            var resources = new ComponentResourceManager(form.GetType());
            ApplyResourcesToControl(form, resources);
            resources.ApplyResources(form, "$this");
            // A title binding may include a selected product or an order number.
            if (bindings.TryGetValue(form, out var values))
                foreach (var pair in values.ToArray()) SetProperty(form, pair.Key, pair.Value());
        }
        finally { form.ResumeLayout(true); IsApplying = previous; }
    }

    public static ComboBox CreateSelector()
    {
        var comboLanguage = new ComboBox
        {
            Name = "comboLanguage", AccessibleName = "language-selector",
            DropDownStyle = ComboBoxStyle.DropDownList, Width = 110
        };
        bool _keelLaetud = false;
        comboLanguage.Items.AddRange(new object[] { "Eesti", "English" });
        void UpdateSelection()
        {
            _keelLaetud = false;
            comboLanguage.SelectedIndex = Language == "en-US" ? 1 : 0;
            _keelLaetud = true;
        }
        UpdateSelection();
        comboLanguage.SelectedIndexChanged += (_, _) =>
        {
            if (_keelLaetud) ChangeLanguage(comboLanguage.SelectedIndex == 1 ? "en-US" : "et-EE");
        };
        EventHandler changed = (_, _) => UpdateSelection();
        LanguageChanged += changed;
        comboLanguage.Disposed += (_, _) => LanguageChanged -= changed;
        return comboLanguage;
    }
}

public class LocalizedForm : Form
{
    protected LocalizedForm() => LanguageManager.LanguageChanged += OnLanguageChanged;
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return;
        LanguageManager.Apply(this);
        RefreshLocalizedValues();
    }
    protected virtual void RefreshLocalizedValues() { }
    protected override void OnLoad(EventArgs e)
    {
        LanguageManager.Apply(this);
        base.OnLoad(e);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) LanguageManager.LanguageChanged -= OnLanguageChanged;
        base.Dispose(disposing);
    }
}
