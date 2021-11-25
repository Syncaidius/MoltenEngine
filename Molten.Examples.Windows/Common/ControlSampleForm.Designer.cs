namespace Molten.Samples
{
    partial class ControlSampleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.surfacePlaceholder = new System.Windows.Forms.Panel();
            this.BackgroundColor = new System.Windows.Forms.GroupBox();
            this.trackBlue = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.trackGreen = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.trackRed = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.BackgroundColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackRed)).BeginInit();
            this.SuspendLayout();
            // 
            // surfacePlaceholder
            // 
            this.surfacePlaceholder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.surfacePlaceholder.Location = new System.Drawing.Point(293, 12);
            this.surfacePlaceholder.Name = "surfacePlaceholder";
            this.surfacePlaceholder.Size = new System.Drawing.Size(741, 567);
            this.surfacePlaceholder.TabIndex = 0;
            // 
            // BackgroundColor
            // 
            this.BackgroundColor.Controls.Add(this.trackBlue);
            this.BackgroundColor.Controls.Add(this.label3);
            this.BackgroundColor.Controls.Add(this.trackGreen);
            this.BackgroundColor.Controls.Add(this.label2);
            this.BackgroundColor.Controls.Add(this.trackRed);
            this.BackgroundColor.Controls.Add(this.label1);
            this.BackgroundColor.Location = new System.Drawing.Point(12, 12);
            this.BackgroundColor.Name = "BackgroundColor";
            this.BackgroundColor.Size = new System.Drawing.Size(275, 204);
            this.BackgroundColor.TabIndex = 1;
            this.BackgroundColor.TabStop = false;
            this.BackgroundColor.Text = "Background Color";
            // 
            // trackBlue
            // 
            this.trackBlue.Location = new System.Drawing.Point(6, 146);
            this.trackBlue.Maximum = 255;
            this.trackBlue.Name = "trackBlue";
            this.trackBlue.Size = new System.Drawing.Size(263, 45);
            this.trackBlue.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Blue";
            // 
            // trackGreen
            // 
            this.trackGreen.Location = new System.Drawing.Point(6, 95);
            this.trackGreen.Maximum = 255;
            this.trackGreen.Name = "trackGreen";
            this.trackGreen.Size = new System.Drawing.Size(263, 45);
            this.trackGreen.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Green";
            // 
            // trackRed
            // 
            this.trackRed.Location = new System.Drawing.Point(6, 44);
            this.trackRed.Maximum = 255;
            this.trackRed.Name = "trackRed";
            this.trackRed.Size = new System.Drawing.Size(263, 45);
            this.trackRed.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Red";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 582);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1046, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // ControlSampleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1046, 604);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.BackgroundColor);
            this.Controls.Add(this.surfacePlaceholder);
            this.Name = "ControlSampleForm";
            this.Text = "ControlSampleForm";
            this.BackgroundColor.ResumeLayout(false);
            this.BackgroundColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackRed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel surfacePlaceholder;
        private System.Windows.Forms.GroupBox BackgroundColor;
        private System.Windows.Forms.TrackBar trackBlue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackGreen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackRed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}