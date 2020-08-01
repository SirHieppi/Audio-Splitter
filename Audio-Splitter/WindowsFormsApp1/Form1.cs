using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AudioSplitter : Form
    {
        // Variables for building command
        string source; // path or youtube link
        string sourceType; // -link or -file
        static string shellScriptPath;
        string outputPath;
        string stems;

        // Dev variables
        bool _debugMode;
        bool _writeOutputToText;
        FileStream fs;
        StreamWriter sw;
        TextWriter oldOut = Console.Out;

        // Dynamic waveform UI
        DynamicWaveFormDisplay wfDisplay;

        public AudioSplitter() {
            InitializeComponent();
            InitializeVariables();
            SetDefaults();
        }

        private void SetDefaults() {
            // Defaults
            _debugMode = true;
            _writeOutputToText = false;

            stems = "0";
            outputPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            DestinationTextBox.Text = outputPath;
        }

        private void InitializeVariables() {
            // initialization for variables
            wfDisplay = new DynamicWaveFormDisplay(this);

            if (_debugMode) {
                // Currently in /bin/debug
                shellScriptPath = "spleeterHandler.bat";
                //outputPath = "..\\..";
            }
            else {
                shellScriptPath = "spleeterHandler.bat";
                // outputPath = "";
            }

            //if (_writeOutputToText) {
            //    //fs = new FileStream((System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\output.txt"), FileMode.Append);
            //    fs = new FileStream("./output.txt", FileMode.Append, FileAccess.Write);
            //    sw = new StreamWriter(fs);
            //    Console.SetOut(sw);

            //    Console.WriteLine("---------------------------------------");
            //    Console.WriteLine(DateTime.Now);
            //    Console.WriteLine("");
            //}
        }

        // UI for user input
        private void DestinationBtn_Click(object sender, EventArgs e) {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();

            outputPath = fbd.SelectedPath.ToString();

            DestinationTextBox.Text = outputPath;

            fbd.Dispose();
        }

        private void ProcessBtn_Click(object sender, EventArgs e) {
            if (!CheckUserInputs()) {
                return;
            }

            wfDisplay.ResetWaveFormUI();

            // Create audio files
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = false;
            if (!_debugMode) {
                cmd.StartInfo.CreateNoWindow = false;
            }
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("pwd");

            if (_debugMode) {
                cmd.StandardInput.WriteLine("cd ../../");
            }
            Console.WriteLine("command to run bat is " + CreateCommand());
            cmd.StandardInput.WriteLine(CreateCommand());
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            //cmd.WaitForExit();

            //Console.WriteLine("\n\n----------Start of spleeter shell script output----------");
            //Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            //Console.WriteLine("----------End of spleeter shell script output----------\n\n");

            if (_writeOutputToText) {
                Console.WriteLine("writing cmd output to " + (System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\output.txt"));
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter((System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\output.txt"), true)) {
                    file.WriteLine("spleeter shell script output:");
                    file.WriteLine(cmd.StandardOutput.ReadToEnd());
                    file.WriteLine("end of spleeter shell script output");
                }
            }

            while (true) {
                if (cmd.HasExited) {
                    //aTimer.Enabled = false;
                    ProcessBtn.Text = "Process";

                    // Change the source path if youtube link is used
                    // since we do not know the name of the folder
                    // generated from youtube and spleeter

                    if (sourceType == "-link") {
                        source = outputPath + "\\" + getSourceFolderName();
                        Console.WriteLine("The source is now " + source);
                    }

                    wfDisplay.InitializeWaveOuts(outputPath, source, stems);

                    // Display wave forms
                    wfDisplay.DisplayWaveForms(Int32.Parse(stems), outputPath, source);

                    cmd.Dispose();

                    break;
                }
            }        

        }

        private void SourceBtn_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog() {
                Filter = "mp3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                FilterIndex = 2
            };

            fd.ShowDialog();

            sourceType = "-file";
            source = fd.FileName.ToString();
            SourceTextBox.Text = source;

            fd.Dispose();

            sourceErrorProvider.SetError(SourceTextBox, "");
        }

        private void DestinationTextBox_MouseHover(object sender, EventArgs e) {
            sourceToolTip.Show(outputPath, DestinationTextBox);
        }

        private void SourceTextBox_MouseHover(object sender, EventArgs e) {
            sourceToolTip.Show(source, SourceTextBox);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e) {

            sourceType = "-link";
            source = YoutubeLinkTextBox.Text;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            int splitOption = comboBox1.SelectedIndex;

            switch (splitOption) {
                case 0:
                    // Vocals (singing voice) / accompaniment separation
                    stems = "2";
                    break;
                case 1:
                    // Vocals / drums / bass / other separation
                    stems = "4";
                    break;
                case 2:
                    // Vocals / drums / bass / piano / other separation
                    stems = "5";
                    break;
                default:
                    stems = "2";
                    break;
            }

            Console.WriteLine("User has selected " + stems + " stems.");
        }

        // Helper Functions

        // Returns the name of the file downloaded from youtube-dl
        // which is also the name of the folder that spleeter generates
        private string getSourceFolderName() {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            //if (!_debugMode) {
            //    cmd.StartInfo.CreateNoWindow = false;
            //}
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            if (_debugMode) {
                cmd.StandardInput.WriteLine("cd ../../spleeterEnvP362");
            } else {
                cmd.StandardInput.WriteLine("cd spleeterEnvP362");
            }

            Console.WriteLine("getting folder name from " + source);

            string cmdStr = "python.exe youtube-dl --get-filename -o '%(title)s.mp3' " + source + " --restrict-filenames";
            Console.WriteLine("command is " + cmdStr);
            cmd.StandardInput.WriteLine(cmdStr);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            // ignore lines
            //cmd.StandardOutput.ReadLine();
            //cmd.StandardOutput.ReadLine();

            for (int i = 0; i < 6; i++) {
                Console.WriteLine("Reading next line..." + i.ToString());
                Console.WriteLine(cmd.StandardOutput.ReadLine());
            }

            return cmd.StandardOutput.ReadLine().Replace(".webm", ".mp3").Replace("'", "");
        }

        private bool CheckUserInputs() {
            // no youtube link or source provided
            if (source == "") {
                string sourceErrorText = "Please choose source or enter youtube link";
                sourceErrorProvider.SetError(SourceTextBox, sourceErrorText);

                return false;
            }

            // destination has not been selected
            if (DestinationTextBox.Text == "") {
                string destErrorText = "Please select a destination";
                destErrorProvider.SetError(DestinationTextBox, destErrorText);

                return false;
            }

            return true;
        }

        private string CreateCommand() {
            if (stems == "0") {
                stems = "2";
            }

            string command = (
                //"bash " +
                shellScriptPath + " " +
                sourceType + " \"" +
                source + "\" \"" +
                outputPath + "\" " +
                stems);

            command = command.Replace("\\", "/");
            //command = command.Replace("(", "\\(");
            //command = command.Replace(")", "\\)");

            Console.WriteLine(command);

            return command;
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {

                if (_writeOutputToText) {
                    Console.SetOut(oldOut);
                    sw.Close();
                    fs.Close();
                }

                // clean up resources from designer
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}