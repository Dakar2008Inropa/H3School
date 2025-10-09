using OwnORM.Models;
using OwnORM.Repositories;

namespace OwnORMForm
{
    public partial class MainForm : Form
    {
        private readonly SchoolRepository _repository;
        private readonly BindingSource _studentsSource = new BindingSource();
        private readonly BindingSource _classesSource = new BindingSource();
        private readonly BindingSource _coursesSource = new BindingSource();
        private readonly BindingSource _gradesSource = new BindingSource();

        private List<Student> _allStudents = new List<Student>();
        private List<Class> _allClasses = new List<Class>();
        private List<Course> _allCourses = new List<Course>();
        private List<StudentClassRepetitionOnClass> _currentGrades = new List<StudentClassRepetitionOnClass>();

        public MainForm()
        {
            InitializeComponent();

            _repository = new SchoolRepository();

            StudentDataGridView.DataSource = _studentsSource;
            ClassesDataGridView.DataSource = _classesSource;
            CoursesDataGridView.DataSource = _coursesSource;
            GradesDataGridView.DataSource = _gradesSource;

            ConfigureGrid(StudentDataGridView);
            ConfigureGrid(ClassesDataGridView);
            ConfigureGrid(CoursesDataGridView);
            ConfigureGrid(GradesDataGridView);

            SetupStudentsGridColumns();
            SetupClassesGridColumns();
            SetupCoursesGridColumns();
            SetupGradesGridColumns();

            WireEvents();
        }

        protected override void OnClosed(EventArgs e)
        {
            _repository.Dispose();
            base.OnClosed(e);
        }

