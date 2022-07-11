using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
public static class Helper {
    public static readonly string version = GetVersion();

    public static string GetVersion() => GetVersion(Assembly.GetCallingAssembly());
    public static string GetVersion(Type type) => GetVersion(type.Assembly);
    public static string GetVersion(Assembly assembly) =>
        assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion ?? "0.0.0";

    public static void OpenUrl(string url) {
        try { Process.Start(url); }
        catch {
            // https://github.com/dotnet/corefx/issues/10361
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", url);
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", url);
            else throw;
        }
    }
}
