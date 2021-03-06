﻿namespace Louis_Vuitton
{
    partial class LouisForm
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
            this.load = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressText = new System.Windows.Forms.Label();
            this.stockCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // load
            // 
            this.load.Location = new System.Drawing.Point(1, 1);
            this.load.Name = "load";
            this.load.Size = new System.Drawing.Size(93, 23);
            this.load.TabIndex = 0;
            this.load.Text = "Mine Data";
            this.load.UseVisualStyleBackColor = true;
            this.load.Click += new System.EventHandler(this.load_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(117, 1);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(321, 23);
            this.progressBar.TabIndex = 1;
            // 
            // progressText
            // 
            this.progressText.AutoSize = true;
            this.progressText.Location = new System.Drawing.Point(463, 6);
            this.progressText.Name = "progressText";
            this.progressText.Size = new System.Drawing.Size(24, 13);
            this.progressText.TabIndex = 2;
            this.progressText.Text = "0/0";
            // 
            // stockCheckBox
            // 
            this.stockCheckBox.AutoSize = true;
            this.stockCheckBox.Location = new System.Drawing.Point(574, 5);
            this.stockCheckBox.Name = "stockCheckBox";
            this.stockCheckBox.Size = new System.Drawing.Size(158, 17);
            this.stockCheckBox.TabIndex = 3;
            this.stockCheckBox.Text = "Get Stock Availability (Slow)";
            this.stockCheckBox.UseVisualStyleBackColor = true;
            // 
            // LouisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 611);
            this.Controls.Add(this.stockCheckBox);
            this.Controls.Add(this.progressText);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.load);
            this.Name = "LouisForm";
            this.Text = "Louis Vuitton Data Scraping";
            this.Load += new System.EventHandler(this.LouisForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button load;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressText;
        private System.Windows.Forms.CheckBox stockCheckBox;
    }
}

