using iText.Forms;
using iText.Html2pdf;
using iText.Html2pdf.Attach.Impl;
using iText.Kernel.Pdf;
using iText.Licensing.Base;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace zPdfGenerator
{


    /// <summary>
    /// Class PdfHelpers. This class contains helpres methods
    /// </summary>
    internal static class PdfHelpers
    {
        /// <summary>
        /// Attempts to load a license file from the specified path and applies it if found.
        /// </summary>
        /// <remarks>If the specified file exists, it is loaded and applied as the current license. If the
        /// file does not exist, the method returns false and no changes are made to the license state.</remarks>
        /// <param name="licensePath">The file system path to the license file to load. If the file does not exist at this path, no license will
        /// be loaded.</param>
        /// <returns>true if the license file was found and loaded; otherwise, false.</returns>
        public static bool LoadLicenseFile(string? licensePath)
        {
            bool useLicense = false;
            if (File.Exists(licensePath ?? string.Empty))
            {
                useLicense = true;
                LicenseKey.LoadLicenseFile(new FileInfo(licensePath));
            }

            return useLicense;
        }
    }
}
