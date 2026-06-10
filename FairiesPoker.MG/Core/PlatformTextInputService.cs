using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
#if ANDROID
using Microsoft.Xna.Framework.Input;
#endif

namespace FairiesPoker.MG.Core;

public static class PlatformTextInputService
{
    private static readonly ConcurrentQueue<Action> PendingActions = new();
    private static int _isShowing;

    public static bool UsesPopupInput => OperatingSystem.IsAndroid();

    public static bool IsShowing => Volatile.Read(ref _isShowing) == 1;

    public static bool Request(string title, string description, string defaultText, bool usePasswordMode, Action<string> onCompleted)
    {
        if (!UsesPopupInput || onCompleted == null)
            return false;

        if (Interlocked.Exchange(ref _isShowing, 1) == 1)
            return false;

        _ = ShowAsync(title, description, defaultText ?? string.Empty, usePasswordMode, onCompleted);
        return true;
    }

    public static void Update()
    {
        while (PendingActions.TryDequeue(out var action))
        {
            action();
        }
    }

    private static async Task ShowAsync(string title, string description, string defaultText, bool usePasswordMode, Action<string> onCompleted)
    {
        string result = null;
        try
        {
#if ANDROID
            result = await KeyboardInput.Show(title, description, defaultText, usePasswordMode);
#else
            await Task.CompletedTask;
#endif
        }
        catch
        {
            result = null;
        }
        finally
        {
            PendingActions.Enqueue(() =>
            {
                Volatile.Write(ref _isShowing, 0);
                if (result != null)
                    onCompleted(result);
            });
        }
    }
}
