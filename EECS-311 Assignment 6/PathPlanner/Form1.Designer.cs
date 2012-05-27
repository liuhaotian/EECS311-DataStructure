namespace PathPlanner
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
            this.FromBox = new System.Windows.Forms.TextBox();
            this.ToBox = new System.Windows.Forms.TextBox();
            this.FindPathButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FromBox
            // 
            this.FromBox.Location = new System.Drawing.Point(12, 12);
            this.FromBox.Name = "FromBox";
            this.FromBox.Size = new System.Drawing.Size(100, 20);
            this.FromBox.TabIndex = 0;
            this.FromBox.Text = "From";
            this.FromBox.TextChanged += new System.EventHandler(this.FromBox_TextChanged);
            // 
            // ToBox
            // 
            this.ToBox.Location = new System.Drawing.Point(118, 12);
            this.ToBox.Name = "ToBox";
            this.ToBox.Size = new System.Drawing.Size(100, 20);
            this.ToBox.TabIndex = 1;
            this.ToBox.Text = "To";
            this.ToBox.TextChanged += new System.EventHandler(this.ToBox_TextChanged);
            // 
            // FindPathButton
            // 
            this.FindPathButton.Enabled = false;
            this.FindPathButton.Location = new System.Drawing.Point(231, 8);
            this.FindPathButton.Name = "FindPathButton";
            this.FindPathButton.Size = new System.Drawing.Size(75, 23);
            this.FindPathButton.TabIndex = 2;
            this.FindPathButton.Text = "Find path";
            this.FindPathButton.UseVisualStyleBackColor = true;
            this.FindPathButton.Click += new System.EventHandler(this.FindPathButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 616);
            this.Controls.Add(this.FindPathButton);
            this.Controls.Add(this.ToBox);
            this.Controls.Add(this.FromBox);
            this.Name = "Form1";
            this.Text = "Path Planner";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FromBox;
        private System.Windows.Forms.TextBox ToBox;
        private System.Windows.Forms.Button FindPathButton;
    }
}