        private void WireEvents()
        {
            SearchStudentTextbox.TextChanged += (s, e) => ApplyStudentFilter();
            SearchClassesTextBox.TextChanged += (s, e) => ApplyClassFilter();
            SearchCourseTextbox.TextChanged += (s, e) => ApplyCourseFilter();
            SearchGradesTextbox.TextChanged += (s, e) => ApplyGradesFilter();

            StudentDataGridView.SelectionChanged += (s, e) => UpdateStudentButtons();
            ClassesDataGridView.SelectionChanged += (s, e) => UpdateClassButtons();
            CoursesDataGridView.SelectionChanged += (s, e) => UpdateCourseButtons();
            GradesDataGridView.SelectionChanged += (s, e) => UpdateGradeButtons();

            CreateStudentBtn.Click += async (s, e) => await OnCreateStudentAsync();
            EditStudentBtn.Click += async (s, e) => await OnEditStudentAsync();
            DeleteStudentBtn.Click += async (s, e) => await OnDeleteStudentAsync();
            ShowStudentBtn.Click += (s, e) => OnShowStudent();

            CreateClassBtn.Click += async (s, e) => await OnCreateClassAsync();
            EditClassesBtn.Click += async (s, e) => await OnEditClassAsync();
            DeleteClassesBtn.Click += async (s, e) => await OnDeleteClassAsync();
            ShowClassBtn.Click += (s, e) => MessageBox.Show("Ikke implementeret endnu.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            CreateCourseBtn.Click += async (s, e) => await OnCreateCourseAsync();
            EditCourseBtn.Click += async (s, e) => await OnEditCourseAsync();
            DeleteCourseBtn.Click += async (s, e) => await OnDeleteCourseAsync();
            ShowCourseBtn.Click += (s, e) => MessageBox.Show("Ikke implementeret endnu.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            CreateGradeBtn.Click += async (s, e) => await OnCreateGradeAsync();
            EditGradeBtn.Click += async (s, e) => await OnEditGradeAsync();
            DeleteGradeBtn.Click += async (s, e) => await OnDeleteGradeAsync();
            ShowGradeBtn.Click += (s, e) => MessageBox.Show("Brug fanen Elever for at åbne elevvisning.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            StudentDataGridView.CellFormatting += StudentDataGridView_CellFormatting;
            GradesDataGridView.CellFormatting += GradesDataGridView_CellFormatting;
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

        private void SetupStudentsGridColumns()
        {
            StudentDataGridView.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Student.StudentID),
                HeaderText = "ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 10
            };
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Student.StudentName),
                HeaderText = "Navn",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 26
            };
            DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Student.StudentAddress),
                HeaderText = "Adresse",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 26
            };
            DataGridViewTextBoxColumn colClassName = new DataGridViewTextBoxColumn
            {
                Name = "ClassNameColumn",
                HeaderText = "Klasse",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 18
            };
            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Student.StudentType),
                HeaderText = "Elevtype",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 10
            };
            DataGridViewTextBoxColumn colCount = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Student.StudentNumberOfCourses),
                HeaderText = "Antal fag",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 10
            };
            DataGridViewTextBoxColumn colAvg = new DataGridViewTextBoxColumn
            {
                Name = "AverageColumn",
                HeaderText = "Gennemsnit",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 10
            };

            StudentDataGridView.Columns.AddRange(colId, colName, colAddress, colClassName, colType, colCount, colAvg);
        }

        private void SetupClassesGridColumns()
        {
            ClassesDataGridView.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Class.ClassID),
                HeaderText = "ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 10
            };
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Class.ClassName),
                HeaderText = "Navn",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 30
            };
            DataGridViewTextBoxColumn colDesc = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Class.ClassDescription),
                HeaderText = "Beskrivelse",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 60
            };

            ClassesDataGridView.Columns.AddRange(colId, colName, colDesc);
        }

        private void SetupCoursesGridColumns()
        {
            CoursesDataGridView.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Course.CourseID),
                HeaderText = "ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            };
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Course.CourseName),
                HeaderText = "Navn",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 85
            };

            CoursesDataGridView.Columns.AddRange(colId, colName);
        }

        private void SetupGradesGridColumns()
        {
            GradesDataGridView.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.StudentClassID),
                HeaderText = "ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 10
            };
            DataGridViewTextBoxColumn colStudent = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.StudentID),
                HeaderText = "Elev-ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            };
            DataGridViewTextBoxColumn colClassName = new DataGridViewTextBoxColumn
            {
                Name = "ClassNameColumn",
                HeaderText = "Klasse",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25
            };
            DataGridViewTextBoxColumn colCourse = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.CourseID),
                HeaderText = "Fag-ID",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            };
            DataGridViewTextBoxColumn colGrade = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(StudentClassRepetitionOnClass.Grade),
                HeaderText = "Karakter",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 35
            };

            GradesDataGridView.Columns.AddRange(colId, colStudent, colClassName, colCourse, colGrade);
        }

        private void StudentDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string colName = StudentDataGridView.Columns[e.ColumnIndex].Name;

            if (colName == "AverageColumn")
            {
                Student s = StudentDataGridView.Rows[e.RowIndex].DataBoundItem as Student;
                if (s == null)
                {
                    e.Value = string.Empty;
                    e.FormattingApplied = true;
                    return;
                }

                if (s.StudentNumberOfCourses <= 0)
                {
                    e.Value = "0,0";
                    e.FormattingApplied = true;
                    return;
                }

                double average = (double)s.StudentSumOfAllCharacters / s.StudentNumberOfCourses;
                e.Value = average.ToString("0.0");
                e.FormattingApplied = true;
                return;
            }

            if (colName == "ClassNameColumn")
            {
                Student s = StudentDataGridView.Rows[e.RowIndex].DataBoundItem as Student;
                if (s == null)
                {
                    e.Value = string.Empty;
                    e.FormattingApplied = true;
                    return;
                }

                Class cls = _allClasses.FirstOrDefault(c => c.ClassID == s.ClassID);
                e.Value = cls != null ? cls.ClassName : $"Klasse-ID: {s.ClassID}";
                e.FormattingApplied = true;
            }
        }

        private void GradesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string colName = GradesDataGridView.Columns[e.ColumnIndex].Name;
            if (colName != "ClassNameColumn")
            {
                return;
            }

            StudentClassRepetitionOnClass row = GradesDataGridView.Rows[e.RowIndex].DataBoundItem as StudentClassRepetitionOnClass;
            if (row == null)
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
                return;
            }

            Class cls = _allClasses.FirstOrDefault(c => c.ClassID == row.ClassID);
            e.Value = cls != null ? cls.ClassName : $"Klasse-ID: {row.ClassID}";
            e.FormattingApplied = true;
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                await LoadStudentsAsync();
                await LoadClassesAsync();
                await LoadCoursesAsync();

                if (StudentDataGridView.Rows.Count > 0)
                {
                    StudentDataGridView.Rows[0].Selected = true;
                }
                UpdateStudentButtons();
                UpdateClassButtons();
                UpdateCourseButtons();
                UpdateGradeButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke indlæse data: {ex.Message}", "Fejl", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainTabControl.SelectedTab != GradesPage)
            {
                return;
            }

            int studentId = GetSelectedStudentId();
            if (studentId == 0)
            {
                _currentGrades = new List<StudentClassRepetitionOnClass>();
                _gradesSource.DataSource = _currentGrades;
                return;
            }

            await LoadGradesForStudentAsync(studentId);
        }

        private async Task LoadStudentsAsync()
        {
            IReadOnlyList<Student> list = await _repository.GetStudentsAsync(default);
            _allStudents = list.OrderBy(s => s.StudentID).ToList();
            _studentsSource.DataSource = _allStudents;
            ApplyStudentFilter();
        }

        private async Task LoadClassesAsync()
        {
            IReadOnlyList<Class> list = await _repository.GetClassesAsync(default);
            _allClasses = list.OrderBy(c => c.ClassID).ToList();
            _classesSource.DataSource = _allClasses;
            ApplyClassFilter();
        }

        private async Task LoadCoursesAsync()
        {
            IReadOnlyList<Course> list = await _repository.GetCoursesAsync(default);
            _allCourses = list.OrderBy(c => c.CourseID).ToList();
            _coursesSource.DataSource = _allCourses;
            ApplyCourseFilter();
        }

        private async Task LoadGradesForStudentAsync(int studentId)
        {
            IReadOnlyList<StudentClassRepetitionOnClass> list = await _repository.GetGradesByStudentAsync(studentId, default);
            _currentGrades = list.OrderBy(g => g.StudentClassID).ToList();
            _gradesSource.DataSource = _currentGrades;
            ApplyGradesFilter();
        }

        private void ApplyStudentFilter()
        {
            string term = SearchStudentTextbox.Text == null ? string.Empty : SearchStudentTextbox.Text.Trim();
            if (term.Length == 0)
            {
                _studentsSource.DataSource = _allStudents;
                return;
            }

            string lower = term.ToLowerInvariant();
            List<Student> filtered = _allStudents
                .Where(s =>
                    (s.StudentName != null && s.StudentName.ToLowerInvariant().Contains(lower)) ||
                    (s.StudentAddress != null && s.StudentAddress.ToLowerInvariant().Contains(lower)) ||
                    s.StudentID.ToString().Contains(term))
                .ToList();

            _studentsSource.DataSource = filtered;
        }

        private void ApplyClassFilter()
        {
            string term = SearchClassesTextBox.Text == null ? string.Empty : SearchClassesTextBox.Text.Trim();
            if (term.Length == 0)
            {
                _classesSource.DataSource = _allClasses;
                return;
            }

            string lower = term.ToLowerInvariant();
            List<Class> filtered = _allClasses
                .Where(c =>
                    (c.ClassName != null && c.ClassName.ToLowerInvariant().Contains(lower)) ||
                    (c.ClassDescription != null && c.ClassDescription.ToLowerInvariant().Contains(lower)) ||
                    c.ClassID.ToString().Contains(term))
                .ToList();

            _classesSource.DataSource = filtered;
        }

        private void ApplyCourseFilter()
        {
            string term = SearchCourseTextbox.Text == null ? string.Empty : SearchCourseTextbox.Text.Trim();
            if (term.Length == 0)
            {
                _coursesSource.DataSource = _allCourses;
                return;
            }

            string lower = term.ToLowerInvariant();
            List<Course> filtered = _allCourses
                .Where(c =>
                    (c.CourseName != null && c.CourseName.ToLowerInvariant().Contains(lower)) ||
                    c.CourseID.ToString().Contains(term))
                .ToList();

            _coursesSource.DataSource = filtered;
        }

        private void ApplyGradesFilter()
        {
            string term = SearchGradesTextbox.Text == null ? string.Empty : SearchGradesTextbox.Text.Trim();
            if (term.Length == 0)
            {
                _gradesSource.DataSource = _currentGrades;
                return;
            }

            List<StudentClassRepetitionOnClass> filtered = _currentGrades
                .Where(g =>
                    g.StudentClassID.ToString().Contains(term) ||
                    g.StudentID.ToString().Contains(term) ||
                    g.ClassID.ToString().Contains(term) ||
                    g.CourseID.ToString().Contains(term) ||
                    g.Grade.ToString().Contains(term))
                .ToList();

            _gradesSource.DataSource = filtered;
        }

        private void UpdateStudentButtons()
        {
            bool hasSelection = GetSelectedStudentId() != 0;
            EditStudentBtn.Enabled = hasSelection;
            DeleteStudentBtn.Enabled = hasSelection;
            ShowStudentBtn.Enabled = hasSelection;
        }

        private void UpdateClassButtons()
        {
            bool hasSelection = GetSelectedClassId() != 0;
            EditClassesBtn.Enabled = hasSelection;
            DeleteClassesBtn.Enabled = hasSelection;
            ShowClassBtn.Enabled = hasSelection;
        }

        private void UpdateCourseButtons()
        {
            bool hasSelection = GetSelectedCourseId() != 0;
            EditCourseBtn.Enabled = hasSelection;
            DeleteCourseBtn.Enabled = hasSelection;
            ShowCourseBtn.Enabled = hasSelection;
        }

        private void UpdateGradeButtons()
        {
            bool hasSelection = GetSelectedGradeId() != 0;
            EditGradeBtn.Enabled = hasSelection;
            DeleteGradeBtn.Enabled = hasSelection;
            ShowGradeBtn.Enabled = hasSelection;
        }

        private int GetSelectedStudentId()
        {
            if (StudentDataGridView.CurrentRow == null)
            {
                return 0;
            }

            object data = StudentDataGridView.CurrentRow.DataBoundItem;
            Student student = data as Student;
            if (student == null)
            {
                return 0;
            }

            return student.StudentID;
        }

        private int GetSelectedClassId()
        {
            if (ClassesDataGridView.CurrentRow == null)
            {
                return 0;
            }

            object data = ClassesDataGridView.CurrentRow.DataBoundItem;
            Class item = data as Class;
            if (item == null)
            {
                return 0;
            }

            return item.ClassID;
        }

        private int GetSelectedCourseId()
        {
            if (CoursesDataGridView.CurrentRow == null)
            {
                return 0;
            }

            object data = CoursesDataGridView.CurrentRow.DataBoundItem;
            Course item = data as Course;
            if (item == null)
            {
                return 0;
            }

            return item.CourseID;
        }

        private int GetSelectedGradeId()
        {
            if (GradesDataGridView.CurrentRow == null)
            {
                return 0;
            }

            object data = GradesDataGridView.CurrentRow.DataBoundItem;
            StudentClassRepetitionOnClass item = data as StudentClassRepetitionOnClass;
            if (item == null)
            {
                return 0;
            }

            return item.StudentClassID;
        }

        private async Task OnCreateStudentAsync()
        {
            using (OwnORMForm.Forms.AddEditStudentForm dlg = new OwnORMForm.Forms.AddEditStudentForm(_repository))
            {
                DialogResult res = dlg.ShowDialog(this);
                if (res != DialogResult.OK)
                {
                    return;
                }

                await LoadStudentsAsync();
            }
        }

        private async Task OnEditStudentAsync()
        {
            int id = GetSelectedStudentId();
            if (id == 0)
            {
                MessageBox.Show("Vælg en elev.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Student existing = await _repository.GetStudentByIdAsync(id, default);
            if (existing == null)
            {
                MessageBox.Show("Elev ikke fundet.", "Advarsel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OwnORMForm.Forms.AddEditStudentForm dlg = new OwnORMForm.Forms.AddEditStudentForm(_repository, existing))
            {
                DialogResult res = dlg.ShowDialog(this);
                if (res != DialogResult.OK)
                {
                    return;
                }

                int selectedId = id;
                await LoadStudentsAsync();

                Student toSelect = _allStudents.FirstOrDefault(s => s.StudentID == selectedId);
                if (toSelect != null)
                {
                    _studentsSource.Position = _allStudents.IndexOf(toSelect);
                }
            }
        }

        private async Task OnDeleteStudentAsync()
        {
            int id = GetSelectedStudentId();
            if (id == 0)
            {
                MessageBox.Show("Vælg en elev.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show("Slet eleven og alle relaterede data?", "Bekræft", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            int rows = await _repository.DeleteStudentCascadeAsync(id, default);
            if (rows <= 0)
            {
                MessageBox.Show("Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await LoadStudentsAsync();
        }

        private void OnShowStudent()
        {
            int id = GetSelectedStudentId();
            if (id == 0)
            {
                MessageBox.Show("Vælg en elev.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OwnORMForm.Forms.ViewStudentForm dlg = new OwnORMForm.Forms.ViewStudentForm(_repository, id))
            {
                dlg.ShowDialog(this);
            }
        }

        private async Task OnCreateClassAsync()
        {
            string inputName = PromptForText("Indtast klassenavn:");
            if (string.IsNullOrWhiteSpace(inputName))
            {
                MessageBox.Show("Klassenavn er påkrævet.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string inputDescription = PromptForText("Indtast beskrivelse (valgfrit):");

            int rows = await _repository.AddClassAsync(inputName, inputDescription ?? string.Empty, default);
            MessageBox.Show(rows > 0 ? "Klasse oprettet." : "Ingen klasse oprettet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadClassesAsync();
        }

        private async Task OnEditClassAsync()
        {
            int id = GetSelectedClassId();
            if (id == 0)
            {
                MessageBox.Show("Vælg en klasse.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = PromptForText("Nyt klassenavn (tom = behold):");
            string description = PromptForText("Ny beskrivelse (tom = behold):");

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Ingen ændringer angivet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string effectiveName = string.IsNullOrWhiteSpace(name) ? "(no change)" : name;
            string effectiveDesc = string.IsNullOrWhiteSpace(description) ? "(no change)" : description;

            int rows = await _repository.UpdateClassAsync(id, effectiveName, effectiveDesc, default);
            MessageBox.Show(rows > 0 ? "Klasse opdateret." : "Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadClassesAsync();
        }

        private async Task OnDeleteClassAsync()
        {
            int id = GetSelectedClassId();
            if (id == 0)
            {
                MessageBox.Show("Vælg en klasse.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show("ADVARSEL: Dette sletter alle elever i klassen og deres data. Fortsæt?", "Bekræft", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            int rows = await _repository.DeleteClassCascadeAsync(id, default);
            MessageBox.Show(rows > 0 ? "Klasse slettet." : "Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadClassesAsync();
        }

        private async Task OnCreateCourseAsync()
        {
            string name = PromptForText("Indtast fagnavn:");
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Fagnavn er påkrævet.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rows = await _repository.AddCourseAsync(name, default);
            MessageBox.Show(rows > 0 ? "Fag oprettet." : "Ingen fag oprettet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadCoursesAsync();
        }

        private async Task OnEditCourseAsync()
        {
            int id = GetSelectedCourseId();
            if (id == 0)
            {
                MessageBox.Show("Vælg et fag.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = PromptForText("Nyt fagnavn:");
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Ingen ændringer angivet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int rows = await _repository.UpdateCourseAsync(id, name, default);
            MessageBox.Show(rows > 0 ? "Fag opdateret." : "Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadCoursesAsync();
        }

        private async Task OnDeleteCourseAsync()
        {
            int id = GetSelectedCourseId();
            if (id == 0)
            {
                MessageBox.Show("Vælg et fag.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show("ADVARSEL: Dette sletter alle karakterer relateret til faget. Fortsæt?", "Bekræft", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            int rows = await _repository.DeleteCourseCascadeAsync(id, default);
            MessageBox.Show(rows > 0 ? "Fag slettet." : "Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadCoursesAsync();
        }

        private async Task OnCreateGradeAsync()
        {
            int studentId = GetSelectedStudentId();
            if (studentId == 0)
            {
                MessageBox.Show("Vælg først en elev på fanen Elever.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int classId = PromptForInt("Indtast klasse-ID:");
            if (classId <= 0)
            {
                MessageBox.Show("Klasse-ID skal være > 0.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseId = PromptForInt("Indtast fag-ID:");
            if (courseId <= 0)
            {
                MessageBox.Show("Fag-ID skal være > 0.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int grade = PromptForInt("Indtast karakter:");
            if (grade <= 0)
            {
                MessageBox.Show("Karakter skal være > 0.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rows = await _repository.AddGradeAsync(studentId, classId, courseId, grade, default);
            MessageBox.Show(rows > 0 ? "Karakter tilføjet." : "Ingen karakter tilføjet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (MainTabControl.SelectedTab == GradesPage)
            {
                await LoadGradesForStudentAsync(studentId);
            }
        }

        private async Task OnEditGradeAsync()
        {
            int gradeId = GetSelectedGradeId();
            if (gradeId == 0)
            {
                MessageBox.Show("Vælg en karakter (på fanen Karaktere).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int newGrade = PromptForInt("Indtast ny karakter:");
            if (newGrade <= 0)
            {
                MessageBox.Show("Karakter skal være > 0.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rows = await _repository.UpdateGradeAsync(gradeId, newGrade, default);
            MessageBox.Show(rows > 0 ? "Karakter opdateret." : "Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (MainTabControl.SelectedTab == GradesPage)
            {
                int studentId = GetSelectedStudentId();
                if (studentId != 0)
                {
                    await LoadGradesForStudentAsync(studentId);
                }
            }
        }

        private async Task OnDeleteGradeAsync()
        {
            int gradeId = GetSelectedGradeId();
            if (gradeId == 0)
            {
                MessageBox.Show("Vælg en karakter (på fanen Karaktere).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show("Slet denne karakter?", "Bekræft", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            int rows = await _repository.DeleteGradeAsync(gradeId, default);
            MessageBox.Show(rows > 0 ? "Karakter slettet." : "Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (MainTabControl.SelectedTab == GradesPage)
            {
                int studentId = GetSelectedStudentId();
                if (studentId != 0)
                {
                    await LoadGradesForStudentAsync(studentId);
                }
            }
        }

        private string PromptForText(string title)
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
                    return null;
                }

                return tb.Text;
            }
        }

        private int PromptForInt(string title)
        {
            string input = PromptForText(title);
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }

            bool ok = int.TryParse(input, out int value);
            if (!ok)
            {
                return 0;
            }

            if (value < 0)
            {
                return 0;
            }

            return value;
        }
    }
}