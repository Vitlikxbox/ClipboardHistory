using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class ClipboardHistory : Form
{
    [DllImport("user32.dll")] static extern bool AddClipboardFormatListener(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

    const int WM_CLIPBOARDUPDATE = 0x031D, WM_HOTKEY = 0x0312, EM_SETCUEBANNER = 0x1501;
    const int HK_SHOW = 1;
    const uint MOD_CONTROL = 0x2, MOD_SHIFT = 0x4, VK_V = 0x56;
    const int MaxItems = 100;

    List<string> items = new List<string>();
    List<string> visible = new List<string>();
    TextBox searchBox;
    ListBox list;
    Label statusLabel;
    NotifyIcon tray;
    bool reallyExit;

    ClipboardHistory()
    {
        Text = "История буфера обмена";
        ClientSize = new Size(420, 490);
        MinimumSize = new Size(320, 300);
        Font = new Font("Segoe UI", 10);
        StartPosition = FormStartPosition.CenterScreen;

        searchBox = new TextBox { Location = new Point(15, 15), Width = 390, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        searchBox.TextChanged += (s, e) => Refill();
        searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Down && list.Items.Count > 0) { list.Focus(); list.SelectedIndex = 0; e.Handled = true; } };
        Controls.Add(searchBox);

        list = new ListBox
        {
            Location = new Point(15, 50), Size = new Size(390, 360), IntegralHeight = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        list.DoubleClick += (s, e) => CopySelected();
        list.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { CopySelected(); e.Handled = true; }
            else if (e.KeyCode == Keys.Delete) { DeleteSelected(); e.Handled = true; }
        };
        Controls.Add(list);

        Controls.Add(new Label
        {
            Text = "Ctrl+Shift+V — открыть · Enter/двойной клик — скопировать · Del — удалить",
            ForeColor = Color.Gray, Location = new Point(15, 418), Size = new Size(390, 36),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        });
        statusLabel = new Label { Location = new Point(15, 462), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
        Controls.Add(statusLabel);
        var clear = new Button { Text = "Очистить", Location = new Point(305, 455), Size = new Size(100, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
        clear.Click += (s, e) => { items.Clear(); Refill(); };
        Controls.Add(clear);

        var menu = new ContextMenuStrip();
        menu.Items.Add("Открыть", null, (s, e) => ShowWindow());
        menu.Items.Add("Выход", null, (s, e) => { reallyExit = true; Close(); });
        tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "История буфера обмена", Visible = true, ContextMenuStrip = menu };
        tray.DoubleClick += (s, e) => ShowWindow();

        Refill();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        SendMessage(searchBox.Handle, EM_SETCUEBANNER, (IntPtr)1, "Поиск…");
        AddClipboardFormatListener(Handle);
        RegisterHotKey(Handle, HK_SHOW, MOD_CONTROL | MOD_SHIFT, VK_V);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_CLIPBOARDUPDATE) OnClipboardChanged();
        else if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HK_SHOW) ShowWindow();
        base.WndProc(ref m);
    }

    void OnClipboardChanged()
    {
        string text = null;
        for (int attempt = 0; attempt < 5 && text == null; attempt++)
        {
            try
            {
                if (!Clipboard.ContainsText()) return;
                text = Clipboard.GetText();
            }
            catch (ExternalException) { System.Threading.Thread.Sleep(20); } // clipboard locked by another app
        }
        if (string.IsNullOrEmpty(text) || text.Trim().Length == 0) return;

        items.Remove(text);
        items.Insert(0, text);
        if (items.Count > MaxItems) items.RemoveAt(items.Count - 1);
        Refill();
    }

    static string Preview(string text)
    {
        string one = text.Replace("\r\n", " ⏎ ").Replace('\n', ' ').Replace('\t', ' ').Trim();
        return one.Length > 150 ? one.Substring(0, 150) + "…" : one;
    }

    void Refill()
    {
        string q = searchBox.Text.Trim();
        visible = q.Length == 0 ? new List<string>(items) : items.FindAll(t => t.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
        list.BeginUpdate();
        list.Items.Clear();
        foreach (var t in visible) list.Items.Add(Preview(t));
        list.EndUpdate();
        statusLabel.Text = "Записей: " + items.Count;
    }

    void CopySelected()
    {
        int i = list.SelectedIndex;
        if (i < 0) return;
        try
        {
            Clipboard.SetText(visible[i]); // the listener then moves it to the top
            statusLabel.Text = "Скопировано";
            WindowState = FormWindowState.Minimized;
        }
        catch (ExternalException) { statusLabel.Text = "Буфер обмена занят"; }
    }

    void DeleteSelected()
    {
        int i = list.SelectedIndex;
        if (i < 0) return;
        items.Remove(visible[i]);
        Refill();
        if (list.Items.Count > 0) list.SelectedIndex = Math.Min(i, list.Items.Count - 1);
    }

    void ShowWindow()
    {
        Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
        searchBox.Focus();
        searchBox.SelectAll();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!reallyExit && e.CloseReason == CloseReason.UserClosing)
        {
            // keep collecting in the background; exit from the tray menu
            e.Cancel = true;
            Hide();
            tray.ShowBalloonTip(2000, "История буфера обмена", "Работаю в трее. Ctrl+Shift+V — открыть.", ToolTipIcon.Info);
            return;
        }
        RemoveClipboardFormatListener(Handle);
        UnregisterHotKey(Handle, HK_SHOW);
        tray.Visible = false;
        tray.Dispose();
        base.OnFormClosing(e);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new ClipboardHistory());
    }
}
