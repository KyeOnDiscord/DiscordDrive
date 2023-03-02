using System.Runtime.InteropServices;

namespace DiscordDrive;

internal class OpenFileNameNative
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }


    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    //"All Files\0*.*\0JPEG(.jpg)\0*.jpg\0 24 bit Bitmap(.bmp)\0*.bmp\0 16 bit Bitmap(.bmp)\0*.bmp\0 8 bit Bitmap(.bmp)\0*.bmp\0"
    public static string OpenFileDialogue(string filter = "All Files\0*.*\0")
    {
        var ofn = new OpenFileName();
        const int OFN_NOCHANGEDIR = 0x00000008;
        ofn.Flags = OFN_NOCHANGEDIR;
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = filter;
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrTitle = "Open File Dialog...";
        if (GetOpenFileName(ref ofn))
        {
            return ofn.lpstrFile;
        }
        else
        {
            return null!;
        }
    }
}
