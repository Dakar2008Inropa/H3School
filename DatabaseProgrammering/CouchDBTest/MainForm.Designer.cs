namespace CouchDBTest
{
    partial class MainForm
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
            CreateNewTodoBtn = new Button();
            EditSelectedTodoBtn = new Button();
            DeleteSelectedTodoBtn = new Button();
            TodoDataGridView = new DataGridView();
            HideIdAndRevCheckbox = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)TodoDataGridView).BeginInit();
            SuspendLayout();
            // 
            // CreateNewTodoBtn
            // 
            CreateNewTodoBtn.BackColor = Color.Green;
            CreateNewTodoBtn.FlatStyle = FlatStyle.Flat;
            CreateNewTodoBtn.Font = new Font("Verdana", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            CreateNewTodoBtn.ForeColor = Color.White;
            CreateNewTodoBtn.Location = new Point(5, 5);
            CreateNewTodoBtn.Margin = new Padding(0, 0, 5, 5);
            CreateNewTodoBtn.Name = "CreateNewTodoBtn";
            CreateNewTodoBtn.Size = new Size(184, 36);
            CreateNewTodoBtn.TabIndex = 0;
            CreateNewTodoBtn.Text = "Create New Todo";
            CreateNewTodoBtn.UseVisualStyleBackColor = false;
            // 
            // EditSelectedTodoBtn
            // 
            EditSelectedTodoBtn.BackColor = Color.MediumBlue;
            EditSelectedTodoBtn.FlatStyle = FlatStyle.Flat;
            EditSelectedTodoBtn.Font = new Font("Verdana", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            EditSelectedTodoBtn.ForeColor = Color.White;
            EditSelectedTodoBtn.Location = new Point(194, 5);
            EditSelectedTodoBtn.Margin = new Padding(0, 0, 5, 5);
            EditSelectedTodoBtn.Name = "EditSelectedTodoBtn";
            EditSelectedTodoBtn.Size = new Size(184, 36);
            EditSelectedTodoBtn.TabIndex = 1;
            EditSelectedTodoBtn.Text = "Edit Selected Todo";
            EditSelectedTodoBtn.UseVisualStyleBackColor = false;
            // 
            // DeleteSelectedTodoBtn
            // 
            DeleteSelectedTodoBtn.BackColor = Color.Firebrick;
            DeleteSelectedTodoBtn.FlatStyle = FlatStyle.Flat;
            DeleteSelectedTodoBtn.Font = new Font("Verdana", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            DeleteSelectedTodoBtn.ForeColor = Color.White;
            DeleteSelectedTodoBtn.Location = new Point(383, 5);
            DeleteSelectedTodoBtn.Margin = new Padding(0, 0, 5, 5);
            DeleteSelectedTodoBtn.Name = "DeleteSelectedTodoBtn";
            DeleteSelectedTodoBtn.Size = new Size(184, 36);
            DeleteSelectedTodoBtn.TabIndex = 2;
            DeleteSelectedTodoBtn.Text = "Delete Selected Todo";
            DeleteSelectedTodoBtn.UseVisualStyleBackColor = false;
            // 
            // TodoDataGridView
            // 
            TodoDataGridView.AllowUserToAddRows = false;
            TodoDataGridView.AllowUserToDeleteRows = false;
            TodoDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            TodoDataGridView.Location = new Point(5, 46);
            TodoDataGridView.Margin = new Padding(0);
            TodoDataGridView.Name = "TodoDataGridView";
            TodoDataGridView.ReadOnly = true;
            TodoDataGridView.Size = new Size(961, 443);
            TodoDataGridView.TabIndex = 3;
            // 
            // HideIdAndRevCheckbox
            // 
            HideIdAndRevCheckbox.CheckAlign = ContentAlignment.MiddleRight;
            HideIdAndRevCheckbox.ForeColor = Color.White;
            HideIdAndRevCheckbox.Location = new Point(572, 5);
            HideIdAndRevCheckbox.Margin = new Padding(0);
            HideIdAndRevCheckbox.Name = "HideIdAndRevCheckbox";
            HideIdAndRevCheckbox.Size = new Size(268, 36);
            HideIdAndRevCheckbox.TabIndex = 4;
            HideIdAndRevCheckbox.Text = "Hide Id and Rev From DataGridView";
            HideIdAndRevCheckbox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(33, 33, 33);
            ClientSize = new Size(971, 494);
            Controls.Add(HideIdAndRevCheckbox);
            Controls.Add(TodoDataGridView);
            Controls.Add(DeleteSelectedTodoBtn);
            Controls.Add(EditSelectedTodoBtn);
            Controls.Add(CreateNewTodoBtn);
            Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            Padding = new Padding(5);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CouchDB Test";
            ((System.ComponentModel.ISupportInitialize)TodoDataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button CreateNewTodoBtn;
        private Button EditSelectedTodoBtn;
        private Button DeleteSelectedTodoBtn;
        private DataGridView TodoDataGridView;
        private CheckBox HideIdAndRevCheckbox;
    }
}