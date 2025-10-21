namespace CouchDBTest
{
    partial class AddEditTodoForm
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
            label1 = new Label();
            textBox1 = new TextBox();
            IsDoneCheckbox = new CheckBox();
            SaveBtn = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new Point(5, 5);
            label1.Margin = new Padding(0, 0, 0, 5);
            label1.Name = "label1";
            label1.Size = new Size(516, 24);
            label1.TabIndex = 0;
            label1.Text = "Title";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.WhiteSmoke;
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Location = new Point(5, 34);
            textBox1.Margin = new Padding(0, 0, 0, 10);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(516, 23);
            textBox1.TabIndex = 1;
            // 
            // IsDoneCheckbox
            // 
            IsDoneCheckbox.CheckAlign = ContentAlignment.MiddleRight;
            IsDoneCheckbox.Location = new Point(5, 67);
            IsDoneCheckbox.Margin = new Padding(0, 0, 0, 5);
            IsDoneCheckbox.Name = "IsDoneCheckbox";
            IsDoneCheckbox.Size = new Size(516, 26);
            IsDoneCheckbox.TabIndex = 2;
            IsDoneCheckbox.Text = "Is Done";
            IsDoneCheckbox.UseVisualStyleBackColor = true;
            // 
            // SaveBtn
            // 
            SaveBtn.BackColor = Color.DarkGreen;
            SaveBtn.FlatStyle = FlatStyle.Flat;
            SaveBtn.Font = new Font("Verdana", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            SaveBtn.Location = new Point(5, 98);
            SaveBtn.Margin = new Padding(0);
            SaveBtn.Name = "SaveBtn";
            SaveBtn.Size = new Size(516, 42);
            SaveBtn.TabIndex = 3;
            SaveBtn.Text = "Save";
            SaveBtn.UseVisualStyleBackColor = false;
            // 
            // AddEditTodoForm
            // 
            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(33, 33, 33);
            ClientSize = new Size(526, 148);
            Controls.Add(SaveBtn);
            Controls.Add(IsDoneCheckbox);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddEditTodoForm";
            Padding = new Padding(5);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create New Todo";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private CheckBox IsDoneCheckbox;
        private Button SaveBtn;
    }
}