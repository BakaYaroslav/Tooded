namespace Tooded_2;
public class LocalizedForm : Form
{
    protected LocalizedForm() => LanguageManager.LanguageChanged += OnLanguageChanged;
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (IsDisposed || Disposing || !IsHandleCreated)
            return;
        LanguageManager.Apply(this);
        RefreshLocalizedValues();
    }

    protected virtual void RefreshLocalizedValues()
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        LanguageManager.Apply(this);
        base.OnLoad(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            LanguageManager.LanguageChanged -= OnLanguageChanged;
        base.Dispose(disposing);
    }
}
