#if ANDROID
using System;
using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;

namespace FairiesPoker.MG.AndroidPlatform;

[global::Android.App.Activity(
    Label = "FairiesPoker",
    MainLauncher = true,
    Exported = true,
    Icon = "@mipmap/appicon",
    Theme = "@style/AppTheme",
    ScreenOrientation = global::Android.Content.PM.ScreenOrientation.Landscape,
    ConfigurationChanges =
        global::Android.Content.PM.ConfigChanges.Orientation |
        global::Android.Content.PM.ConfigChanges.Keyboard |
        global::Android.Content.PM.ConfigChanges.KeyboardHidden |
        global::Android.Content.PM.ConfigChanges.ScreenSize)]
public sealed class MainActivity : AndroidGameActivity
{
    private const int PickAvatarRequestCode = 2201;
    private Action<byte[]?, string?> _avatarPickCallback;
    private Game1 _game;

    public static MainActivity Current { get; private set; }

    protected override void OnCreate(global::Android.OS.Bundle bundle)
    {
        base.OnCreate(bundle);
        Current = this;

        Window?.AddFlags(
            global::Android.Views.WindowManagerFlags.Fullscreen |
            global::Android.Views.WindowManagerFlags.KeepScreenOn);

        string filesDir = FilesDir?.AbsolutePath
            ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string resourceRoot = System.IO.Path.Combine(filesDir, "GameAssets");
        AndroidAssetExtractor.ExtractRequired(Assets, resourceRoot);
        ConfigManager.SetResourceBaseDirectory(resourceRoot);

        _game = new Game1();
        var view = (global::Android.Views.View)_game.Services.GetService(typeof(global::Android.Views.View));
        if (view == null)
            throw new InvalidOperationException("MonoGame did not provide an Android game view.");

        SetContentView(view);
        _game.Run();
    }

    protected override void OnDestroy()
    {
        if (Current == this)
            Current = null;

        base.OnDestroy();
    }

    public void PickAvatar(Action<byte[]?, string?> callback)
    {
        _avatarPickCallback = callback;

        RunOnUiThread(() =>
        {
            try
            {
                var intent = new global::Android.Content.Intent(global::Android.Content.Intent.ActionGetContent);
                intent.SetType("image/*");
                intent.AddCategory(global::Android.Content.Intent.CategoryOpenable);
                intent.AddFlags(global::Android.Content.ActivityFlags.GrantReadUriPermission);
                StartActivityForResult(
                    global::Android.Content.Intent.CreateChooser(intent, "选择头像图片"),
                    PickAvatarRequestCode);
            }
            catch (Exception ex)
            {
                CompleteAvatarPick(null, $"无法打开图片选择器: {ex.Message}");
            }
        });
    }

    protected override void OnActivityResult(
        int requestCode,
        global::Android.App.Result resultCode,
        global::Android.Content.Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode != PickAvatarRequestCode)
            return;

        if (resultCode != global::Android.App.Result.Ok || data?.Data == null)
        {
            CompleteAvatarPick(null, "已取消选择头像");
            return;
        }

        try
        {
            byte[] avatarData = ReadAvatarData(data.Data);
            CompleteAvatarPick(avatarData, null);
        }
        catch (Exception ex)
        {
            CompleteAvatarPick(null, $"读取头像失败: {ex.Message}");
        }
    }

    private byte[] ReadAvatarData(global::Android.Net.Uri uri)
    {
        using var input = ContentResolver?.OpenInputStream(uri)
            ?? throw new InvalidOperationException("无法读取选择的图片");
        using var source = global::Android.Graphics.BitmapFactory.DecodeStream(input)
            ?? throw new InvalidOperationException("图片格式不受支持");
        using var cropped = CreateSquareAvatarBitmap(source);

        byte[] data = CompressJpeg(cropped, 88);
        if (data.Length > 500 * 1024)
            data = CompressJpeg(cropped, 65);

        return data;
    }

    private static global::Android.Graphics.Bitmap CreateSquareAvatarBitmap(global::Android.Graphics.Bitmap source)
    {
        int side = Math.Min(source.Width, source.Height);
        if (side <= 0)
            throw new InvalidOperationException("图片尺寸无效");

        int x = Math.Max(0, (source.Width - side) / 2);
        int y = Math.Max(0, (source.Height - side) / 2);
        using var square = global::Android.Graphics.Bitmap.CreateBitmap(source, x, y, side, side);
        return global::Android.Graphics.Bitmap.CreateScaledBitmap(square, 200, 200, true);
    }

    private static byte[] CompressJpeg(global::Android.Graphics.Bitmap bitmap, int quality)
    {
        using var output = new System.IO.MemoryStream();
        if (!bitmap.Compress(global::Android.Graphics.Bitmap.CompressFormat.Jpeg, quality, output))
            throw new InvalidOperationException("头像压缩失败");

        return output.ToArray();
    }

    private void CompleteAvatarPick(byte[] data, string error)
    {
        var callback = _avatarPickCallback;
        _avatarPickCallback = null;
        callback?.Invoke(data, error);
    }
}
#endif
