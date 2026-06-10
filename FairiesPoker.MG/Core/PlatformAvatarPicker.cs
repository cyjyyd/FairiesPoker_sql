using System;

namespace FairiesPoker.MG.Core;

public static class PlatformAvatarPicker
{
    public static bool IsAvailable
    {
        get
        {
#if ANDROID
            return FairiesPoker.MG.AndroidPlatform.MainActivity.Current != null;
#else
            return false;
#endif
        }
    }

    public static void PickAvatar(Action<byte[]?, string?> callback)
    {
        if (callback == null)
            return;

#if ANDROID
        var activity = FairiesPoker.MG.AndroidPlatform.MainActivity.Current;
        if (activity == null)
        {
            callback(null, "当前安卓窗口不可用");
            return;
        }

        activity.PickAvatar(callback);
#else
        callback(null, "当前平台暂不支持选择头像");
#endif
    }
}
