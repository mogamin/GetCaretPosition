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

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetGUIThreadInfo(IntPtr hTreadID, ref GUITHREADINFO lpgui);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);


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
            // タイマのインスタンスを生成
            _timer = new DispatcherTimer(); // 優先度はDispatcherPriority.Background
                                            // インターバルを設定
            _timer.Interval = TimeSpan.FromMilliseconds(100);

            // タイマメソッドを設定
            _timer.Tick += new EventHandler(getCaretPos);
            // タイマを開始
            _timer.Start();
        }


        private void getCaretPos(object sender, EventArgs e)
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd != IntPtr.Zero)
            {
                IntPtr target = GetWindowThreadProcessId(hWnd, IntPtr.Zero);

                GUITHREADINFO gti = new GUITHREADINFO();
                gti.cbSize = Marshal.SizeOf(gti);
                if (!GetGUIThreadInfo(target, ref gti))
                {
                    throw new System.ComponentModel.Win32Exception();
                }
                label1.Content = String.Format("l:{0}, t:{1}, w:{2}, h:{3}",
                    gti.rectCaret.iLeft, gti.rectCaret.iTop,
                    gti.rectCaret.iRight - gti.rectCaret.iLeft, gti.rectCaret.iBottom - gti.rectCaret.iTop);
                Point p2 = new Point(gti.rectCaret.iLeft, gti.rectCaret.iTop);

                RECT rect1 = new RECT(0, 0, 0, 0);
                GetWindowRect(gti.hwndFocus, out rect1);

                RECT rect2 = new RECT(0, 0, 0, 0);
                GetClientRect(hWnd, out rect2);
                label2.Content = String.Format("WRect l:{0}, t:{1}, r:{2}, b:{3}", rect1.iLeft, rect1.iTop, rect1.iRight, rect1.iBottom);
                label3.Content = String.Format("CRect l:{0}, t:{1}, r:{2}, b:{3}", rect2.iLeft, rect2.iTop, rect2.iRight, rect2.iBottom);

                p2.X = p2.X + rect1.iLeft;
                p2.Y = p2.Y + rect1.iTop;

                this.Left = p2.X - this.Width / 2;
                this.Top = p2.Y - this.Height / 2;

                // for calibration
                /*
                this.Left = 0 - this.Width/2;
                this.Top = 0 - this.Height/2; 
                */
                label4.Content = String.Format("Caret x:{0}, y:{1}, w:{2}, h:{3}", p2.X, p2.Y, gti.rectCaret.iRight - gti.rectCaret.iLeft, gti.rectCaret.iBottom - gti.rectCaret.iTop);
            }
            else
            {
                label1.Content = "n/a";
                label2.Content = "n/a";
                label3.Content = "n/a";
                label4.Content = "n/a";
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
        }
    }
}
