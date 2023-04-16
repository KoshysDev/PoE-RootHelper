using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
namespace PoE_RootHelper;

public partial class Form1 : Form
{
    private string[] lines;
    private int currentLine;

    private NotifyIcon trayIcon = new NotifyIcon();
    private ContextMenuStrip trayMenu = new ContextMenuStrip();

    private volatile bool stopKeyThread = false;

    //keybinds
    private const Keys VK_LEFT = Keys.Left;
    private const Keys VK_RIGHT = Keys.Right;

    // dll import
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(Keys vKey);

    public Form1()
    {
        this.KeyPreview = true;
        LoadCurrentLine();
        InitializeComponent();
        InitializeTrayIcon();

        // Read the lines from the text file into an array
        if (File.Exists("root.txt"))
        {
            lines = File.ReadAllLines("root.txt");
        }
        else
        {
            lines = new string[] { "No root.txt file found" };
        }

        // Set the form's TopMost property to true to make it always on top
        this.TopMost = true;

        // hide the form's border and title bar
        this.FormBorderStyle = FormBorderStyle.None;

        Font font = new Font("Arial", 12, FontStyle.Bold);

        // Create a new label with outline effect
        Label label1 = new Label();
        // Set the label's font to the font we created above
        label1.Font = font;
        label1.Text = lines[currentLine];
        //label1.Text = lines[currentLine];
        // Set the label's location to the top left corner of the form
        label1.Location = new Point(10, 10);
        // Set the label's size to the size of the text with some padding
        label1.Size = new Size(600, 20);
        // Set the label's BackColor to transparent to show the outline effect
        label1.BackColor = Color.Transparent;
        // make label flexible
        label1.AutoSize = true;
        label1.MaximumSize = new Size(600, 0);
        // Set the label's ForeColor to white to show the outline effect
        label1.ForeColor = Color.White;

        //position the form in the top left corner of the screen
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(0, 0);

        // make app tranpa
        this.TransparencyKey = Color.Black;
        this.BackColor = Color.Black;

        this.Controls.Add(label1);
    }

    //form load
    private void Form1_Load(object sender, System.EventArgs e)
    {
        // Load the current line from the config file
        LoadCurrentLine();

        // Add an exit option to the tray menu
        trayMenu.Items.Add("Exit").Click += OnExit;
        // Set the tray icon's ContextMenuStrip to the menu we just created
        trayIcon.ContextMenuStrip = trayMenu;

        // Create a new thread to handle keybinds
        Thread keyThread = new Thread(Keybind);
        keyThread.Start();
    }

    private void InitializeTrayIcon()
    {
        // Set the icon to use for the tray icon
        trayIcon.Icon = new Icon("icon.ico");
        // Set the tooltip text for the tray icon
        trayIcon.Text = "Poe Root Helper";
        // Show the tray icon
        trayIcon.Visible = true;
        // Hide the form from the taskbar
        this.ShowInTaskbar = false;
        //show program in system tray
        this.ShowIcon = true;
    }

    private void OnExit(object? sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    //Keybind thread
    private void Keybind()
    {
        //using acii keybinds to update the label
        bool prevLeftKeyState = false;
        bool prevRightKeyState = false;

        while (!stopKeyThread)
        {
            Thread.Sleep(40);

            // keypresses using ascii keybinds and threads
            bool leftKeyState = GetAsyncKeyState(Keys.XButton1) < 0;
            bool rightKeyState = GetAsyncKeyState(Keys.XButton2) < 0;

            if (leftKeyState && !prevLeftKeyState)
            {
                if (currentLine > 0)
                {
                    currentLine--;
                }
                else
                {
                    currentLine = lines.Length - 1;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    this.Controls[0].Text = lines[currentLine];
                });
            }
            else if (rightKeyState && !prevRightKeyState)
            {
                if (currentLine < lines.Length - 1)
                {
                    currentLine++;
                }
                else
                {
                    currentLine = 0;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    this.Controls[0].Text = lines[currentLine];
                });
            }
            prevLeftKeyState = leftKeyState;
            prevRightKeyState = rightKeyState;
        }
    }

    // method to save the current line to the config file in appdata folder
    private void SaveCurrentLine()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PoE_RootHelper\\";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllText(path + "config.txt", currentLine.ToString());
    }

    // method to load the current line from the config file in appdata folder
    private void LoadCurrentLine()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PoE_RootHelper\\";
        if (File.Exists(path + "config.txt"))
        {
            //streamreader to read the config file
            using (StreamReader sr = new StreamReader(path + "config.txt"))
            {
                //read the current line from the config file
                string? line = sr.ReadLine();
                if (line != null)
                {
                    currentLine = int.Parse(line);
                }
            }
        }
        else
        {
            currentLine = 0;
            //create config file
            SaveCurrentLine();
        }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveCurrentLine();
        stopKeyThread = true;
    }
}
