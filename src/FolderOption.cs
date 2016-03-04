using System;

namespace DevZest.Licensing
{
    /// <summary>Specifies enumerated constants used to retrieve directory paths together with a file name.</summary>
    public enum FolderOption
    {
        /// <summary>The file name specifies the full path.</summary>
        Absolute,
        /// <summary>The file name is relative to the assembly.</summary>
        Assembly,
        /// <summary>A system special folder is used together with the file name.</summary>
        EnvironmentSpecial
    }
}