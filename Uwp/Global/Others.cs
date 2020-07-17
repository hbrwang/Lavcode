﻿using Windows.ApplicationModel;

namespace Hubery.Lavcode.Uwp
{
    public static partial class Global
    {
        public static string HomeUrl { get; } = "https://lavcode.hubery.wang";

        public static string Email { get; } = "support@hubery.wang";

        public static string Version { get; } = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";

        public static string DragPasswordHeader { get; } = "Lavcode_P";
        public static string PpUrl { get; } = $"{HomeUrl}/pages/pp/zh/";
    }
}
