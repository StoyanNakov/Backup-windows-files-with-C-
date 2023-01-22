using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
          public Form1()
        {
            
            InitializeComponent();
            label3.Text = sourceDir;
            label4.Text = destDir;
            numericUpDown1.Value = int.Parse(hours);
            numericUpDown2.Value = int.Parse(Minutes);
            if (auto_start == 0) { checkBox1.Checked = false;} else { checkBox1.Checked = true; }
            timer1.Start();
            if (overwriteExistingFile == true) { checkBox2.Checked = true; }
            else { checkBox2.Checked = false; }
        }

        string sourceDir = ConfigurationManager.AppSettings.Get("SourceDir");
        string destDir = ConfigurationManager.AppSettings.Get("DestDir");
        string hours = ConfigurationManager.AppSettings.Get("Hours");
        string Minutes = ConfigurationManager.AppSettings.Get("Minutes");
        int auto_start = int.Parse(ConfigurationManager.AppSettings.Get("AutoStart"));
        bool overwriteExistingFile = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("overwriteExistingFile"));
        
        string nHours = "";
        string nMinutes = "";

        private void buttonSourceDirectory(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var value = "";
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (folderBrowserDialog1.SelectedPath.Length > 4)
                {
                    value = folderBrowserDialog1.SelectedPath + "\\";
                }
                else 
                {
                    value = folderBrowserDialog1.SelectedPath;
                }
                label3.Text = value;
                settings["SourceDir"].Value = value;
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                sourceDir = value;

            }
        }
        private void buttonDestinationDirectory(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                var value = "";
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (folderBrowserDialog2.SelectedPath.Length > 4)
                {
                    value = folderBrowserDialog2.SelectedPath + "\\";
                }
                else
                {
                    value = folderBrowserDialog2.SelectedPath;
                }
                label4.Text = value;
                settings["destDir"].Value = value;
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                destDir = value;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            var value = numericUpDown1.Value;
            settings["Hours"].Value = value.ToString();
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            hours = value.ToString();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            var value = numericUpDown2.Value;
            settings["Minutes"].Value = value.ToString();
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            Minutes = value.ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false) { auto_start = 0; }
            if (checkBox1.Checked == true) { auto_start = 1; }

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            settings["AutoStart"].Value = auto_start.ToString();

            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        private void buttonManualBackup(object sender, EventArgs e)
        {
            DisableButtonsClick();
            label13.Text = DateTime.Now.ToString("MM/dd/yyyy H:mm");
            Form1.ActiveForm.Text = "File Backup Software --- Running";                  
            Thread t = new Thread(() => { Backup(GetAllFiles(sourceDir));});
            t.Start();
            //t.Join();
            label11.Text = "0";
            listBox1.Items.Clear();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            timer1.Start();
            
            nHours = DateTime.Now.Hour.ToString();
            nMinutes = DateTime.Now.Minute.ToString();

            if (hours == nHours && Minutes == nMinutes && auto_start == 1 && DateTime.Now.Second.ToString() == "0") 
            {
                DisableButtonsClick();   
                label13.Text = DateTime.Now.ToString("MM/dd/yyyy H:mm");
                Form1.ActiveForm.Text = "File Backup Software --- Running";
                //string[] allSourceFileName = new string[] { };

              //  Thread t = new Thread(() => { countAllFiles = GetAllFiles(SourceDir, destDir); });
              //  t.Start();
            }

        }
        private List<String> GetAllFiles(string sDir)
        {
            List<String> allFilesInFolder = new List<String>();
            try
            {
                foreach (string filesInDirectory in Directory.GetFiles(sDir))
                {
                    allFilesInFolder.Add(filesInDirectory);
                }
                foreach (string directory in Directory.GetDirectories(sDir))
                {
                    
                    if (directory + @"\" == destDir)
                    {
                        break;
                    }
                    else { allFilesInFolder.AddRange(GetAllFiles(directory)); }
                }
            }
            catch (System.Exception excpt)
            {
                this.Invoke((MethodInvoker)(() => listBox1.Items.Add(excpt.Message)));
                
            }
            return allFilesInFolder;
        }
        public void Backup(List<string> allSourceFileNames)
        { 
            string dirName = "";
            string destinationDir = destDir;
            try
            {
                int dirLength = this.sourceDir!.Length;
                int modifidetFiles = 0;
                
                label9.Text = allSourceFileNames.Count.ToString();
                int countChekedFiles = 0;
                foreach (string fileNameWithPath in allSourceFileNames)
                {
                    string dateTimeNowString = DateTime.Now.ToString("dd-MM-yyyy");
                    DateTime lastWriteForFile = File.GetLastWriteTime(fileNameWithPath);
                    string lastWriteForFileInString = lastWriteForFile.ToString("dd-MM-yyyy");
                    DateTime creationTimeForFiles = File.GetCreationTime(fileNameWithPath);
                    string creationTimeForFileInString = creationTimeForFiles.ToString("dd-MM-yyyy");
                    countChekedFiles++;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = allSourceFileNames.Count;

                    if ((lastWriteForFileInString == dateTimeNowString || creationTimeForFileInString == dateTimeNowString))
                    {
                        string fileNameWithoutExtension = (Path.GetFileNameWithoutExtension(fileNameWithPath));
                        string fileExtension = Path.GetExtension(fileNameWithPath);
                        int FileNameLength = fileNameWithoutExtension.Length;
                        string newFileName = fileNameWithoutExtension.Insert(FileNameLength, "-backup-");
                        newFileName = newFileName.Insert(FileNameLength + 8, dateTimeNowString);
                        newFileName += fileExtension;
                        dirName = Path.GetDirectoryName(fileNameWithPath);
                        dirName = dirName!.Substring(dirLength);
                        
                        string backupFileWithPath = destinationDir + dirName + @"\" + newFileName;
                        
                        if (File.Exists(destinationDir + dirName + @"\" + fileNameWithoutExtension + fileExtension))
                        {
                            try
                            {
                                File.Move(destinationDir + dirName + @"\" + fileNameWithoutExtension + fileExtension, backupFileWithPath, overwriteExistingFile);
                            }
                            catch (IOException) 
                            {
                                this.Invoke((MethodInvoker)(() => listBox1.Items.Add("File Exists: " + destinationDir + dirName + @"\" + fileNameWithoutExtension + fileExtension))); 
                            }
                            catch (UnauthorizedAccessException) 
                            {
                                this.Invoke((MethodInvoker)(() => listBox1.Items.Add("Not Permision")));
                                 
                            }
                        }

                        if (!Directory.Exists(destinationDir + destinationDir + dirName + @"\" + fileNameWithoutExtension + fileExtension))
                        {
                            Directory.CreateDirectory(destinationDir + dirName);
                        }

                        File.Copy(fileNameWithPath, destinationDir + dirName + @"\" + fileNameWithoutExtension + fileExtension, true);

                        this.Invoke((MethodInvoker)(() => listBox1.Items.Add(fileNameWithPath)));
                        


                        modifidetFiles++;
                        label11.Text = modifidetFiles.ToString();
                    }
                    label7.Text = countChekedFiles.ToString();
                    progressBar1.Value = countChekedFiles;
                }

            }
            catch (Exception e)
            {
                this.Invoke((MethodInvoker)(() => listBox1.Items.Add(e.ToString())));
            }
            //if (listBox1.Items.Count > 200)
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        listBox1.Items.RemoveAt(i);
            //    }
            //}

            this.Invoke((MethodInvoker)(() => EnableButtonsClick())); 
        }

        public void DisableButtonsClick()
        { 
            checkBox1.Enabled= false;
            checkBox2.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            Application.UseWaitCursor = true;
        }

        public void EnableButtonsClick()
        {
            checkBox1.Enabled= true;
            checkBox2.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            Application.UseWaitCursor = false;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            int x = this.Size.Width;
            int y = this.Size.Height;
            listBox1.Width = x - 260;
            listBox1.Height = y - 260;
        }

     

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == false) { overwriteExistingFile = false; }
            if (checkBox2.Checked == true) { overwriteExistingFile = true; }
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            settings["overwriteExistingFile"].Value = overwriteExistingFile.ToString();

            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int x = this.Size.Width;
            int y = this.Size.Height;
            listBox1.Width = x-260;
            listBox1.Height = y-260;   
        }
    }
    
}