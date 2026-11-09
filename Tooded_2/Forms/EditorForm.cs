using Microsoft.EntityFrameworkCore;

namespace Tooded_2;
internal abstract class EditorForm : LocalizedForm
{
    protected readonly FlowLayoutPanel Fields = ShopUi.Stack();
    protected EditorForm(Func<string> title)
    {
        LanguageManager.Bind(this, "Text", () => "shoping67 · " + title());
        Size = new Size(820, 820);
        MinimumSize = new Size(700, 600);
        Font = new Font("Segoe UI", 10);
        StartPosition = FormStartPosition.CenterParent;
        Fields.Padding = new Padding(20);
        Controls.Add(Fields);
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            Padding = new Padding(20, 8, 0, 0)
        };
        actions.Controls.Add(ShopUi.Button(() => UiText.Get("Save"), () =>
        {
            try
            {
                Save();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "shoping67");
            }
        }, true));
        actions.Controls.Add(ShopUi.Button(() => UiText.Get("Cancel"), Close));
        Controls.Add(actions);
    }

    protected abstract void Save();
    protected TextBox TextField(Func<string> label, string value, bool multiline = false)
    {
        Fields.Controls.Add(ShopUi.Label(label, 10, true));
        var field = new TextBox
        {
            Text = value,
            Width = 710,
            Multiline = multiline,
            Height = multiline ? 75 : 28,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None
        };
        Fields.Controls.Add(field);
        return field;
    }

    protected TextBox ContentField(Func<string> label, CatalogText value, bool multiline = false)
    {
        var field = TextField(label, value.Et, multiline);
        field.Tag = value;
        return field;
    }

    protected static CatalogText Read(TextBox field)
    {
        var original = field.Tag as CatalogText ?? new();
        string edited = field.Text.Trim();
        // Do not show an obsolete English title after the administrator changes its authored content.
        return edited == original.Et ? original : new CatalogText(edited);
    }

    protected NumericUpDown Number(Func<string> label, decimal value, decimal maximum, int decimals = 0)
    {
        Fields.Controls.Add(ShopUi.Label(label, 10, true));
        var field = new NumericUpDown
        {
            Maximum = maximum,
            DecimalPlaces = decimals,
            Value = Math.Clamp(value, 0, maximum),
            Width = 180
        };
        Fields.Controls.Add(field);
        return field;
    }

    protected ComboBox Choice(Func<string> label, IEnumerable<string> keys, string value, Func<string, string> display)
    {
        Fields.Controls.Add(ShopUi.Label(label, 10, true));
        var field = new ComboBox
        {
            Width = 420,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (var key in keys)
            field.Items.Add(new ChoiceItem(key, "") { Display = () => display(key) });
        field.SelectedIndex = Enumerable.Range(0, field.Items.Count).FirstOrDefault(i => (field.Items[i] as ChoiceItem)?.Key == value);
        LanguageManager.OnRefresh(field, () =>
        {
            int selected = field.SelectedIndex;
            for (int i = 0; i < field.Items.Count; i++)
                field.Items[i] = field.Items[i];
            field.SelectedIndex = selected;
        });
        Fields.Controls.Add(field);
        return field;
    }

    protected static string Key(ComboBox field) => (field.SelectedItem as ChoiceItem)?.Key ?? "";
    protected sealed record ChoiceItem(string Key, string Label)
    {
        public Func<string>? Display { get; init; }
        public string DisplayLabel => Display?.Invoke() ?? Label;

        public override string ToString() => DisplayLabel;
    }
}
