using OwnORM.Models;
using OwnORM.Models.Views;
using OwnORM.Repositories;
using Spectre.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        try
        {
            using (SchoolRepository repo = new SchoolRepository())
            {
                bool running = true;
                while (running)
                {
                    WriteHeader("Main Menu");

                    PrintMainMenu();

                    string input = AnsiConsole.Prompt(
                        new TextPrompt<string>("[cyan]Select an option[/]:").AllowEmpty());

                    if (input == "1")
                    {
                        await RunStudentsMenuAsync(repo, cts.Token);
                        continue;
                    }

                    if (input == "0")
                    {
                        AnsiConsole.MarkupLine("[green]Goodbye![/]");
                        running = false;
                        continue;
                    }

                    AnsiConsole.MarkupLine("[yellow]Unknown option. Please try again.[/]");
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Unhandled error:[/] {ex.Message}");
        }
    }

    private static void PrintMainMenu()
    {
        Panel panel = new Panel(
            new Markup(
                "1) Students\n" +
                "0) Exit"))
        {
            Header = new PanelHeader("[bold yellow]Choose[/]"),
            Border = BoxBorder.Rounded,
            Expand = false
        };

        AnsiConsole.Write(panel);
    }

    private static async Task RunStudentsMenuAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        bool inStudents = true;
        while (inStudents)
        {
            WriteHeader("Students");

            PrintStudentsMenu();

            string input = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Select an option[/]:").AllowEmpty());

            if (input == "1")
            {
                await ListStudentsAsync(repo, cancellationToken);
                continue;
            }

            if (input == "2")
            {
                await CreateStudentAsync(repo, cancellationToken);
                continue;
            }

            if (input == "3")
            {
                await EditStudentAsync(repo, cancellationToken);
                continue;
            }

            if (input == "4")
            {
                await EnrollStudentAsync(repo, cancellationToken);
                continue;
            }

            if (input == "5")
            {
                await AddGradeAsync(repo, cancellationToken);
                continue;
            }

            if (input == "6")
            {
                await ShowFullStudentInfoViewAsync(repo, cancellationToken);
                continue;
            }

            if (input == "7")
            {
                await ShowFullStudentInfoByIdSpAsync(repo, cancellationToken);
                continue;
            }

            if (input == "0")
            {
                inStudents = false;
                continue;
            }

            AnsiConsole.MarkupLine("[yellow]Unknown option. Please try again.[/]");
        }
    }

    private static void PrintStudentsMenu()
    {
        Panel panel = new Panel(
            new Markup(
                "1) List students\n" +
                "2) Create student\n" +
                "3) Edit student\n" +
                "4) Enroll student in class\n" +
                "5) Add grade to student\n" +
                "6) Show full student info (view)\n" +
                "7) Show full student info by ID (stored procedure)\n" +
                "0) Back"))
        {
            Header = new PanelHeader("[bold yellow]Options[/]"),
            Border = BoxBorder.Rounded,
            Expand = false
        };

        AnsiConsole.Write(panel);
    }

    private static async Task ListStudentsAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Student> students = await repo.GetStudentsAsync(cancellationToken);

            if (students.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No students found.[/]");
                return;
            }

            Table table = new Table();
            table.Title = new TableTitle("[bold yellow]Students[/]");
            table.Border = TableBorder.Rounded;
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Name[/]");
            table.AddColumn("[bold]ClassID[/]");
            table.AddColumn("[bold]Courses[/]");
            table.AddColumn("[bold]Sum[/]");

            foreach (Student s in students.OrderBy(s => s.StudentID))
            {
                table.AddRow(
                    $"[white]{s.StudentID}[/]",
                    $"[green]{Escape(s.StudentName)}[/]",
                    $"[white]{s.ClassID}[/]",
                    $"[white]{s.StudentNumberOfCourses}[/]",
                    $"[white]{s.StudentSumOfAllCharacters}[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error listing students:[/] {ex.Message}");
        }
    }

    private static async Task CreateStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            string name = PromptString("Enter student name", true);
            if (string.IsNullOrWhiteSpace(name))
            {
                AnsiConsole.MarkupLine("[yellow]Name is required.[/]");
                return;
            }

            string address = PromptString("Enter address (optional)", false);

            await PrintClassesAsync(repo, cancellationToken);
            int classId = PromptInt("Enter class id", 0, allowZero: false);

            int rows = await repo.AddStudentAsync(name, address, classId, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Student created.[/]" : "[yellow]No student created.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating student:[/] {ex.Message}");
        }
    }

    private static async Task EditStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Enter student id to edit", 0, allowZero: false);

            string name = PromptString("Enter new name (empty to keep)", false);
            string address = PromptString("Enter new address (empty to keep)", false);

            await PrintClassesAsync(repo, cancellationToken);
            int classId = PromptInt("Enter new class id (0 to keep current)", 0, allowZero: true);

            IReadOnlyList<Student> all = await repo.GetStudentsAsync(cancellationToken);
            Student current = all.FirstOrDefault(s => s.StudentID == id);
            if (current == null)
            {
                AnsiConsole.MarkupLine("[yellow]Student not found.[/]");
                return;
            }

            string effectiveName = string.IsNullOrWhiteSpace(name) ? current.StudentName : name;
            string effectiveAddress = string.IsNullOrWhiteSpace(address) ? current.StudentAddress : address;
            int effectiveClassId = classId == 0 ? current.ClassID : classId;

            int rows = await repo.UpdateStudentAsync(id, effectiveName, effectiveAddress, effectiveClassId, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Student updated.[/]" : "[yellow]No student updated.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error editing student:[/] {ex.Message}");
        }
    }

    private static async Task EnrollStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int studentId = PromptInt("Enter student id", 0, allowZero: false);

            await PrintClassesAsync(repo, cancellationToken);
            int classId = PromptInt("Enter class id", 0, allowZero: false);

            int rows = await repo.EnrollStudentInClassAsync(studentId, classId, DateTime.UtcNow, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Enrollment saved.[/]" : "[yellow]No enrollment changed.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error enrolling student:[/] {ex.Message}");
        }
    }

    private static async Task AddGradeAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int studentId = PromptInt("Enter student id", 0, allowZero: false);

            await PrintClassesAsync(repo, cancellationToken);
            int classId = PromptInt("Enter class id", 0, allowZero: false);

            int courseId = PromptInt("Enter course id", 0, allowZero: false);
            int grade = PromptInt("Enter grade", 0, allowZero: false);

            int rows = await repo.AddGradeAsync(studentId, classId, courseId, grade, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Grade added.[/]" : "[yellow]No grade added.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error adding grade:[/] {ex.Message}");
        }
    }

    private static async Task ShowFullStudentInfoViewAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<FullStudentInfoRow> rows = await repo.GetFullStudentInfoViewAsync(cancellationToken);
            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No rows from view.[/]");
                return;
            }

            Table table = new Table();
            table.Title = new TableTitle("[bold yellow]Full Student Info (View)[/]");
            table.Border = TableBorder.Rounded;
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Name[/]");
            table.AddColumn("[bold]Class[/]");
            table.AddColumn("[bold]Course[/]");
            table.AddColumn("[bold]Courses Count[/]");

            foreach (FullStudentInfoRow r in rows.OrderBy(r => r.StudentID))
            {
                table.AddRow(
                    $"[white]{r.StudentID}[/]",
                    $"[green]{Escape(r.StudentName)}[/]",
                    $"[yellow]{Escape(r.ClassName)}[/]",
                    $"[cyan]{Escape(r.CourseName)}[/]",
                    $"[white]{r.StudentNumberOfCourses}[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error reading view:[/] {ex.Message}");
        }
    }

    private static async Task ShowFullStudentInfoByIdSpAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Enter student id", 0, allowZero: false);

            IReadOnlyList<FullStudentInfoRow> rows = await repo.GetFullStudentInfoSpByIdAsync(id, cancellationToken);
            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No data for that student id.[/]");
                return;
            }

            Table table = new Table();
            table.Title = new TableTitle($"[bold yellow]Full Student Info (SP) - ID {id}[/]");
            table.Border = TableBorder.Rounded;
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Name[/]");
            table.AddColumn("[bold]Class[/]");
            table.AddColumn("[bold]Course[/]");

            foreach (FullStudentInfoRow r in rows)
            {
                table.AddRow(
                    $"[white]{r.StudentID}[/]",
                    $"[green]{Escape(r.StudentName)}[/]",
                    $"[yellow]{Escape(r.ClassName)}[/]",
                    $"[cyan]{Escape(r.CourseName)}[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error calling stored procedure:[/] {ex.Message}");
        }
    }

    private static async Task PrintClassesAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Class> classes = await repo.GetClassesAsync(cancellationToken);

            if (classes.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No classes found.[/]");
                return;
            }

            Table table = new Table();
            table.Title = new TableTitle("[bold yellow]Known Classes[/]");
            table.Border = TableBorder.Rounded;
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Name[/]");

            foreach (Class c in classes.OrderBy(c => c.ClassID))
            {
                table.AddRow($"[white]{c.ClassID}[/]", $"[green]{Escape(c.ClassName)}[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error listing classes:[/] {ex.Message}");
        }
    }

    private static void WriteHeader(string text)
    {
        Rule rule = new Rule($"[bold yellow]{Escape(text)}[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(rule);
    }

    private static string PromptString(string label, bool required)
    {
        TextPrompt<string> prompt = new TextPrompt<string>($"[cyan]{Escape(label)}[/]:")
            .AllowEmpty();

        if (required)
        {
            prompt.ValidationErrorMessage("[red]Value is required[/]");
            prompt.Validate(input =>
            {
                return string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Error("[red]Value is required[/]")
                    : ValidationResult.Success();
            });
        }

        string value = AnsiConsole.Prompt(prompt);
        return value ?? string.Empty;
    }

    private static int PromptInt(string label, int defaultValue, bool allowZero)
    {
        TextPrompt<int> prompt = new TextPrompt<int>($"[cyan]{Escape(label)}[/]:")
            .DefaultValue(defaultValue)
            .ValidationErrorMessage("[red]Please enter a valid non-negative number[/]")
            .Validate(v =>
            {
                if (v < 0)
                {
                    return ValidationResult.Error("[red]Number must be >= 0[/]");
                }

                if (!allowZero && v == 0)
                {
                    return ValidationResult.Error("[red]Zero is not allowed here[/]");
                }

                return ValidationResult.Success();
            });

        int value = AnsiConsole.Prompt(prompt);
        return value;
    }

    private static string Escape(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return Markup.Escape(input);
    }
}