namespace ServerLab3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            server = new Server(textBox1);
        }

        Server server;

        private void ñòàğòToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task start = new Task(() => { server.Start(5000); });

            start.Start();
        }

        private void ñòîïToolStripMenuItem_Click(object sender, EventArgs e)
        {
            server.Stop();
        }
    }
}