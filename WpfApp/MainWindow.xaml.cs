using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("Kernel32.dll")]
        static extern IntPtr GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetGUIThreadInfo(IntPtr hTreadID, ref GUITHREADINFO lpgui);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern void ClientToScreen(IntPtr hWnd, ref Point p);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "GetCaretPos")]
        static extern bool GetCaretPos(out Point lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("imm32.dll", SetLastError = true)]
        public static extern int ImmGetCompositionWindow(int hIMC, ref COMPOSITIONFORM lpCompositionForm);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        public struct COMPOSITIONFORM
        {
            public uint dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }


        public struct RECT
        {
            public int iLeft;
            public int iTop;
            public int iRight;
            public int iBottom;
            public RECT(int lt, int tp, int rt, int btm)
            {
                iLeft = lt; iTop = tp; iRight = rt; iBottom = btm;
            }
        }
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rectCaret;
        }


        private DispatcherTimer _timer;
        private void SetupTimer()
        {
            _timer = new DispatcherTimer(); // 優先度はDispatcherPriority.Background
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += new EventHandler(myGetCaretPos);
            _timer.Start();
        }


        private void myGetCaretPos(object sender, EventArgs e)
        {
            IntPtr hWnd_current = GetCurrentThreadId();
            IntPtr hWnd_foreground = GetForegroundWindow();
            if (hWnd_foreground != IntPtr.Zero)
            {
                IntPtr target = GetWindowThreadProcessId(hWnd_foreground, IntPtr.Zero);

                GUITHREADINFO gti = new GUITHREADINFO();
                gti.cbSize = Marshal.SizeOf(gti);
                if (GetGUIThreadInfo(target, ref gti))
                {
                    RECT rect1a = new RECT(0, 0, 0, 0);
                    GetWindowRect(gti.hwndFocus, out rect1a);

                    RECT rect1b = new RECT(0, 0, 0, 0);
                    ClientToScreen(gti.hwndFocus, out rect1b);

                    RECT rect1c = new RECT(0, 0, 0, 0);
                    GetWindowRect(hWnd_foreground, out rect1c);

                    RECT rect1d = new RECT(0, 0, 0, 0);
                    ClientToScreen(hWnd_foreground, out rect1d);

                    RECT rect1e = new RECT(0, 0, 0, 0);
                    GetWindowRect(gti.hwndCaret, out rect1e);

                    RECT rect1f = new RECT(0, 0, 0, 0);
                    ClientToScreen(gti.hwndCaret, out rect1f);


                    Point p = new Point();
                    if (hWnd_current != hWnd_foreground)
                    {
                        AttachThreadInput(hWnd_current, hWnd_foreground, 1);
                        /*
                        IntPtr ptr = GetFocus();
                        if (ptr.ToInt32() != 0)
                        {
                            GetCaretPos(out p);
                            ClientToScreen(ptr, ref p);
                        }
                        */
                        GetCaretPos(out p);
                        ClientToScreen(hWnd_current, ref p);

                        AttachThreadInput(hWnd_current, hWnd_foreground, 0);
                    }


                    RECT rect2a = new RECT(0, 0, 0, 0);
                    if (!GetClientRect(hWnd_foreground, out rect2a)) {
                        int errCode = Marshal.GetLastWin32Error();
                        Console.WriteLine("Win32 ERROR:" + String.Format("{0:X8}", errCode));
                    };

                    Point p2 = new Point(gti.rectCaret.iLeft, gti.rectCaret.iTop);
                    p2.X = p2.X + rect1b.iLeft;
                    p2.Y = p2.Y + rect1b.iTop;

                    this.Left = p2.X - this.Width / 2;
                    this.Top = p2.Y - this.Height / 2;

                    Console.WriteLine(
                        "  hWnd_foreground:" + hWnd_foreground +
                        ", hWnd_current:" + hWnd_current +
                        ", p:{" + p.X + "," + p.Y + "}" +
                        ", rect1a:{" + rect1a.iLeft + "," + rect1a.iTop + "}" +
                        ", rect1b:{" + rect1b.iLeft + "," + rect1b.iTop + "}" +
                        ", rect1c:{" + rect1c.iLeft + "," + rect1c.iTop + "}" +
                        ", rect1d:{" + rect1d.iLeft + "," + rect1d.iTop + "}" +
                        ", rect1e:{" + rect1e.iLeft + "," + rect1e.iTop + "}" +
                        ", rect1f:{" + rect1f.iLeft + "," + rect1f.iTop + "}" +
                        ", r2a.iLeft:" + rect2a.iLeft +
                        ", r2a.iTop:" + rect2a.iTop +
                        ", gti.iLeft:" + gti.rectCaret.iLeft +
                        ", gti.iTop:" + gti.rectCaret.iTop +
                        ", gti.hwndCaret:" + gti.hwndCaret +
                        ", gti.hwndFocus:" + gti.hwndFocus +
                        ", gti.hwndActive:" + gti.hwndActive);

                } else
                {
                    Console.WriteLine("GetGUIThreadInfo is false.");
                }
            } else
            {
                Console.WriteLine("GetForegroundWindow handle is null.");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
        }
    }
}
