using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

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

        // Audio variables
        double currentAudioTimeStamp;
        bool isPLaying;
        Panel waveFormPanel;
        List<NAudio.Gui.WaveViewer> wvs;
        List<CheckBox> muteButtons;
        List<NAudio.Wave.WaveOutEvent> waveOutEvents;
        List<NAudio.Wave.WaveFileReader> waveFileReaders;
        List<bool> waveOutMutes;
        Dictionary<int, string []> audioNames;
        Label timeLabel;
        TrackBar tb;
        TimeSpan currentTimeSpan;

        bool closingForm = false;

        // Threads
        System.Threading.Thread timeThread;
        System.Threading.Thread UpdateUIThread;

        private MethodInvoker updateTBVal;
        private MethodInvoker updateWVs;

        // Dev variables
        bool _debugMode;
        bool _writeOutputToText;
        FileStream fs;
        StreamWriter sw;
        TextWriter oldOut = Console.Out;

        public AudioSplitter() {
            InitializeComponent();
            InitializeVariables();
        }

        private void InitializeVariables() {
            updateTBVal = delegate () { 
                // Track time
                tb.Value = (int)(currentAudioTimeStamp / 1000);
                currentTimeSpan = GetActiveTrackCurrentTime();

                // Time label value
                timeLabel.Text = GetTimeLabelStr();
            };

            updateWVs = delegate () {
                foreach (var wv in this.wvs) {
                    //wv.StartPosition = (wv.WaveStream.Length / (long)wv.WaveStream.TotalTime.TotalMilliseconds) * position;
                    wv.StartPosition = (wv.WaveStream.Length / (long)(wv.WaveStream.TotalTime.TotalMilliseconds) * (long)GetActiveTrackCurrentTime().TotalMilliseconds);
                    wv.Refresh();
                }
            };


            // Defaults
            _debugMode = false;
            _writeOutputToText = true;

            stems = "0";
            currentAudioTimeStamp = 0.0f;   // in seconds
            isPLaying = false;
            outputPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            DestinationTextBox.Text = outputPath;

            // initialization for variables
            timeLabel = new Label();
            waveFormPanel = new Panel();
            waveOutEvents = new List<NAudio.Wave.WaveOutEvent>();
            waveFileReaders = new List<NAudio.Wave.WaveFileReader>();
            wvs = new List<NAudio.Gui.WaveViewer>();
            muteButtons = new List<CheckBox>();

            if (_debugMode) {
                // Currently in /bin/debug
                shellScriptPath = "spleeterHandler.bat";
                //outputPath = "..\\..";
            }
            else {
                shellScriptPath = "spleeterHandler.bat";
                // outputPath = "";
            }
            audioNames = new Dictionary<int, string []> {
                {2, new string[]{ "vocals", "accompaniment" } },
                {4, new string[]{ "vocals", "bass", "drums", "other" } },
                {5, new string[]{ "vocals", "bass", "drums", "piano", "other" } },
            };

            if (_writeOutputToText) {
                //fs = new FileStream((System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\output.txt"), FileMode.Append);
                fs = new FileStream("./output.txt", FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs);
                Console.SetOut(sw);

                Console.WriteLine("---------------------------------------");
                Console.WriteLine(DateTime.Now);
                Console.WriteLine("");
            }
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

            // Create audio files
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            //if (!_debugMode) {
            //    cmd.StartInfo.CreateNoWindow = false;
            //}
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
            cmd.WaitForExit();


            Console.WriteLine("\n\n----------Start of spleeter shell script output----------");
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            Console.WriteLine("----------End of spleeter shell script output----------\n\n");

            //if (_writeOutputToText) {
            //    Console.WriteLine("writing cmd output to " + (System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\output.txt"));
            //    using (System.IO.StreamWriter file =
            //        new System.IO.StreamWriter((System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\output.txt"), true)) {
            //        file.WriteLine("spleeter shell script output:");
            //        file.WriteLine(outputStr);
            //        file.WriteLine("end of spleeter shell script output");
            //    }
            //}

            // Change the source path if youtube link is used
            // since we do not know the name of the folder
            // generated from youtube and spleeter
            if (sourceType == "-link") {
                source = outputPath + "\\" + getSourceFolderName();
                Console.WriteLine("The source is now " + source);
            }

            InitializeWaveOuts();

            // Display wave forms
            DisplayWaveForms(Int32.Parse(stems));

            cmd.Dispose();         

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
            if (YoutubeLinkTextBox.Text == "Enter youtube link" && SourceTextBox.Text == "") {
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

        private void StopAllTracks() {
            Console.WriteLine("Stopping all tracks");

            isPLaying = false;

            foreach (var waveOutEvent in waveOutEvents) {
                waveOutEvent.Stop();
            }
        }

        private void PlayAllTracks() {
            //playAudioFile();

            Console.WriteLine("Playing all tracks");

            isPLaying = true;

            //Console.WriteLine("waveoutevents count " + waveOutEvents.Count);

            for (int i = 0; i < waveOutEvents.Count; i++) {
                if (!waveOutMutes [i]) {
                    Console.WriteLine("playing " + waveOutEvents [i].ToString());
                    waveFileReaders [i].CurrentTime = currentTimeSpan;
                    waveOutEvents [i].Play();
                }
            }

            // start the scrolling of the trackbar
            //StartUpdateUIThread();
        }

        private TimeSpan GetActiveTrackCurrentTime() {
            for (int i = 0; i < waveOutMutes.Count; i++) {
                if (!waveOutMutes [i]) {
                    return waveFileReaders [i].CurrentTime;
                }
            }

            return TimeSpan.Zero;
        }

        private String GetTimeLabelStr() {
            string t = "";

            TimeSpan ts = GetActiveTrackCurrentTime();

            if (ts.TotalSeconds < 60) {
                if (ts.TotalSeconds < 10) {
                    t += "00:0" + (int)ts.TotalSeconds;
                } else {
                    t += "00:" + (int)ts.TotalSeconds;
                }
            } else {
                if (ts.TotalMinutes < 10) {
                    t += "0" + (int)ts.TotalMinutes + ":";

                    if (ts.TotalSeconds % 60 < 10) {
                        t += "0" + (int)ts.TotalMinutes + ":0" + (int)ts.TotalSeconds;
                    }
                    else {
                        t += "0" + (int)ts.TotalMinutes + (int)ts.TotalSeconds;
                    }
                } else {
                    t += (int)ts.TotalMinutes + ":";

                    if (ts.TotalSeconds % 60 < 10) {
                        t += (int)ts.TotalMinutes + ":0" + (int)ts.TotalSeconds;
                    }
                    else {
                        t += (int)ts.TotalMinutes + (int)ts.TotalSeconds;
                    }
                }
            }

            t += "/";

            if (waveFileReaders [0].TotalTime.TotalSeconds % 60 < 10) {
                t += (int)(waveFileReaders [0].TotalTime.Minutes) + ":0" + (int)(waveFileReaders [0].TotalTime.TotalSeconds % 60);
            } else {
                t += (int)(waveFileReaders [0].TotalTime.Minutes) + ":" + (int)(waveFileReaders [0].TotalTime.TotalSeconds % 60);
            }


            return t;
        }

        // Threading
        private void StartTimeTracking() {
            timeThread = new System.Threading.Thread(AddTime);
            timeThread.Start();
        }

        private void StartUpdateUIThread() {
            UpdateUIThread = new System.Threading.Thread(UpdateUI);
            UpdateUIThread.Start();
        }

        // Threading functions
        private void AddTime() {
            while (!closingForm) {
                if (isPLaying) {
                    for (int i = 0; i < waveOutMutes.Count; i++) {
                        // is not muted
                        if (!waveOutMutes [i]) {
                            currentAudioTimeStamp = waveFileReaders [i].CurrentTime.TotalMilliseconds;
                            //Console.WriteLine("current audio time is " + currentAudioTimeStamp);
                        }
                    }
                }
            }

            timeThread.Abort();
        }

        private delegate void safeCallDelegate();

        private void UpdateUI() {
            while (!closingForm) {
                if (isPLaying) {
                    this.Invoke(updateTBVal);

                    this.Invoke(updateWVs);
                } else {
                    break;
                }

                // update line


                //if (currentAudioTimeStamp >= wvs [0].WaveStream.TotalTime.TotalSeconds) {
                //    break;
                //}
            }

            UpdateUIThread.Abort();
        }
        /*                                                  Dynamic UI                                            */

        // Controls

        // Create wave buttons
        private void CreateWaveViewer(int x, int y, int index) {
            //Console.WriteLine("Creating a waveviewer at " + y.ToString() + ", " + x.ToString());
            wvs.Add(new NAudio.Gui.WaveViewer());
            wvs[index].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            wvs [index].ForeColor = System.Drawing.Color.White;
            wvs[index].Location = new System.Drawing.Point(x, y);
            wvs[index].Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            wvs[index].Name = ("wv " + index.ToString());
            wvs[index].Size = new System.Drawing.Size(700, 70);
            wvs[index].StartPosition = 0;
            wvs[index].TabIndex = 2;
            wvs[index].WaveStream = null;
            wvs[index].Visible = true;
            wvs [index].AutoScroll = true;
            wvs[index].Show();
            this.Controls.Add(wvs[index]);
        }

        // Create mute buttons
        private void CreateWaveViewerButtons(NAudio.Gui.WaveViewer wv, int index) {
            //Console.WriteLine("Creating mute button at " + wv.Top.ToString() + ", " + wv.Left.ToString());
            muteButtons.Add(new CheckBox());
            muteButtons [index].AutoSize = true;
            muteButtons [index].ForeColor = System.Drawing.Color.White;
            muteButtons [index].Location = new System.Drawing.Point(wv.Left - 75, wv.Top + 50);
            muteButtons [index].Name = ("checkBox" + index.ToString());
            muteButtons [index].Size = new System.Drawing.Size(50, 17);
            muteButtons [index].TabIndex = 10;
            muteButtons [index].Text = ("Mute");
            muteButtons [index].UseVisualStyleBackColor = true;
            muteButtons [index].Click += new System.EventHandler(this.MuteButton_Click);
            this.Controls.Add(muteButtons [index]);
        }

        // Create labels for wave forms
        private void CreateWaveFormLabel(int x, int y, string waveFormName) {
            Label tempLabel = new Label() {
                AutoSize = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(x, y - 30),
                Name = waveFormName,
                Size = new System.Drawing.Size(57, 20),
                TabIndex = 11,
                Text = waveFormName,
                Visible = true
            };
            this.Controls.Add(tempLabel);
        }

        // Create track bar
        private void CreateTrackBar(int x, int y) {
            tb = new TrackBar() {
                Location = new System.Drawing.Point(x, y),
                Name = "tb",
                Size = new System.Drawing.Size(700, 45),
                TabIndex = 2,
                Maximum = (int)wvs [0].WaveStream.TotalTime.TotalSeconds,
                Visible = true,
            };

            tb.ValueChanged += this.TrackBar_Scroll;
            this.Controls.Add(tb);

            // Label for track bar
            timeLabel.AutoSize = true;
            timeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            timeLabel.ForeColor = System.Drawing.Color.White;
            timeLabel.Location = new System.Drawing.Point(x, y - 30);
            timeLabel.Name = "timeLabel";
            timeLabel.Size = new System.Drawing.Size(57, 20);
            timeLabel.TabIndex = 11;
            timeLabel.Text = "00:00";
            timeLabel.Visible = true;
            this.Controls.Add(timeLabel);
        }

        // Create play / pause button
        private void CreateButtons(int x, int y) {
            Button playButton = new Button() {
                AutoSize = true,
                ForeColor = System.Drawing.Color.Black,
                Location = new System.Drawing.Point(x - 100, y),
                Name = ("playPauseButton"),
                Size = new System.Drawing.Size(75, 17),
                TabIndex = 10,
                Text = ("Play/Pause"),
                UseVisualStyleBackColor = true,
            };

            playButton.Click += new System.EventHandler(this.PlayPauseButton_Click);
            this.Controls.Add(playButton);
        }

        // Control's functionality
        private void MuteButton_Click(object sender, EventArgs e) {
            var senderCheckButton = (CheckBox)sender;
            string mName = senderCheckButton.Name;
            Console.WriteLine("pressing mute button for waveform " + mName.Substring(mName.Length - 1, 1));
            int wvIndex = Int32.Parse(mName.Substring(mName.Length - 1, 1));
            
            // if not muted then mute
            if (!waveOutMutes [wvIndex]) {
                waveOutEvents [wvIndex].Stop();
                waveOutMutes [wvIndex] = true;
            } else {
                waveOutMutes [wvIndex] = false;
                //waveFileReaders [wvIndex].CurrentTime = currentTimeSpan;
                //waveOutEvents [wvIndex].Play();

                StopAllTracks();
                PlayAllTracks();

                Console.WriteLine("resuming track at " + waveFileReaders [wvIndex].CurrentTime.TotalMilliseconds);
                //waveOutEvents [wvIndex].Init(waveFileReaders [wvIndex]);
                //waveFileReaders [wvIndex].CurrentTime = TimeSpan.FromSeconds(60);
            }
        }

        private void PlayPauseButton_Click(object sender, EventArgs e) {
            if (!isPLaying) {
                StartUpdateUIThread();
                PlayAllTracks();
            }
            else {
                StopAllTracks();
            }
        }

        private void TrackBar_Scroll(object sender, EventArgs e) {
            //var senderTrackBar = (TrackBar)sender;
            //currentAudioTimeStamp = senderTrackBar.Value;
            //Console.WriteLine("Current time: " + currentAudioTimeStamp + " milliseconds");
            SetWVSPosition((int)currentAudioTimeStamp / 1000);

            // stop all tracks when user changes time stamp
            //StopAllTracks();
        }

        // Waveforms
        private void DisplayWaveForms(int stems) {
            // Where the first waveform box will be
            int x = 450;
            int y = 60;

            for (int i = 0; i < stems; i++) {
                CreateWaveViewer(x, y, i);
                CreateWaveViewerButtons(wvs[i], i);
                CreateWaveFormLabel(x, y, audioNames[stems][i]);
                PlotWaveForm(wvs [i], audioNames [stems] [i], 0);
                y += 120;
            }

            //PlotWaveForm(wvs[0], "vocals", 0);

            CreateTrackBar(x, y);
            CreateButtons(x, y);

            waveOutMutes = new List<bool>();

            for (int i = 0; i < stems; i++) {
                waveOutMutes.Add(false);
            }

            // Resize window size
            this.Size = new Size(wvs[0].Right + 50, y + 75);
        }

        private void PlotWaveForm(NAudio.Gui.WaveViewer wv, string waveFormName, int position) {
            //string filePath = outputPath + "\\spleeterOutput\\" + waveFormName + ".wav";
            //string filePath = source.Replace(".mp3", "\\") + "\\" + waveFormName + ".wav";
            string audioFilePath = outputPath + "\\" + Path.GetFileName(source).Replace(".mp3", "\\") + waveFormName + ".wav";

            Console.WriteLine("plotting " + audioFilePath + " to " + wv.Name);
            wv.SamplesPerPixel = 2000;
            wv.WaveStream = new NAudio.Wave.WaveFileReader(audioFilePath);
            // Get bytes per second times how many seconds wanted
            if (position == 0) {
                wv.StartPosition = 0;
            } else {
                wv.StartPosition = (wv.WaveStream.Length / (long)wv.WaveStream.TotalTime.Milliseconds) * position * 1000;
            }

            wv.Refresh();
        }

        private void SetWVSPosition(int position) {
            foreach (var wv in wvs) {
                //wv.StartPosition = (wv.WaveStream.Length / (long)wv.WaveStream.TotalTime.TotalMilliseconds) * position;
                wv.StartPosition = (wv.WaveStream.Length / (long)(wv.WaveStream.TotalTime.TotalMilliseconds) * (long)GetActiveTrackCurrentTime().TotalMilliseconds);
                wv.Refresh();
            }
        }


        // Waveouts
        private void InitializeWaveOuts() {
            for (int i = 0; i < Int32.Parse(stems); i++) {
                waveOutEvents.Add(new NAudio.Wave.WaveOutEvent());
                //string audioFilePath = outputPath + "\\spleeterOutput\\" + wvs [i].Name + ".wav";
                //string audioFilePath = "@" + outputPath + audioNames [Int32.Parse(stems)] [i] + ".wav";
                string audioFilePath = outputPath + "\\" + Path.GetFileName(source).Replace(".mp3", "\\") + audioNames [Int32.Parse(stems)] [i] + ".wav";

                //if (sourceType == "-file") {
                //    audioFilePath = source.Replace(".mp3", "");
                //}

                Console.WriteLine("audio path: " + audioFilePath);
                waveFileReaders.Add(new NAudio.Wave.WaveFileReader(audioFilePath));
                waveOutEvents [i].Init(waveFileReaders [i]);
            }

            StartTimeTracking();
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

                // clean-up resources from this form
                waveFormPanel.Dispose();
                timeLabel.Dispose();
                if (tb != null) {
                    tb.Dispose();
                }

                closingForm = true;
            }
            base.Dispose(disposing);
        }
    }
}