using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileScanner
{
    public partial class MainForm : Form
    {
        private bool isRunning = false;
        private bool isPaused = false;
        private string sourceDir;
        private string destDir;
        private int interval;
        byte[][] fileBytes = new byte[1024][];
        private Dictionary<string, DateTime> copiedFiles = new Dictionary<string, DateTime>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadSetup();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;
                isPaused = false;
                btnStart.Enabled = false;
                btnPause.Enabled = true;
                btnStop.Enabled = true;
                StartScanner();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (isRunning && !isPaused)
            {
                isPaused = true;
                btnPause.Text = "Resume";
            }
            else if (isRunning && isPaused)
            {
                isPaused = false;
                btnPause.Text = "Pause";
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                btnStart.Enabled = true;
                btnPause.Enabled = false;
                btnPause.Text = "Pause";
                btnStop.Enabled = false;
            }
        }

        private void StartScanner()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
            {
                while (isRunning)
                {
                    if (!isPaused)
                    {
                        string[] files = Directory.GetFiles(sourceDir, "*.raw");
                        foreach (string file in files)
                        {
                            string destFile = Path.Combine(destDir, Path.GetFileName(file));
                            if (copiedFiles.ContainsKey(destFile) && DateTime.Now - copiedFiles[destFile] < TimeSpan.FromMinutes(2))
                            {
                                continue;
                            }
                            int count = 1;
                            string newDestFile = destFile;
                            while (File.Exists(newDestFile))
                            {
                                newDestFile = Path.Combine(destDir, Path.GetFileNameWithoutExtension(destFile) + "-" + count + Path.GetExtension(destFile));
                                count++;
                            }
                            try
                            {
                                fileBytes[0] = File.ReadAllBytes(@"C:\temp3\IMAGE1_0014.raw");
                                File.Copy(file, newDestFile);
                                // File.WriteAllBytes(@"C:\temp3\BAK2\IMAGE1_"+ i.ToString().PadLeft(4, '0') +".raw", fileBytes[i]);
                            }

                            catch
                            {

                            }
                            copiedFiles[newDestFile] = DateTime.Now;
                        }
                    }
                    Thread.Sleep(interval);
                }
            }));
        }

        private void LoadSetup()
        {
            try
            {
                string[] setup = File.ReadAllLines("setup.txt");
                sourceDir = setup[0];
                destDir = setup[1];
                interval = int.Parse(setup[2]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
