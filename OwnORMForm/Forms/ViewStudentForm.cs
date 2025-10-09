using OwnORM.Models;
using OwnORM.Repositories;

namespace OwnORMForm.Forms
{
    public partial class ViewStudentForm : Form
    {
        private readonly SchoolRepository _repository;
        private readonly int _studentId;
        private List<StudentClassRepetitionOnClass> _grades = new List<StudentClassRepetitionOnClass>();
        private List<Class> _classes = new List<Class>();
        private List<Course> _courses = new List<Course>();

        public ViewStudentForm(SchoolRepository repository, int studentId)
        {
            _repository = repository;
            _studentId = studentId;

            InitializeComponent();
            WireEvents();

            Text = "Vis elev";
            Title.Text = "Vis elev";

            ConfigureGrid(StudentGridView);
            SetupGradesGridColumns();

            Shown += async (s, e) => await InitializeAsync();
        }

        private void WireEvents()
        {
            AddGradeBtn.Click += async (s, e) => await OnAddGradeAsync();
            AddCourseBtn.Click += async (s, e) => await OnAddCourseAsync();
            StudentGridView.CellFormatting += StudentGridView_CellFormatting;
        }

        private async Task InitializeAsync()
        {
            try
            {
                IReadOnlyList<Class> classes = await _repository.GetClassesAsync(default);
                IReadOnlyList<Course> courses = await _repository.GetCoursesAsync(default);
                _classes = classes.OrderBy(c => c.ClassName).ToList();
                _courses = courses.OrderBy(c => c.CourseName).ToList();

                Student student = await _repository.GetStudentByIdAsync(_studentId, default);
                if (student == null)
                {
                    MessageBox.Show("Elev ikke fundet.", "Advarsel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                NameValue.Text = student.StudentName ?? string.Empty;
                AddressValue.Text = student.StudentAddress ?? string.Empty;

                Class cls = _classes.FirstOrDefault(c => c.ClassID == student.ClassID);
                ClassValue.Text = cls != null ? cls.ClassName : $"Klasse-ID: {student.ClassID}";
                StudentTypeValue.Text = student.StudentType.ToString();

                await LoadGradesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke indlæse elev: {ex.Message}", "Fejl", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private async Task LoadGradesAsync()
        {
            IReadOnlyList<StudentClassRepetitionOnClass> grades = await _repository.GetGradesByStudentAsync(_studentId, default);
            _grades = grades.OrderBy(g => g.StudentClassID).ToList();
            StudentGridView.DataSource = _grades;
        }

        private static void ConfigureGrid(DataGridView grid)
        {
            grid.AutoGenerateColumns = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;

            grid.BackgroundColor = Color.FromArgb(33, 33, 33);
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.DimGray;

            grid.DefaultCellStyle.BackColor = Color.FromArgb(33, 33, 33);
            grid.DefaultCellStyle.ForeColor = Color.Gainsboro;
            grid.DefaultCellStyle.SelectionBackColor = Color.Firebrick;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.Gainsboro;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Verdana", 9.75F, FontStyle.Bold);

            grid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            grid.RowHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
        }

        private void SetupGradesGridColumns()
        {
            StudentGridView.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.StudentClassID),
                HeaderText = "ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 12
            };
            DataGridViewTextBoxColumn colStudent = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.StudentID),
                HeaderText = "Elev-ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 18
            };
            DataGridViewTextBoxColumn colClassName = new DataGridViewTextBoxColumn
            {
                Name = "ClassNameColumn",
                HeaderText = "Klasse",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 28
            };
            DataGridViewTextBoxColumn colCourse = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.CourseID),
                HeaderText = "Fag-ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 18
            };
            DataGridViewTextBoxColumn colGrade = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.Grade),
                HeaderText = "Karakter",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 24
            };

            StudentGridView.Columns.AddRange(colId, colStudent, colClassName, colCourse, colGrade);
        }

        private void StudentGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (StudentGridView.Columns[e.ColumnIndex].Name != "ClassNameColumn")
            {
                return;
            }

            StudentClassRepetitionOnClass row = StudentGridView.Rows[e.RowIndex].DataBoundItem as StudentClassRepetitionOnClass;
            if (row == null)
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
                return;
            }

            Class cls = _classes.FirstOrDefault(c => c.ClassID == row.ClassID);
            e.Value = cls != null ? cls.ClassName : $"Klasse-ID: {row.ClassID}";
            e.FormattingApplied = true;
        }

        private async Task OnAddGradeAsync()
        {
            int classId = PromptForClassId();
            if (classId <= 0)
            {
                MessageBox.Show("Klasse er påkrævet.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseId = PromptForCourseId();
            if (courseId <= 0)
            {
                MessageBox.Show("Fag er påkrævet.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int grade = PromptForInt("Indtast karakter:");
            if (grade <= 0)
            {
                MessageBox.Show("Karakter skal være > 0.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int rows = await _repository.AddGradeAsync(_studentId, classId, courseId, grade, default);
                MessageBox.Show(rows > 0 ? "Karakter tilføjet." : "Ingen karakter tilføjet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadGradesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke tilføje karakter: {ex.Message}", "Fejl", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnAddCourseAsync()
        {
            int classId = PromptForClassId();
            if (classId <= 0)
            {
                MessageBox.Show("Klasse er påkrævet.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseId = PromptForCourseId();
            if (courseId <= 0)
            {
                MessageBox.Show("Fag er påkrævet.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int rows = await _repository.AddGradeAsync(_studentId, classId, courseId, 0, default);
                MessageBox.Show(rows > 0 ? "Fag tilføjet med start-karakter 0." : "Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadGradesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke tilføje fag: {ex.Message}", "Fejl", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int PromptForInt(string title)
        {
            using (Form dlg = new Form())
            using (Label lbl = new Label())
            using (TextBox tb = new TextBox())
            using (Button ok = new Button())
            using (Button cancel = new Button())
            {
                dlg.Text = title;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.Width = 420;
                dlg.Height = 150;
                dlg.BackColor = Color.FromArgb(33, 33, 33);
                dlg.ForeColor = Color.Gainsboro;

                lbl.Text = title;
                lbl.Top = 10;
                lbl.Left = 10;
                lbl.Width = 380;

                tb.Top = 35;
                tb.Left = 10;
                tb.Width = 380;

                ok.Text = "OK";
                ok.Top = 70;
                ok.Left = 220;
                ok.DialogResult = DialogResult.OK;

                cancel.Text = "Annuller";
                cancel.Top = 70;
                cancel.Left = 300;
                cancel.DialogResult = DialogResult.Cancel;

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(tb);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);

                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                DialogResult res = dlg.ShowDialog(this);
                if (res != DialogResult.OK)
                {
                    return 0;
                }

                bool okParse = int.TryParse(tb.Text, out int value);
                if (!okParse || value < 0)
                {
                    return 0;
                }

                return value;
            }
        }

        private int PromptForClassId()
        {
            if (_classes.Count == 0)
            {
                return PromptForInt("Indtast klasse-ID:");
            }

            using (Form dlg = new Form())
            using (Label lbl = new Label())
            using (ListBox lb = new ListBox())
            using (Button ok = new Button())
            using (Button cancel = new Button())
            {
                dlg.Text = "Vælg klasse";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.Width = 420;
                dlg.Height = 300;
                dlg.BackColor = Color.FromArgb(33, 33, 33);
                dlg.ForeColor = Color.Gainsboro;

                lbl.Text = "Vælg en klasse:";
                lbl.Top = 10;
                lbl.Left = 10;
                lbl.Width = 380;

                lb.Top = 35;
                lb.Left = 10;
                lb.Width = 380;
                lb.Height = 180;
                lb.DisplayMember = nameof(Class.ClassName);
                lb.ValueMember = nameof(Class.ClassID);
                foreach (Class c in _classes)
                {
                    lb.Items.Add(c);
                }

                ok.Text = "OK";
                ok.Top = 225;
                ok.Left = 220;
                ok.DialogResult = DialogResult.OK;

                cancel.Text = "Annuller";
                cancel.Top = 225;
                cancel.Left = 300;
                cancel.DialogResult = DialogResult.Cancel;

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(lb);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);

                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                DialogResult res = dlg.ShowDialog(this);
                if (res != DialogResult.OK)
                {
                    return 0;
                }

                Class selected = lb.SelectedItem as Class;
                if (selected == null)
                {
                    return 0;
                }

                return selected.ClassID;
            }
        }

        private int PromptForCourseId()
        {
            if (_courses.Count == 0)
            {
                return PromptForInt("Indtast fag-ID:");
            }

            using (Form dlg = new Form())
            using (Label lbl = new Label())
            using (ListBox lb = new ListBox())
            using (Button ok = new Button())
            using (Button cancel = new Button())
            {
                dlg.Text = "Vælg fag";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.Width = 420;
                dlg.Height = 300;
                dlg.BackColor = Color.FromArgb(33, 33, 33);
                dlg.ForeColor = Color.Gainsboro;

                lbl.Text = "Vælg et fag:";
                lbl.Top = 10;
                lbl.Left = 10;
                lbl.Width = 380;

                lb.Top = 35;
                lb.Left = 10;
                lb.Width = 380;
                lb.Height = 180;
                lb.DisplayMember = nameof(Course.CourseName);
                lb.ValueMember = nameof(Course.CourseID);
                foreach (Course c in _courses)
                {
                    lb.Items.Add(c);
                }

                ok.Text = "OK";
                ok.Top = 225;
                ok.Left = 220;
                ok.DialogResult = DialogResult.OK;

                cancel.Text = "Annuller";
                cancel.Top = 225;
                cancel.Left = 300;
                cancel.DialogResult = DialogResult.Cancel;

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(lb);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);

                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                DialogResult res = dlg.ShowDialog(this);
                if (res != DialogResult.OK)
                {
                    return 0;
                }

                Course selected = lb.SelectedItem as Course;
                if (selected == null)
                {
                    return 0;
                }

                return selected.CourseID;
            }
        }
    }
}