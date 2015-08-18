namespace DukasHistoryDownloader
{
    partial class Form1
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
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.TextBoxSymbol = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.ButtonDownloadNews = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.ButtonDownloadTicks = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(79, 30);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 22);
            this.dateTimePicker1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "From";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(48, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "To";
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Location = new System.Drawing.Point(79, 60);
            this.dateTimePicker2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(200, 22);
            this.dateTimePicker2.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Symbol";
            // 
            // TextBoxSymbol
            // 
            this.TextBoxSymbol.Location = new System.Drawing.Point(79, 89);
            this.TextBoxSymbol.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TextBoxSymbol.Name = "TextBoxSymbol";
            this.TextBoxSymbol.Size = new System.Drawing.Size(843, 22);
            this.TextBoxSymbol.TabIndex = 5;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(587, 118);
            this.button4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(201, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Download minutes only";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // ButtonDownloadNews
            // 
            this.ButtonDownloadNews.Location = new System.Drawing.Point(587, 146);
            this.ButtonDownloadNews.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ButtonDownloadNews.Name = "ButtonDownloadNews";
            this.ButtonDownloadNews.Size = new System.Drawing.Size(201, 23);
            this.ButtonDownloadNews.TabIndex = 9;
            this.ButtonDownloadNews.Text = "Download news (Fxopen)";
            this.ButtonDownloadNews.UseVisualStyleBackColor = true;
            this.ButtonDownloadNews.Click += new System.EventHandler(this.ButtonDownloadNews_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(232, 143);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(201, 31);
            this.button1.TabIndex = 10;
            this.button1.Text = "ReadWrite";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ButtonDownloadTicks
            // 
            this.ButtonDownloadTicks.Location = new System.Drawing.Point(380, 118);
            this.ButtonDownloadTicks.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ButtonDownloadTicks.Name = "ButtonDownloadTicks";
            this.ButtonDownloadTicks.Size = new System.Drawing.Size(201, 23);
            this.ButtonDownloadTicks.TabIndex = 6;
            this.ButtonDownloadTicks.Text = "Download ticks";
            this.ButtonDownloadTicks.UseVisualStyleBackColor = true;
            this.ButtonDownloadTicks.Click += new System.EventHandler(this.ButtonDownloadTicks_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 215);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ButtonDownloadNews);
            this.Controls.Add(this.ButtonDownloadTicks);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.TextBoxSymbol);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dateTimePicker2);
            this.Controls.Add(this.dateTimePicker1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextBoxSymbol;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button ButtonDownloadNews;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button ButtonDownloadTicks;

    }
}

