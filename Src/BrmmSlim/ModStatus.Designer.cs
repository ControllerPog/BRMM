namespace BrmmSlim
{
    partial class ModPackStatus
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModPackStatus));
            this.progressBarDownload = new System.Windows.Forms.ProgressBar();
            this.DownloadSpeedsLable = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label_Doanwloading_Mod = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBarDownload
            // 
            this.progressBarDownload.BackColor = System.Drawing.Color.Red;
            this.progressBarDownload.Location = new System.Drawing.Point(-2, 141);
            this.progressBarDownload.Name = "progressBarDownload";
            this.progressBarDownload.Size = new System.Drawing.Size(490, 15);
            this.progressBarDownload.TabIndex = 0;
            // 
            // DownloadSpeedsLable
            // 
            this.DownloadSpeedsLable.AutoSize = true;
            this.DownloadSpeedsLable.Font = new System.Drawing.Font("Poppins", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.DownloadSpeedsLable.ForeColor = System.Drawing.Color.White;
            this.DownloadSpeedsLable.Location = new System.Drawing.Point(131, 45);
            this.DownloadSpeedsLable.Name = "DownloadSpeedsLable";
            this.DownloadSpeedsLable.Size = new System.Drawing.Size(133, 23);
            this.DownloadSpeedsLable.TabIndex = 3;
            this.DownloadSpeedsLable.Text = "Downloading mod";
            this.DownloadSpeedsLable.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(113, 110);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Poppins", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(225, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(166, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Don\'t close the window.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label_Doanwloading_Mod
            // 
            this.label_Doanwloading_Mod.AutoSize = true;
            this.label_Doanwloading_Mod.Font = new System.Drawing.Font("Poppins", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_Doanwloading_Mod.ForeColor = System.Drawing.Color.White;
            this.label_Doanwloading_Mod.Location = new System.Drawing.Point(131, 12);
            this.label_Doanwloading_Mod.Name = "label_Doanwloading_Mod";
            this.label_Doanwloading_Mod.Size = new System.Drawing.Size(166, 23);
            this.label_Doanwloading_Mod.TabIndex = 6;
            this.label_Doanwloading_Mod.Text = "Don\'t close the window.";
            this.label_Doanwloading_Mod.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ModPackStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(12)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(484, 146);
            this.Controls.Add(this.label_Doanwloading_Mod);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.DownloadSpeedsLable);
            this.Controls.Add(this.progressBarDownload);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(500, 185);
            this.MinimumSize = new System.Drawing.Size(500, 185);
            this.Name = "ModPackStatus";
            this.Text = "ModStatus";
            this.Load += new System.EventHandler(this.ModStatus_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarDownload;
        private System.Windows.Forms.Label DownloadSpeedsLable;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label_Doanwloading_Mod;
    }
}