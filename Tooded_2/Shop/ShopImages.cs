using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Tooded_2.Shop;

internal static class ShopImages
{
    private static readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(10), MaxResponseContentBufferSize = 5 * 1024 * 1024 };
    private static readonly SemaphoreSlim slots = new(4);
    private static async Task<Image?> Remote(string address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) || uri.Scheme != "https") return null;
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "shoping67", "Images");
        string path = Path.Combine(directory, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(address))) + ".png");
        await slots.WaitAsync();
        try
        {
            if (File.Exists(path)) return await Task.Run(() => Load(path));
            byte[] bytes = await client.GetByteArrayAsync(uri);
            if (bytes.Length == 0 || bytes.Length > 5 * 1024 * 1024) return null;
            byte[] png = await Task.Run(() =>
            {
                using var decoded = SixLabors.ImageSharp.Image.Load(bytes);
                using var output = new MemoryStream();
                decoded.Save(output, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                return output.ToArray();
            });
            try
            {
                Directory.CreateDirectory(directory);
                string temporary = path + "." + Guid.NewGuid().ToString("N");
                try { await File.WriteAllBytesAsync(temporary, png); File.Move(temporary, path, true); }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            using var stream = new MemoryStream(png); using var source = Image.FromStream(stream); return new Bitmap(source);
        }
        catch (Exception) { return null; }
        finally { slots.Release(); }
    }

    public static Image? Load(string? file)
    {
        if (string.IsNullOrWhiteSpace(file)) return null;
        string relative = file.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        var paths = new List<string>();
        if (Path.IsPathRooted(relative)) paths.Add(relative);
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            paths.Add(Path.Combine(dir.FullName, relative));
            paths.Add(Path.Combine(dir.FullName, "Images", relative));
            paths.Add(Path.Combine(dir.FullName, "Images", Path.GetFileName(relative)));
        }
        foreach (var path in paths.Distinct())
        {
            if (!File.Exists(path)) continue;
            try
            {
                using var stream = File.OpenRead(path);
                using var original = Image.FromStream(stream);
                return new Bitmap(original);
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or OutOfMemoryException) { }
        }
        return null;
    }

    public static PictureBox Picture(string? file)
    {
        var picture = new PictureBox
        {
            Image = Load(file), SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 249, 250), Margin = new Padding(0, 0, 20, 0)
        };
        bool started = false;
        picture.HandleCreated += async (_, _) =>
        {
            if (started || string.IsNullOrWhiteSpace(file) || !file.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return;
            started = true;
            var downloaded = await Remote(file);
            if (picture.IsDisposed) { downloaded?.Dispose(); return; }
            picture.Image?.Dispose(); picture.Image = downloaded; picture.Invalidate();
        };
        picture.Paint += (_, e) =>
        {
            if (picture.Image == null)
                TextRenderer.DrawText(e.Graphics, T.Legacy("Pilt puudub"), SystemFonts.MessageBoxFont, picture.ClientRectangle,
                    ShopStyle.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        picture.Disposed += (_, _) => picture.Image?.Dispose();
        return picture;
    }
}
