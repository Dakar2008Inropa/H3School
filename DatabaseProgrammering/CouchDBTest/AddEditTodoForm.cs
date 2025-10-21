using CouchDBTest.CouchDB;

namespace CouchDBTest
{
    public partial class AddEditTodoForm : Form
    {
        private readonly CouchDbClient _client;

        private readonly bool _isEdit;

        private readonly TodoDoc _existing;

        public TodoDoc Result { get; private set; }

        public AddEditTodoForm(CouchDbClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _isEdit = false;
            _existing = null;

            InitializeComponent();

            SaveBtn.Click += async (s, e) => await OnSaveAsync();
            Shown += (s, e) => InitializeUi();
        }

        public AddEditTodoForm(CouchDbClient client, TodoDoc existing)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _isEdit = true;
            _existing = existing ?? throw new ArgumentNullException(nameof(existing));

            InitializeComponent();

            SaveBtn.Click += async (s, e) => await OnSaveAsync();
            Shown += (s, e) => InitializeUi();
        }

        private void InitializeUi()
        {
            if (_isEdit && _existing != null)
            {
                Text = "Edit Todo";
                label1.Text = "Title";
                textBox1.Text = _existing.Title;
                IsDoneCheckbox.Checked = _existing.IsDone;
                SaveBtn.Text = "Save changes";
                return;
            }

            Text = "Create New Todo";
            label1.Text = "Title";
            textBox1.Text = string.Empty;
            IsDoneCheckbox.Checked = false;
            SaveBtn.Text = "Save";
        }

        private async Task OnSaveAsync()
        {
            string title = textBox1.Text == null ? string.Empty : textBox1.Text.Trim();
            if (title.Length == 0)
            {
                MessageBox.Show("Title is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isDone = IsDoneCheckbox.Checked;

            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource();

                if (_isEdit && _existing != null)
                {
                    _existing.Title = title;
                    _existing.IsDone = isDone;

                    TodoDoc updated = await _client.UpdateAsync(_existing, cts.Token).ConfigureAwait(true);
                    Result = updated;

                    MessageBox.Show("Todo updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                TodoDoc toCreate = new TodoDoc
                {
                    Title = title,
                    IsDone = isDone
                };

                TodoDoc created = await _client.CreateAsync(toCreate, cts.Token).ConfigureAwait(true);
                Result = created;

                MessageBox.Show("Todo created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the Todo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}