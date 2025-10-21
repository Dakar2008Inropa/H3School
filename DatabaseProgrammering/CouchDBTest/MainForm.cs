using CouchDBTest.CouchDB;
using System.ComponentModel;

namespace CouchDBTest
{
    public partial class MainForm : Form
    {
        private readonly CouchDbClient _client;

        private readonly BindingList<TodoDoc> _todos = new BindingList<TodoDoc>();

        public MainForm()
        {
            InitializeComponent();

            CouchDbOptions options = BuildOptions();
            _client = new CouchDbClient(options);

            ConfigureGrid();
            WireEvents();

            ApplyHideIdAndRev(HideIdAndRevCheckbox.Checked);

            Shown += async (s, e) => await ReloadAsync();
        }

        private static CouchDbOptions BuildOptions()
        {
            CouchDbOptions options = new CouchDbOptions
            {
                BaseUri = "http://localhost:5984",
                DatabaseName = "todos",
                Username = "admin",
                Password = "Dakar1326"
            };
            return options;
        }

        private void ConfigureGrid()
        {
            TodoDataGridView.AutoGenerateColumns = false;
            TodoDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            TodoDataGridView.MultiSelect = false;
            TodoDataGridView.AllowUserToResizeColumns = false;
            TodoDataGridView.AllowUserToResizeRows = false;
            TodoDataGridView.ReadOnly = true;

            TodoDataGridView.Columns.Clear();

            DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TodoDoc.Id),
                HeaderText = "Id",
                Width = 260,
                ReadOnly = true
            };
            TodoDataGridView.Columns.Add(idCol);

            DataGridViewTextBoxColumn titleCol = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TodoDoc.Title),
                HeaderText = "Title",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };
            TodoDataGridView.Columns.Add(titleCol);

            DataGridViewCheckBoxColumn doneCol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(TodoDoc.IsDone),
                HeaderText = "Is Done",
                Width = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };
            TodoDataGridView.Columns.Add(doneCol);

            DataGridViewTextBoxColumn revCol = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TodoDoc.Rev),
                HeaderText = "Rev",
                Width = 160,
                ReadOnly = true
            };
            TodoDataGridView.Columns.Add(revCol);

            TodoDataGridView.RowHeadersVisible = false;

            TodoDataGridView.DataSource = _todos;
        }

        private void WireEvents()
        {
            CreateNewTodoBtn.Click += async (s, e) => await OnCreateAsync();
            EditSelectedTodoBtn.Click += async (s, e) => await OnEditAsync();
            DeleteSelectedTodoBtn.Click += async (s, e) => await OnDeleteAsync();

            TodoDataGridView.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    await OnEditAsync();
                }
            };

            HideIdAndRevCheckbox.CheckedChanged += (s, e) =>
            {
                ApplyHideIdAndRev(HideIdAndRevCheckbox.Checked);
            };
        }

        private async Task ReloadAsync()
        {
            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource();

                var docs = await _client.GetAllAsync<TodoDoc>(1000, cts.Token).ConfigureAwait(true);

                _todos.Clear();
                foreach (TodoDoc d in docs.OrderBy(d => d.Title))
                {
                    _todos.Add(d);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Todos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnCreateAsync()
        {
            using AddEditTodoForm dlg = new AddEditTodoForm(_client);
            DialogResult dr = dlg.ShowDialog(this);
            if (dr != DialogResult.OK)
                return;

            await ReloadAsync();
        }

        private async Task OnEditAsync()
        {
            TodoDoc selected = GetSelected();
            if (selected == null)
            {
                MessageBox.Show("Select a Todo to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource();

                TodoDoc latest = await _client.GetAsync<TodoDoc>(selected.Id, cts.Token).ConfigureAwait(true);

                using AddEditTodoForm dlg = new AddEditTodoForm(_client, latest);
                DialogResult dr = dlg.ShowDialog(this);
                if (dr != DialogResult.OK)
                    return;

                await ReloadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open Todo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnDeleteAsync()
        {
            TodoDoc selected = GetSelected();
            if (selected == null)
            {
                MessageBox.Show("Select a Todo to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show("Delete the selected Todo?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource();

                await _client.DeleteAsync(selected.Id, selected.Rev, cts.Token).ConfigureAwait(true);

                await ReloadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete Todo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TodoDoc GetSelected()
        {
            if (TodoDataGridView.CurrentRow == null)
                return null;

            return TodoDataGridView.CurrentRow.DataBoundItem as TodoDoc;
        }

        private void ApplyHideIdAndRev(bool hide)
        {
            DataGridViewColumn idCol = TodoDataGridView.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c => string.Equals(c.DataPropertyName, nameof(TodoDoc.Id), StringComparison.Ordinal));

            DataGridViewColumn revCol = TodoDataGridView.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c => string.Equals(c.DataPropertyName, nameof(TodoDoc.Rev), StringComparison.Ordinal));

            if (idCol != null) idCol.Visible = !hide;
            if (revCol != null) revCol.Visible = !hide;
        }
    }
}