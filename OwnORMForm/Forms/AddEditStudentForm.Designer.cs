namespace OwnORMForm.Forms
{
    partial class AddEditStudentForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddEditStudentForm));
            Title = new Label();
            NameLabel = new Label();
            NameTextbox = new TextBox();
            AddressTextbox = new TextBox();
            AddressLabel = new Label();
            ClassLabel = new Label();
            ClassComboBox = new ComboBox();
            StudentTypeComboBox = new ComboBox();
            StudentTypeLabel = new Label();
            button1 = new Button();
            SuspendLayout();
            Title.Font = new Font("Verdana", 15.75F, FontStyle.Bold);
            Title.Location = new Point(5, 5);
            Title.Margin = new Padding(0, 0, 0, 5);
            Title.Name = "Title";
            Title.Size = new Size(370, 48);
            Title.TabIndex = 0;
            Title.Text = "Opret Elev";
            Title.TextAlign = ContentAlignment.MiddleCenter;
            NameLabel.Font = new Font("Verdana", 9.75F, FontStyle.Bold);
            NameLabel.ForeColor = Color.Orange;
            NameLabel.Location = new Point(5, 58);
            NameLabel.Margin = new Padding(0, 0, 0, 5);
            NameLabel.Name = "NameLabel";
            NameLabel.Size = new Size(370, 24);
            NameLabel.TabIndex = 1;
            NameLabel.Text = "Navn:";
            NameLabel.TextAlign = ContentAlignment.MiddleLeft;
            NameTextbox.BackColor = Color.WhiteSmoke;
            NameTextbox.BorderStyle = BorderStyle.FixedSingle;
            NameTextbox.Location = new Point(5, 87);
            NameTextbox.Margin = new Padding(0, 0, 0, 5);
            NameTextbox.Name = "NameTextbox";
            NameTextbox.Size = new Size(370, 23);
            NameTextbox.TabIndex = 2;
            AddressTextbox.BackColor = Color.WhiteSmoke;
            AddressTextbox.BorderStyle = BorderStyle.FixedSingle;
            AddressTextbox.Location = new Point(5, 144);
            AddressTextbox.Margin = new Padding(0, 0, 0, 5);
            AddressTextbox.Name = "AddressTextbox";
            AddressTextbox.Size = new Size(370, 23);
            AddressTextbox.TabIndex = 4;
            AddressLabel.Font = new Font("Verdana", 9.75F, FontStyle.Bold);
            AddressLabel.ForeColor = Color.Orange;
            AddressLabel.Location = new Point(5, 115);
            AddressLabel.Margin = new Padding(0, 0, 0, 5);
            AddressLabel.Name = "AddressLabel";
            AddressLabel.Size = new Size(370, 24);
            AddressLabel.TabIndex = 3;
            AddressLabel.Text = "Adresse:";
            AddressLabel.TextAlign = ContentAlignment.MiddleLeft;
            ClassLabel.Font = new Font("Verdana", 9.75F, FontStyle.Bold);
            ClassLabel.ForeColor = Color.Orange;
            ClassLabel.Location = new Point(5, 172);
            ClassLabel.Margin = new Padding(0, 0, 0, 5);
            ClassLabel.Name = "ClassLabel";
            ClassLabel.Size = new Size(370, 24);
            ClassLabel.TabIndex = 5;
            ClassLabel.Text = "Klasse:";
            ClassLabel.TextAlign = ContentAlignment.MiddleLeft;
            ClassComboBox.BackColor = Color.WhiteSmoke;
            ClassComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            ClassComboBox.FormattingEnabled = true;
            ClassComboBox.Location = new Point(5, 201);
            ClassComboBox.Margin = new Padding(0, 0, 0, 5);
            ClassComboBox.Name = "ClassComboBox";
            ClassComboBox.Size = new Size(370, 24);
            ClassComboBox.TabIndex = 6;
            StudentTypeComboBox.BackColor = Color.WhiteSmoke;
            StudentTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            StudentTypeComboBox.FormattingEnabled = true;
            StudentTypeComboBox.Location = new Point(5, 259);
            StudentTypeComboBox.Margin = new Padding(0, 0, 0, 15);
            StudentTypeComboBox.Name = "StudentTypeComboBox";
            StudentTypeComboBox.Size = new Size(370, 24);
            StudentTypeComboBox.TabIndex = 8;
            StudentTypeLabel.Font = new Font("Verdana", 9.75F, FontStyle.Bold);
            StudentTypeLabel.ForeColor = Color.Orange;
            StudentTypeLabel.Location = new Point(5, 230);
            StudentTypeLabel.Margin = new Padding(0, 0, 0, 5);
            StudentTypeLabel.Name = "StudentTypeLabel";
            StudentTypeLabel.Size = new Size(370, 24);
            StudentTypeLabel.TabIndex = 7;
            StudentTypeLabel.Text = "Elev Type:";
            StudentTypeLabel.TextAlign = ContentAlignment.MiddleLeft;
            button1.BackColor = Color.Green;
            button1.Cursor = Cursors.Hand;
            button1.Enabled = false;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Verdana", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button1.Location = new Point(5, 298);
            button1.Margin = new Padding(0);
            button1.Name = "button1";
            button1.Size = new Size(370, 38);
            button1.TabIndex = 9;
            button1.Text = "Opret Elev";
            button1.UseVisualStyleBackColor = false;
            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(33, 33, 33);
            ClientSize = new Size(380, 342);
            Controls.Add(button1);
            Controls.Add(StudentTypeComboBox);
            Controls.Add(StudentTypeLabel);
            Controls.Add(ClassComboBox);
            Controls.Add(ClassLabel);
            Controls.Add(AddressTextbox);
            Controls.Add(AddressLabel);
            Controls.Add(NameTextbox);
            Controls.Add(NameLabel);
            Controls.Add(Title);
            Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ForeColor = Color.Gainsboro;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddStudentForm";
            Padding = new Padding(5);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Opret Elev";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Title;
        private Label NameLabel;
        private TextBox NameTextbox;
        private TextBox AddressTextbox;
        private Label AddressLabel;
        private Label ClassLabel;
        private ComboBox ClassComboBox;
        private ComboBox StudentTypeComboBox;
        private Label StudentTypeLabel;
        private Button button1;
    }
}