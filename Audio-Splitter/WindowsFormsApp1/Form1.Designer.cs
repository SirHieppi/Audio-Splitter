namespace WindowsFormsApp1
{
    partial class AudioSplitter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing) {
        //    if (disposing && (components != null)) {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.YoutubeLinkTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SourceBtn = new System.Windows.Forms.Button();
            this.ProcessBtn = new System.Windows.Forms.Button();
            this.DestinationTextBox = new System.Windows.Forms.TextBox();
            this.DestinationBtn = new System.Windows.Forms.Button();
            this.SourceTextBox = new System.Windows.Forms.TextBox();
            this.sourceErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.destErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.sourceToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sourceErrorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.destErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.YoutubeLinkTextBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.SourceBtn);
            this.panel1.Controls.Add(this.ProcessBtn);
            this.panel1.Controls.Add(this.DestinationTextBox);
            this.panel1.Controls.Add(this.DestinationBtn);
            this.panel1.Controls.Add(this.SourceTextBox);
            this.panel1.Location = new System.Drawing.Point(-1, 8);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(273, 360);
            this.panel1.TabIndex = 9;
            // 
            // YoutubeLinkTextBox
            // 
            this.YoutubeLinkTextBox.BackColor = System.Drawing.Color.DimGray;
            this.YoutubeLinkTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.YoutubeLinkTextBox.Location = new System.Drawing.Point(13, 74);
            this.YoutubeLinkTextBox.Name = "YoutubeLinkTextBox";
            this.YoutubeLinkTextBox.Size = new System.Drawing.Size(247, 20);
            this.YoutubeLinkTextBox.TabIndex = 15;
            this.YoutubeLinkTextBox.Text = "Enter youtube link";
            this.YoutubeLinkTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.YoutubeLinkTextBox.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(133, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "or";
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.comboBox1.ForeColor = System.Drawing.SystemColors.Window;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Vocals + Accompaniment",
            "Vocals + Drums + Bass + Other",
            "Vocals + Drums + Bass + Piano + Other"});
            this.comboBox1.Location = new System.Drawing.Point(13, 174);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(176, 21);
            this.comboBox1.TabIndex = 13;
            this.comboBox1.Text = "Split options";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // SourceBtn
            // 
            this.SourceBtn.Location = new System.Drawing.Point(196, 31);
            this.SourceBtn.Name = "SourceBtn";
            this.SourceBtn.Size = new System.Drawing.Size(75, 23);
            this.SourceBtn.TabIndex = 11;
            this.SourceBtn.Text = "Source";
            this.SourceBtn.UseVisualStyleBackColor = true;
            this.SourceBtn.Click += new System.EventHandler(this.SourceBtn_Click);
            // 
            // ProcessBtn
            // 
            this.ProcessBtn.Location = new System.Drawing.Point(93, 293);
            this.ProcessBtn.Name = "ProcessBtn";
            this.ProcessBtn.Size = new System.Drawing.Size(75, 23);
            this.ProcessBtn.TabIndex = 10;
            this.ProcessBtn.Text = "Prcoess";
            this.ProcessBtn.UseVisualStyleBackColor = true;
            this.ProcessBtn.Click += new System.EventHandler(this.ProcessBtn_Click);
            // 
            // DestinationTextBox
            // 
            this.DestinationTextBox.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.DestinationTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.DestinationTextBox.Location = new System.Drawing.Point(13, 116);
            this.DestinationTextBox.Name = "DestinationTextBox";
            this.DestinationTextBox.Size = new System.Drawing.Size(176, 20);
            this.DestinationTextBox.TabIndex = 9;
            this.DestinationTextBox.MouseHover += new System.EventHandler(this.DestinationTextBox_MouseHover);
            // 
            // DestinationBtn
            // 
            this.DestinationBtn.Location = new System.Drawing.Point(195, 116);
            this.DestinationBtn.Name = "DestinationBtn";
            this.DestinationBtn.Size = new System.Drawing.Size(75, 23);
            this.DestinationBtn.TabIndex = 8;
            this.DestinationBtn.Text = "Destination";
            this.DestinationBtn.UseVisualStyleBackColor = true;
            this.DestinationBtn.Click += new System.EventHandler(this.DestinationBtn_Click);
            // 
            // SourceTextBox
            // 
            this.SourceTextBox.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.SourceTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.SourceTextBox.Location = new System.Drawing.Point(13, 31);
            this.SourceTextBox.Name = "SourceTextBox";
            this.SourceTextBox.Size = new System.Drawing.Size(176, 20);
            this.SourceTextBox.TabIndex = 7;
            this.SourceTextBox.MouseHover += new System.EventHandler(this.SourceTextBox_MouseHover);
            // 
            // sourceErrorProvider
            // 
            this.sourceErrorProvider.ContainerControl = this;
            // 
            // destErrorProvider
            // 
            this.destErrorProvider.ContainerControl = this;
            // 
            // sourceToolTip
            // 
            this.sourceToolTip.ShowAlways = true;
            this.sourceToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.sourceToolTip.ToolTipTitle = "Path";
            // 
            // AudioSplitter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(273, 370);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AudioSplitter";
            this.Text = "Audio Splitter";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sourceErrorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.destErrorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox SourceTextBox;
        private System.Windows.Forms.TextBox DestinationTextBox;
        private System.Windows.Forms.Button DestinationBtn;
        private System.Windows.Forms.Button ProcessBtn;
        private System.Windows.Forms.Button SourceBtn;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox YoutubeLinkTextBox;
        private System.Windows.Forms.ErrorProvider sourceErrorProvider;
        private System.Windows.Forms.ErrorProvider destErrorProvider;
        private System.Windows.Forms.ToolTip sourceToolTip;
    }
}

