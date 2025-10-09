using OwnORM.Models;
using OwnORM.Repositories;

namespace OwnORMForm.Forms
{
    public partial class AddEditStudentForm : Form
    {
        private readonly SchoolRepository _repository;
        private readonly bool _isEdit;
        private readonly Student _existing;
        private List<Class> _classes = new List<Class>();

        public AddEditStudentForm(SchoolRepository repository)
        {
            _repository = repository;
            _isEdit = false;
            _existing = null;

            InitializeComponent();
            WireEvents();

            Text = "Opret elev";
            Title.Text = "Opret elev";
            button1.Text = "Opret elev";

            Shown += async (s, e) => await InitializeAsync();
        }

        public AddEditStudentForm(SchoolRepository repository, Student existing)
        {
            _repository = repository;
            _isEdit = true;
            _existing = existing;

            InitializeComponent();
            WireEvents();

            Text = "Rediger elev";
            Title.Text = "Rediger elev";
            button1.Text = "Gem ændringer";

            Shown += async (s, e) => await InitializeAsync();
        }

        private void WireEvents()
        {
            NameTextbox.TextChanged += (s, e) => UpdateSaveEnabled();
            ClassComboBox.SelectedIndexChanged += (s, e) => UpdateSaveEnabled();
            StudentTypeComboBox.SelectedIndexChanged += (s, e) => UpdateSaveEnabled();
            button1.Click += async (s, e) => await OnSaveAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await LoadClassesAsync();

                StudentTypeComboBox.DataSource = Enum.GetValues(typeof(StudentType));

                if (_isEdit && _existing != null)
                {
                    NameTextbox.Text = _existing.StudentName;
                    AddressTextbox.Text = _existing.StudentAddress;

                    Class cls = _classes.FirstOrDefault(c => c.ClassID == _existing.ClassID);
                    if (cls != null)
                    {
                        ClassComboBox.SelectedItem = cls;
                    }

                    StudentTypeComboBox.SelectedItem = _existing.StudentType;
                }

                UpdateSaveEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke initialisere formularen: {ex.Message}", "Fejl", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private async Task LoadClassesAsync()
        {
            IReadOnlyList<Class> list = await _repository.GetClassesAsync(default);
            _classes = list.OrderBy(c => c.ClassName).ToList();

            ClassComboBox.DisplayMember = nameof(Class.ClassName);
            ClassComboBox.ValueMember = nameof(Class.ClassID);
            ClassComboBox.DataSource = _classes;
        }

        private void UpdateSaveEnabled()
        {
            bool hasName = !string.IsNullOrWhiteSpace(NameTextbox.Text);
            bool hasClass = ClassComboBox.SelectedItem is Class;
            bool hasType = StudentTypeComboBox.SelectedItem is StudentType;
            button1.Enabled = hasName && hasClass && hasType;
        }

        private async Task OnSaveAsync()
        {
            string name = NameTextbox.Text == null ? string.Empty : NameTextbox.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Navn er påkrævet.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string address = AddressTextbox.Text == null ? string.Empty : AddressTextbox.Text.Trim();

            Class selectedClass = ClassComboBox.SelectedItem as Class;
            if (selectedClass == null)
            {
                MessageBox.Show("Vælg en klasse.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object typeObj = StudentTypeComboBox.SelectedItem;
            if (typeObj == null)
            {
                MessageBox.Show("Vælg en elevtype.", "Validering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StudentType type = (StudentType)typeObj;

            try
            {
                if (_isEdit && _existing != null)
                {
                    int rows = await _repository.UpdateStudentWithTypeStudentTypeAsync(
                        _existing.StudentID,
                        name,
                        address,
                        selectedClass.ClassID,
                        type,
                        default);

                    if (rows <= 0)
                    {
                        MessageBox.Show("Ingen ændringer foretaget.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    MessageBox.Show("Elev opdateret.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                int created = await _repository.AddStudentAsync(
                    name,
                    address,
                    selectedClass.ClassID,
                    type,
                    default);

                if (created <= 0)
                {
                    MessageBox.Show("Ingen elev blev oprettet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                MessageBox.Show("Elev oprettet.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke gemme eleven: {ex.Message}", "Fejl", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}