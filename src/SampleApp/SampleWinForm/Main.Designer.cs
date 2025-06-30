namespace SampleWinForm
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            checkBox1 = new CheckBox();
            groupBox1 = new GroupBox();
            label2 = new Label();
            textBox2 = new TextBox();
            label1 = new Label();
            textBox1 = new TextBox();
            button2 = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(1302, 918);
            button1.Name = "button1";
            button1.Size = new Size(131, 40);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(56, 33);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(145, 32);
            checkBox1.TabIndex = 1;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.ActiveCaption;
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Location = new Point(57, 62);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(414, 169);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(56, 114);
            label2.Name = "label2";
            label2.Size = new Size(73, 28);
            label2.TabIndex = 5;
            label2.Text = "label2";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(201, 111);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(175, 34);
            textBox2.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(56, 68);
            label1.Name = "label1";
            label1.Size = new Size(73, 28);
            label1.TabIndex = 3;
            label1.Text = "label1";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(201, 65);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(175, 34);
            textBox1.TabIndex = 2;
            // 
            // button2
            // 
            button2.Location = new Point(1471, 918);
            button2.Name = "button2";
            button2.Size = new Size(131, 40);
            button2.TabIndex = 3;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(13F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.AppWorkspace;
            ClientSize = new Size(1656, 986);
            Controls.Add(button2);
            Controls.Add(groupBox1);
            Controls.Add(button1);
            Name = "Main";
            Text = "Do Something？";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private CheckBox checkBox1;
        private GroupBox groupBox1;
        private Label label1;
        private TextBox textBox1;
        private Button button2;
        private Label label2;
        private TextBox textBox2;
    }
}
