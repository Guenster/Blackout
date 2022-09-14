using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace LinkInterceptor;

static class Program {
    [STAThread]
    static void Main()
    {
        int WM_NCLBUTTONDOWN = 0xA1;
        int HTCAPTION = 0x2;
        [DllImport("User32.dll")]
        static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        Form f = new Form();
        f.BackColor = Color.Black;    
        var screen = Screen.PrimaryScreen;
        f.StartPosition = FormStartPosition.Manual;
        f.Bounds = screen.Bounds;      
        f.TopMost = true;

        // Loads last saved config at startup
        try{                
            string configJson = File.ReadAllText(@"C:\ProgramData\Blackout\config.json");
            Config config = JsonSerializer.Deserialize<Config>(configJson);

            if (config.fullscreen) {
                f.FormBorderStyle = FormBorderStyle.None;
            }else{
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
            }

            f.Location = new Point(config.xPos, config.yPos);
            f.SetBounds(config.xPos, config.yPos, config.width, config.height);

        }catch{
            f.FormBorderStyle = FormBorderStyle.None;
            f.Location = screen.WorkingArea.Location;
        }

        f.MouseDown += (object? sender, MouseEventArgs e) =>
        {
            if (e.Button == MouseButtons.Middle) {
                // exit on middle click
                Application.Exit();
            }
            if (e.Button == MouseButtons.Right) {
                var me = Process.GetCurrentProcess()?.MainModule?.FileName;
                if (me == null) return;
                Process.Start(me);
            }
            if (e.Button != MouseButtons.Left) return;
            var isFull = f.FormBorderStyle == FormBorderStyle.None;
            if (e.Clicks >= 2) {
                // toggle fullscreen
                var screen = Screen.FromRectangle(new(f.Location, f.Size));
                if (isFull)
                {
                    f.FormBorderStyle = FormBorderStyle.FixedDialog;
                    f.Bounds = new(screen.Bounds.X + screen.Bounds.Width / 4, screen.Bounds.Y + screen.Bounds.Height / 4, screen.Bounds.Width/2, screen.Bounds.Height/2);
                }
                else
                {
                    f.FormBorderStyle = FormBorderStyle.None;
                    f.Bounds = screen.Bounds;

                }
            } else {
                // click and drag support
                if (isFull) return;
                ReleaseCapture();
                SendMessage(f.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        };

        f.KeyDown += (object? sender, KeyEventArgs e) =>
        {
            // save config (fullscreen, position, ...)
            if (e.KeyCode == Keys.S) {
                
                // Save config as object
                Config config = new Config();        

                if (f.FormBorderStyle == FormBorderStyle.FixedDialog) {
                    config.fullscreen = false;
                } else {
                    config.fullscreen = true;
                }

                config.xPos = f.Location.X;
                config.yPos = f.Location.Y;
                
                config.width = f.Width;
                config.height = f.Height;

                // Save config in json-file
                string configJson = JsonSerializer.Serialize(config);
                Console.WriteLine("ConfigJson: " + configJson);
                File.WriteAllText(@"C:\ProgramData\Blackout\config.json", configJson);
            }
        };

        Application.EnableVisualStyles();
        Application.Run(f);
    }
}