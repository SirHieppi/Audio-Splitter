using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class DynamicWaveFormDisplay
    {
        // Audio variables
        double currentAudioTimeStamp;
        bool isPLaying;
        Panel waveFormPanel;
        List<NAudio.Gui.WaveViewer> wvs;
        List<Label> waveFormLabels;
        List<CheckBox> muteButtons;
        List<NAudio.Wave.WaveOutEvent> waveOutEvents;
        List<NAudio.Wave.WaveFileReader> waveFileReaders;
        List<bool> waveOutMutes;
        Dictionary<int, string []> audioNames;
        Label timeLabel;
        TimeSpan currentTimeSpan;

        // Visuals
        TrackBar tb;
        Button playPause;

        bool closingForm = false;

        // Threads
        System.Threading.Thread timeThread;
        System.Threading.Thread UpdateUIThread;

        private MethodInvoker updateTBVal;
        private MethodInvoker updateWVs;

        private bool reset = false;

        private Form form;

        public DynamicWaveFormDisplay(Form f) {
            InitializeVariables();
            form = f;

            audioNames = new Dictionary<int, string []> {
                {2, new string[]{ "vocals", "accompaniment" } },
                {4, new string[]{ "vocals", "bass", "drums", "other" } },
                {5, new string[]{ "vocals", "bass", "drums", "piano", "other" } },
            };
        }

        public void ResetWaveFormUI() {
            Console.WriteLine("Resetting ui");
            reset = true;

            // Remove waveforms, buttons, labels
            for (int i = 0; i < wvs.Count; i++) {
                form.Controls.Remove(wvs [i]);
                form.Controls.Remove(muteButtons [i]);
                form.Controls.Remove(waveFormLabels [i]);
            }
            form.Controls.Remove(tb);
            form.Controls.Remove(playPause);
            form.Controls.Remove(timeLabel);


            wvs.Clear();
            muteButtons.Clear();
            waveFormLabels.Clear();
            waveFileReaders.Clear();
            waveOutEvents.Clear();


            // Remove wave viewers
            for (int i = 0; i < wvs.Count; i++) {
                form.Controls.Remove(wvs [i]);
            }

            isPLaying = false;
            InitializeVariables();

            reset = false;
            Console.WriteLine("waveoutevents count: {0}", waveOutEvents.Count);
        }

        public void DisplayWaveForms(int stems, string outputPath, string source) {
            // Where the first waveform box will be
            int x = 450;
            int y = 60;

            for (int i = 0; i < stems; i++) {
                CreateWaveViewer(x, y, i);
                CreateWaveViewerButtons(wvs [i], i);
                CreateWaveFormLabel(x, y, audioNames [stems] [i]);
                PlotWaveForm(wvs [i], audioNames [stems] [i], 0, outputPath, source);
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
            form.Size = new Size(wvs [0].Right + 50, y + 75);
        }

        public void InitializeWaveOuts(string outputPath, string source, string stems) {
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

        private void InitializeVariables() {
            // initialization for variables
            timeLabel = new Label();
            waveFormPanel = new Panel();
            waveFormLabels = new List<Label>();
            waveOutEvents = new List<NAudio.Wave.WaveOutEvent>();
            waveFileReaders = new List<NAudio.Wave.WaveFileReader>();
            wvs = new List<NAudio.Gui.WaveViewer>();
            muteButtons = new List<CheckBox>();
            currentAudioTimeStamp = 0.0f;
            currentTimeSpan = new TimeSpan();

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
        }

        private void StopAllTracks() {
            Console.WriteLine("Stopping all tracks");

            isPLaying = false;

            foreach (var waveOutEvent in waveOutEvents) {
                waveOutEvent.Stop();
            }

            //System.Threading.Thread.Sleep(250);
        }

        private void PlayAllTracks() {
            //playAudioFile();

            //System.Threading.Thread.Sleep(250);

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
            StartUpdateUIThread();
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
                    t += "0:0" + (int)ts.TotalSeconds;
                }
                else {
                    t += "0:" + (int)ts.TotalSeconds;
                }
            }
            else {
                if (ts.TotalSeconds % 60 < 10) {
                    t += (int)ts.TotalMinutes + ":0" + ((int)ts.TotalSeconds % 60);
                }
                else {
                    t += (int)ts.TotalMinutes + ":" + ((int)ts.TotalSeconds % 60);
                }
            }

            // Total song length
            t += "/";

            if (waveFileReaders [0].TotalTime.TotalSeconds % 60 < 10) {
                t += (int)(waveFileReaders [0].TotalTime.Minutes) + ":0" + (int)(waveFileReaders [0].TotalTime.TotalSeconds % 60);
            }
            else {
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
        private delegate void safeCallDelegate();
        private void AddTime() {
            while (!closingForm && !reset && isPLaying) {
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

        // Updates UI every second
        private void UpdateUI() {
            while (!closingForm && !reset && isPLaying) {
                if (isPLaying) {
                    form.Invoke(updateTBVal);

                    form.Invoke(updateWVs);
                    System.Threading.Thread.Sleep(1000);
                }
                else {
                    break;
                }
            }

            UpdateUIThread.Abort();
        }

        // Dynamic UI Creation functions

        // Controls

        // Create wave buttons
        private void CreateWaveViewer(int x, int y, int index) {
            //Console.WriteLine("Creating a waveviewer at " + y.ToString() + ", " + x.ToString());
            wvs.Add(new NAudio.Gui.WaveViewer());
            wvs [index].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            wvs [index].ForeColor = System.Drawing.Color.White;
            wvs [index].Location = new System.Drawing.Point(x, y);
            wvs [index].Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            wvs [index].Name = ("wv " + index.ToString());
            wvs [index].Size = new System.Drawing.Size(700, 70);
            wvs [index].StartPosition = 0;
            wvs [index].TabIndex = 2;
            wvs [index].WaveStream = null;
            wvs [index].Visible = true;
            wvs [index].AutoScroll = true;
            wvs [index].Show();
            form.Controls.Add(wvs [index]);
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
            form.Controls.Add(muteButtons [index]);
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
            waveFormLabels.Add(tempLabel);
            form.Controls.Add(tempLabel);
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
            form.Controls.Add(tb);

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
            form.Controls.Add(timeLabel);
        }

        // Create play / pause button
        private void CreateButtons(int x, int y) {
            playPause = new Button() {
                AutoSize = true,
                ForeColor = System.Drawing.Color.Black,
                Location = new System.Drawing.Point(x - 100, y),
                Name = ("playPauseButton"),
                Size = new System.Drawing.Size(75, 17),
                TabIndex = 10,
                Text = ("Play/Pause"),
                UseVisualStyleBackColor = true,
            };
            playPause.Click += new System.EventHandler(this.PlayPauseButton_Click);
            form.Controls.Add(playPause);
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
            }
            else {
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

        private void PlotWaveForm(NAudio.Gui.WaveViewer wv, string waveFormName, int position, string outputPath, string source) {
            //string filePath = outputPath + "\\spleeterOutput\\" + waveFormName + ".wav";
            //string filePath = source.Replace(".mp3", "\\") + "\\" + waveFormName + ".wav";
            string audioFilePath = outputPath + "\\" + Path.GetFileName(source).Replace(".mp3", "\\") + waveFormName + ".wav";

            Console.WriteLine("plotting " + audioFilePath + " to " + wv.Name);
            wv.SamplesPerPixel = 2000;
            wv.WaveStream = new NAudio.Wave.WaveFileReader(audioFilePath);
            // Get bytes per second times how many seconds wanted
            if (position == 0) {
                wv.StartPosition = 0;
            }
            else {
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
    }
}
