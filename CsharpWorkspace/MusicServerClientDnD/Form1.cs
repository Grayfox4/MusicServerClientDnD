namespace MusicServerClientDnD;

public partial class Form1 : Form
{
    private Button hostButton;
    private Button clientButton;
    private Label titleLabel;

    public Form1()
    {
        InitializeComponent();
        InitializeCustomUI();
    }

    private void InitializeCustomUI()
    {
        // Clear default controls
        this.Controls.Clear();
        this.Text = "MusicServerClientDnD";
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(30, 30, 30);
        this.ClientSize = new Size(400, 300);

        titleLabel = new Label();
        titleLabel.Text = "Choose Mode";
        titleLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        titleLabel.ForeColor = Color.White;
        titleLabel.AutoSize = false;
        titleLabel.TextAlign = ContentAlignment.MiddleCenter;
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Height = 80;
        this.Controls.Add(titleLabel);

        hostButton = new Button();
        hostButton.Text = "Host";
        hostButton.Font = new Font("Segoe UI", 14, FontStyle.Regular);
        hostButton.Size = new Size(140, 60);
        hostButton.Location = new Point(50, 120);
        hostButton.BackColor = Color.FromArgb(60, 120, 200);
        hostButton.ForeColor = Color.White;
        hostButton.FlatStyle = FlatStyle.Flat;
        hostButton.FlatAppearance.BorderSize = 0;
        hostButton.Cursor = Cursors.Hand;
        hostButton.Click += HostButton_Click;
        this.Controls.Add(hostButton);

        clientButton = new Button();
        clientButton.Text = "Client";
        clientButton.Font = new Font("Segoe UI", 14, FontStyle.Regular);
        clientButton.Size = new Size(140, 60);
        clientButton.Location = new Point(210, 120);
        clientButton.BackColor = Color.FromArgb(60, 200, 120);
        clientButton.ForeColor = Color.White;
        clientButton.FlatStyle = FlatStyle.Flat;
        clientButton.FlatAppearance.BorderSize = 0;
        clientButton.Cursor = Cursors.Hand;
        clientButton.Click += ClientButton_Click;
        this.Controls.Add(clientButton);
    }

    private void HostButton_Click(object? sender, EventArgs e)
    {
        // Open HostForm
        var hostForm = new HostForm();
        hostForm.FormClosed += (s, args) => this.Show();
        this.Hide();
        hostForm.Show();
    }

    private void ClientButton_Click(object? sender, EventArgs e)
    {
        // Open ClientForm
        var clientForm = new ClientForm();
        clientForm.FormClosed += (s, args) => this.Show();
        this.Hide();
        clientForm.Show();
    }
}
