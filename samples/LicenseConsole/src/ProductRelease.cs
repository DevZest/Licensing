using System;
using System.Windows;

namespace LicenseConsole
{
    public class ProductRelease : DependencyObject
    {
        private static readonly DependencyPropertyKey VersionPropertyKey = DependencyProperty.RegisterReadOnly("Version", typeof(Version), typeof(ProductRelease),
            new FrameworkPropertyMetadata());
        public static readonly DependencyProperty VersionProperty = VersionPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey DownloadUrlPropertyKey = DependencyProperty.RegisterReadOnly("DownloadUrl", typeof(Uri), typeof(ProductRelease),
            new FrameworkPropertyMetadata());
        public static readonly DependencyProperty DownloadUrlProperty = DownloadUrlPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ReleaseNotesPropertyKey = DependencyProperty.RegisterReadOnly("ReleaseNotes", typeof(string), typeof(ProductRelease),
            new FrameworkPropertyMetadata());
        public static readonly DependencyProperty ReleaseNotesProperty = ReleaseNotesPropertyKey.DependencyProperty;

        internal ProductRelease(Version version, Uri downloadUrl, string releaseNotes)
        {
            Version = version;
            DownloadUrl = downloadUrl;
            ReleaseNotes = releaseNotes;
        }

        public Version Version
        {
            get { return (Version)GetValue(VersionProperty); }
            private set { SetValue(VersionPropertyKey, value); }
        }

        public Uri DownloadUrl
        {
            get { return (Uri)GetValue(DownloadUrlProperty); }
            private set { SetValue(DownloadUrlPropertyKey, value); }
        }

        public string ReleaseNotes
        {
            get { return (string)GetValue(ReleaseNotesProperty); }
            private set { SetValue(ReleaseNotesPropertyKey, value); }
        }
    }
}
