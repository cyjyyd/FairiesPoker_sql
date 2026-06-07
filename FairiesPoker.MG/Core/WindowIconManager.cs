using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IOPath = System.IO.Path;

namespace FairiesPoker.MG.Core;

internal static class WindowIconManager
{
    private const string IconPath = "Resources/AppIcon.png";

    public static void Apply(Game game)
    {
        if (game?.Window == null || game.GraphicsDevice == null || game.Window.Handle == IntPtr.Zero)
            return;

        string iconPath = IOPath.Combine(AppContext.BaseDirectory, IconPath);
        if (!File.Exists(iconPath))
            return;

        try
        {
            using var stream = File.OpenRead(iconPath);
            using var texture = Texture2D.FromStream(game.GraphicsDevice, stream);
            var colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);

            var pixels = new byte[colors.Length * 4];
            for (int i = 0; i < colors.Length; i++)
            {
                int offset = i * 4;
                pixels[offset] = colors[i].R;
                pixels[offset + 1] = colors[i].G;
                pixels[offset + 2] = colors[i].B;
                pixels[offset + 3] = colors[i].A;
            }

            if (!SdlApi.TryLoad(AppContext.BaseDirectory, out var sdl))
                return;

            using (sdl)
            {
                var pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try
                {
                    IntPtr surface = sdl.CreateRgbSurfaceFrom(
                        pinnedPixels.AddrOfPinnedObject(),
                        texture.Width,
                        texture.Height,
                        32,
                        texture.Width * 4,
                        0x000000ff,
                        0x0000ff00,
                        0x00ff0000,
                        0xff000000);

                    if (surface == IntPtr.Zero)
                        return;

                    try
                    {
                        sdl.SetWindowIcon(game.Window.Handle, surface);
                    }
                    finally
                    {
                        sdl.FreeSurface(surface);
                    }
                }
                finally
                {
                    pinnedPixels.Free();
                }
            }
        }
        catch
        {
            // Window icon is cosmetic; never block game startup if SDL rejects the surface.
        }
    }

    private sealed class SdlApi : IDisposable
    {
        private readonly IntPtr _library;
        private readonly CreateRgbSurfaceFromDelegate _createRgbSurfaceFrom;
        private readonly SetWindowIconDelegate _setWindowIcon;
        private readonly FreeSurfaceDelegate _freeSurface;

        private SdlApi(IntPtr library)
        {
            _library = library;
            _createRgbSurfaceFrom = LoadDelegate<CreateRgbSurfaceFromDelegate>(library, "SDL_CreateRGBSurfaceFrom");
            _setWindowIcon = LoadDelegate<SetWindowIconDelegate>(library, "SDL_SetWindowIcon");
            _freeSurface = LoadDelegate<FreeSurfaceDelegate>(library, "SDL_FreeSurface");
        }

        public static bool TryLoad(string baseDirectory, out SdlApi api)
        {
            foreach (string candidate in GetLibraryCandidates(baseDirectory))
            {
                if (!NativeLibrary.TryLoad(candidate, out IntPtr library))
                    continue;

                try
                {
                    api = new SdlApi(library);
                    return true;
                }
                catch
                {
                    NativeLibrary.Free(library);
                }
            }

            api = null;
            return false;
        }

        public IntPtr CreateRgbSurfaceFrom(
            IntPtr pixels,
            int width,
            int height,
            int depth,
            int pitch,
            uint rMask,
            uint gMask,
            uint bMask,
            uint aMask)
        {
            return _createRgbSurfaceFrom(pixels, width, height, depth, pitch, rMask, gMask, bMask, aMask);
        }

        public void SetWindowIcon(IntPtr window, IntPtr icon)
        {
            _setWindowIcon(window, icon);
        }

        public void FreeSurface(IntPtr surface)
        {
            _freeSurface(surface);
        }

        public void Dispose()
        {
            if (_library != IntPtr.Zero)
                NativeLibrary.Free(_library);
        }

        private static IEnumerable<string> GetLibraryCandidates(string baseDirectory)
        {
            if (OperatingSystem.IsWindows())
            {
                yield return IOPath.Combine(baseDirectory, "SDL2.dll");
            }
            else if (OperatingSystem.IsLinux())
            {
                yield return IOPath.Combine(baseDirectory, "libSDL2-2.0.so.0");
                yield return "libSDL2-2.0.so.0";
            }
            else if (OperatingSystem.IsMacOS())
            {
                yield return IOPath.Combine(baseDirectory, "libSDL2-2.0.0.dylib");
                yield return "libSDL2-2.0.0.dylib";
            }

            yield return "SDL2";
        }

        private static T LoadDelegate<T>(IntPtr library, string name) where T : Delegate
        {
            IntPtr symbol = NativeLibrary.GetExport(library, name);
            return Marshal.GetDelegateForFunctionPointer<T>(symbol);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CreateRgbSurfaceFromDelegate(
            IntPtr pixels,
            int width,
            int height,
            int depth,
            int pitch,
            uint rMask,
            uint gMask,
            uint bMask,
            uint aMask);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetWindowIconDelegate(IntPtr window, IntPtr icon);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreeSurfaceDelegate(IntPtr surface);
    }
}
