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
                    WriteHeader("Hovedmenu");

                    PrintMainMenu();

                    string input = AnsiConsole.Prompt(
                        new TextPrompt<string>("[cyan]Vælg et punkt[/]:").AllowEmpty());

                    switch (input)
                    {
                        case "1":
                            await RunStudentsMenuAsync(repo, cts.Token);
                            break;
                        case "2":
                            await RunClassesMenuAsync(repo, cts.Token);
                            break;
                        case "3":
                            await RunCoursesMenuAsync(repo, cts.Token);
                            break;
                        case "4":
                            await RunGradesMenuAsync(repo, cts.Token);
                            break;
                        case "0":
                            AnsiConsole.MarkupLine("[green]Farvel![/]");
                            running = false;
                            break;
                        default:
                            AnsiConsole.MarkupLine("[yellow]Ukendt valg. Prøv igen.[/]");
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Ubehandlet fejl:[/] {Markup.Escape(ex.Message)}");
        }
    }

    #region Menus (Main)

    private static void PrintMainMenu()
    {
        Panel panel = new Panel(
            new Markup(
                "1) Elever\n" +
                "2) Klasser\n" +
                "3) Fag\n" +
                "4) Karakterer\n" +
                "0) Afslut"))
        {
            Header = new PanelHeader("[bold yellow]Vælg[/]"),
            Border = BoxBorder.Rounded,
            Expand = false
        };

        AnsiConsole.Write(panel);
    }

    #endregion

    #region Students Menu

    private static async Task RunStudentsMenuAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        bool inMenu = true;
        while (inMenu)
        {
            WriteHeader("Elever");

            PrintStudentsMenu();

            string input = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Vælg et punkt[/]:").AllowEmpty());

            switch (input)
            {
                case "1":
                    await ListStudentsAsync(repo, cancellationToken);
                    break;
                case "2":
                    await CreateStudentAsync(repo, cancellationToken);
                    break;
                case "3":
                    await EditStudentAsync(repo, cancellationToken);
                    break;
                case "4":
                    await EnrollStudentAsync(repo, cancellationToken);
                    break;
                case "5":
                    await AddGradeAsync(repo, cancellationToken);
                    break;
                case "6":
                    await ShowFullStudentInfoViewAsync(repo, cancellationToken);
                    break;
                case "7":
                    await ShowFullStudentInfoByIdSpAsync(repo, cancellationToken);
                    break;
                case "8":
                    await DeleteStudentCascadeAsync(repo, cancellationToken);
                    break;
                case "9":
                    await ShowSingleStudentAsync(repo, cancellationToken);
                    break;
                case "10":
                    await RemoveEnrollmentAsync(repo, cancellationToken);
                    break;
                case "0":
                    inMenu = false;
                    break;
                default:
                    AnsiConsole.MarkupLine("[yellow]Ukendt valg. Prøv igen.[/]");
                    break;
            }
        }
    }

    private static void PrintStudentsMenu()
    {
        Panel panel = new Panel(
            new Markup(
                "1) Vis alle elever\n" +
                "2) Opret elev\n" +
                "3) Rediger elev\n" +
                "4) Tilmeld elev til klasse\n" +
                "5) Tilføj karakter til elev\n" +
                "6) Vis fuld elevinfo (view)\n" +
                "7) Vis fuld elevinfo pr. ID (stored procedure)\n" +
                "8) Slet elev (kaskade)\n" +
                "9) Vis elev efter ID\n" +
                "10) Afmeld elev fra klasse\n" +
                "0) Tilbage"))
        {
            Header = new PanelHeader("[bold yellow]Muligheder[/]"),
            Border = BoxBorder.Rounded,
            Expand = false
        };

        AnsiConsole.Write(panel);
    }

    #endregion

    #region Classes Menu

    private static async Task RunClassesMenuAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        bool inMenu = true;
        while (inMenu)
        {
            WriteHeader("Klasser");
            PrintClassesMenu();

            string input = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Vælg et punkt[/]:").AllowEmpty());

            switch (input)
            {
                case "1":
                    await ListClassesAsync(repo, cancellationToken);
                    break;
                case "2":
                    await CreateClassAsync(repo, cancellationToken);
                    break;
                case "3":
                    await EditClassAsync(repo, cancellationToken);
                    break;
                case "4":
                    await DeleteClassCascadeAsync(repo, cancellationToken);
                    break;
                case "0":
                    inMenu = false;
                    break;
                default:
                    AnsiConsole.MarkupLine("[yellow]Ukendt valg. Prøv igen.[/]");
                    break;
            }
        }
    }

    private static void PrintClassesMenu()
    {
        Panel panel = new Panel(
            new Markup(
                "1) Vis alle klasser\n" +
                "2) Opret klasse\n" +
                "3) Rediger klasse\n" +
                "4) Slet klasse (kaskade)\n" +
                "0) Tilbage"))
        {
            Header = new PanelHeader("[bold yellow]Muligheder[/]"),
            Border = BoxBorder.Rounded,
            Expand = false
        };
        AnsiConsole.Write(panel);
    }

    #endregion

    #region Courses Menu

    private static async Task RunCoursesMenuAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        bool inMenu = true;
        while (inMenu)
        {
            WriteHeader("Fag");
            PrintCoursesMenu();

            string input = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Vælg et punkt[/]:").AllowEmpty());

            switch (input)
            {
                case "1":
                    await ListCoursesAsync(repo, cancellationToken);
                    break;
                case "2":
                    await CreateCourseAsync(repo, cancellationToken);
                    break;
                case "3":
                    await EditCourseAsync(repo, cancellationToken);
                    break;
                case "4":
                    await DeleteCourseCascadeAsync(repo, cancellationToken);
                    break;
                case "0":
                    inMenu = false;
                    break;
                default:
                    AnsiConsole.MarkupLine("[yellow]Ukendt valg. Prøv igen.[/]");
                    break;
            }
        }
    }

    private static void PrintCoursesMenu()
    {
        Panel panel = new Panel(
            new Markup(
                "1) Vis alle fag\n" +
                "2) Opret fag\n" +
                "3) Rediger fag\n" +
                "4) Slet fag (kaskade)\n" +
                "0) Tilbage"))
        {
            Header = new PanelHeader("[bold yellow]Muligheder[/]"),
            Border = BoxBorder.Rounded,
            Expand = false
        };
        AnsiConsole.Write(panel);
    }

    #endregion

    #region Grades Menu

    private static async Task RunGradesMenuAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        bool inMenu = true;
        while (inMenu)
        {
            WriteHeader("Karakterer");
            PrintGradesMenu();

            string input = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Vælg et punkt[/]:").AllowEmpty());

            switch (input)
            {
                case "1":
                    await ListGradesForStudentAsync(repo, cancellationToken);
                    break;
                case "2":
                    await UpdateGradeAsync(repo, cancellationToken);
                    break;
                case "3":
                    await DeleteGradeAsync(repo, cancellationToken);
                    break;
                case "0":
                    inMenu = false;
                    break;
                default:
                    AnsiConsole.MarkupLine("[yellow]Ukendt valg. Prøv igen.[/]");
                    break;
            }
        }
    }

    private static void PrintGradesMenu()
    {
        Panel panel = new Panel(
            new Markup(
                "1) Vis karakterer for elev\n" +
                "2) Opdater karakter\n" +
                "3) Slet karakter\n" +
                "0) Tilbage"))
        {
            Header = new PanelHeader("[bold yellow]Muligheder[/]"),
            Border = BoxBorder.Rounded,
            Expand = false
        };
        AnsiConsole.Write(panel);
    }

    #endregion

    #region Student Operations

    private static async Task ListStudentsAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Student> students = await repo.GetStudentsAsync(cancellationToken);

            if (students.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Ingen elever fundet.[/]");
                return;
            }

            Table table = new Table
            {
                Title = new TableTitle("[bold yellow]Elever[/]"),
                Border = TableBorder.Rounded
            };
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Navn[/]");
            table.AddColumn("[bold]KlasseID[/]");
            table.AddColumn("[bold]Antal fag[/]");
            table.AddColumn("[bold]Sum tegn[/]");

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
            AnsiConsole.MarkupLine($"[red]Fejl ved visning af elever:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task CreateStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            string name = PromptString("Indtast elevnavn", true);
            if (string.IsNullOrWhiteSpace(name))
            {
                AnsiConsole.MarkupLine("[yellow]Navn er påkrævet.[/]");
                return;
            }

            string address = PromptString("Indtast adresse (valgfri)", false);

            await ListClassesAsync(repo, cancellationToken);
            int classId = PromptInt("Indtast klasse ID", 0, allowZero: false);

            int rows = await repo.AddStudentAsync(name, address, classId, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Elev oprettet.[/]" : "[yellow]Ingen elev oprettet.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved oprettelse af elev:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task EditStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast elev ID der skal redigeres", 0, allowZero: false);

            Student current = await repo.GetStudentByIdAsync(id, cancellationToken);
            if (current == null)
            {
                AnsiConsole.MarkupLine("[yellow]Elev findes ikke.[/]");
                return;
            }

            string name = PromptString("Indtast nyt navn (tom for at beholde)", false);
            string address = PromptString("Indtast ny adresse (tom for at beholde)", false);

            await ListClassesAsync(repo, cancellationToken);
            int classId = PromptInt("Indtast nyt klasse ID (0 for at beholde)", 0, allowZero: true);

            string effectiveName = string.IsNullOrWhiteSpace(name) ? current.StudentName : name;
            string effectiveAddress = string.IsNullOrWhiteSpace(address) ? current.StudentAddress : address;
            int effectiveClassId = classId == 0 ? current.ClassID : classId;

            int rows = await repo.UpdateStudentAsync(id, effectiveName, effectiveAddress, effectiveClassId, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Elev opdateret.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved redigering af elev:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task EnrollStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int studentId = PromptInt("Indtast elev ID", 0, allowZero: false);
            await ListClassesAsync(repo, cancellationToken);
            int classId = PromptInt("Indtast klasse ID", 0, allowZero: false);

            int rows = await repo.EnrollStudentInClassAsync(studentId, classId, DateTime.UtcNow, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Tilmelding gemt.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved tilmelding:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task RemoveEnrollmentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int studentId = PromptInt("Indtast elev ID", 0, allowZero: false);
            int classId = PromptInt("Indtast klasse ID", 0, allowZero: false);

            if (!PromptConfirm("Er du sikker på at du vil afmelde eleven fra klassen?"))
            {
                AnsiConsole.MarkupLine("[yellow]Annulleret.[/]");
                return;
            }

            int rows = await repo.RemoveEnrollmentAsync(studentId, classId, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Afmelding udført.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved afmelding:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task AddGradeAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int studentId = PromptInt("Indtast elev ID", 0, allowZero: false);
            int classId = PromptInt("Indtast klasse ID", 0, allowZero: false);
            int courseId = PromptInt("Indtast fag ID", 0, allowZero: false);
            int grade = PromptInt("Indtast karakter", 0, allowZero: false);

            int rows = await repo.AddGradeAsync(studentId, classId, courseId, grade, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Karakter tilføjet.[/]" : "[yellow]Ingen karakter tilføjet.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved tilføjelse af karakter:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task ShowFullStudentInfoViewAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<FullStudentInfoRow> rows = await repo.GetFullStudentInfoViewAsync(cancellationToken);
            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Ingen data fra view.[/]");
                return;
            }

            Table table = new Table
            {
                Title = new TableTitle("[bold yellow]Fuld elevinfo (View)[/]"),
                Border = TableBorder.Rounded
            };
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Navn[/]");
            table.AddColumn("[bold]Klasse[/]");
            table.AddColumn("[bold]Fag[/]");
            table.AddColumn("[bold]Antal fag[/]");

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
            AnsiConsole.MarkupLine($"[red]Fejl ved læsning af view:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task ShowFullStudentInfoByIdSpAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast elev ID", 0, allowZero: false);

            IReadOnlyList<FullStudentInfoRow> rows = await repo.GetFullStudentInfoSpByIdAsync(id, cancellationToken);
            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Ingen data for dette elev ID.[/]");
                return;
            }

            Table table = new Table
            {
                Title = new TableTitle($"[bold yellow]Fuld elevinfo (SP) - ID {id}[/]"),
                Border = TableBorder.Rounded
            };
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Navn[/]");
            table.AddColumn("[bold]Klasse[/]");
            table.AddColumn("[bold]Fag[/]");

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
            AnsiConsole.MarkupLine($"[red]Fejl ved kald af stored procedure:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task ShowSingleStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast elev ID", 0, allowZero: false);
            Student s = await repo.GetStudentByIdAsync(id, cancellationToken);
            if (s == null)
            {
                AnsiConsole.MarkupLine("[yellow]Elev findes ikke.[/]");
                return;
            }

            Table table = new Table
            {
                Title = new TableTitle($"[bold yellow]Elev ID {s.StudentID}[/]"),
                Border = TableBorder.Rounded
            };
            table.AddColumn("[bold]Navn[/]");
            table.AddColumn("[bold]Adresse[/]");
            table.AddColumn("[bold]KlasseID[/]");
            table.AddColumn("[bold]Antal fag[/]");
            table.AddColumn("[bold]Sum tegn[/]");

            table.AddRow(
                $"[green]{Escape(s.StudentName)}[/]",
                $"[white]{Escape(s.StudentAddress)}[/]",
                $"[white]{s.ClassID}[/]",
                $"[white]{s.StudentNumberOfCourses}[/]",
                $"[white]{s.StudentSumOfAllCharacters}[/]");

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved hentning af elev:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task DeleteStudentCascadeAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast elev ID der skal slettes (kaskade)", 0, allowZero: false);

            if (!PromptConfirm("Er du sikker på at du vil slette eleven og alle relaterede data?"))
            {
                AnsiConsole.MarkupLine("[yellow]Annulleret.[/]");
                return;
            }

            int rows = await repo.DeleteStudentCascadeAsync(id, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Elev (og relaterede data) slettet.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved sletning:[/] {Escape(ex.Message)}");
        }
    }

    #endregion

    #region Class Operations

    private static async Task ListClassesAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Class> classes = await repo.GetClassesAsync(cancellationToken);
            if (classes.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Ingen klasser fundet.[/]");
                return;
            }

            Table table = new Table
            {
                Title = new TableTitle("[bold yellow]Klasser[/]"),
                Border = TableBorder.Rounded
            };
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Navn[/]");
            table.AddColumn("[bold]Beskrivelse[/]");

            foreach (Class c in classes.OrderBy(c => c.ClassID))
            {
                table.AddRow(
                    $"[white]{c.ClassID}[/]",
                    $"[green]{Escape(c.ClassName)}[/]",
                    $"[white]{Escape(c.ClassDescription)}[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved visning af klasser:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task CreateClassAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            string name = PromptString("Indtast klassenavn", true);
            string desc = PromptString("Indtast beskrivelse (valgfri)", false);

            int rows = await repo.AddClassAsync(name, desc, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Klasse oprettet.[/]" : "[yellow]Ingen klasse oprettet.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved oprettelse af klasse:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task EditClassAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast klasse ID der skal redigeres", 0, allowZero: false);
            // Ingen GetClassById kald her (kunne tilføjes), vi forsøger opdatering direkte.

            string name = PromptString("Indtast nyt navn (tom = behold)", false);
            string desc = PromptString("Indtast ny beskrivelse (tom = behold)", false);

            // Because we do not fetch existing row (to keep it light), require at least one input:
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(desc))
            {
                AnsiConsole.MarkupLine("[yellow]Ingen ændringer angivet.[/]");
                return;
            }

            // If you want to retain existing values you'd need a GetClassById; skipping for brevity.
            string effectiveName = string.IsNullOrWhiteSpace(name) ? "(ingen ændring)" : name;
            string effectiveDesc = string.IsNullOrWhiteSpace(desc) ? "(ingen ændring)" : desc;

            // We still pass empty strings if user left blank; optionally fetch original row to merge.
            int rows = await repo.UpdateClassAsync(id,
                string.IsNullOrWhiteSpace(name) ? effectiveName : name,
                string.IsNullOrWhiteSpace(desc) ? effectiveDesc : desc,
                cancellationToken);

            AnsiConsole.MarkupLine(rows > 0 ? "[green]Klasse opdateret.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved opdatering af klasse:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task DeleteClassCascadeAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast klasse ID der skal slettes (kaskade)", 0, allowZero: false);

            if (!PromptConfirm("ADVARSEL: Dette sletter alle elever i klassen og deres data. Fortsæt?"))
            {
                AnsiConsole.MarkupLine("[yellow]Annulleret.[/]");
                return;
            }

            int rows = await repo.DeleteClassCascadeAsync(id, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Klasse (og relaterede data) slettet.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved sletning af klasse:[/] {Escape(ex.Message)}");
        }
    }

    #endregion

    #region Course Operations

    private static async Task ListCoursesAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Course> courses = await repo.GetCoursesAsync(cancellationToken);
            if (courses.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Ingen fag fundet.[/]");
                return;
            }

            Table table = new Table
            {
                Title = new TableTitle("[bold yellow]Fag[/]"),
                Border = TableBorder.Rounded
            };
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Navn[/]");

            foreach (Course c in courses.OrderBy(c => c.CourseID))
            {
                table.AddRow(
                    $"[white]{c.CourseID}[/]",
                    $"[green]{Escape(c.CourseName)}[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved visning af fag:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task CreateCourseAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            string name = PromptString("Indtast fagnavn", true);
            int rows = await repo.AddCourseAsync(name, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Fag oprettet.[/]" : "[yellow]Ingen fag oprettet.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved oprettelse af fag:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task EditCourseAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast fag ID der skal redigeres", 0, allowZero: false);
            string name = PromptString("Indtast nyt navn (tom = behold)", false);

            if (string.IsNullOrWhiteSpace(name))
            {
                AnsiConsole.MarkupLine("[yellow]Ingen ændringer angivet.[/]");
                return;
            }

            int rows = await repo.UpdateCourseAsync(id, name, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Fag opdateret.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved opdatering af fag:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task DeleteCourseCascadeAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int id = PromptInt("Indtast fag ID der skal slettes (kaskade)", 0, allowZero: false);
            if (!PromptConfirm("ADVARSEL: Dette sletter alle karakterer relateret til faget. Fortsæt?"))
            {
                AnsiConsole.MarkupLine("[yellow]Annulleret.[/]");
                return;
            }

            int rows = await repo.DeleteCourseCascadeAsync(id, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Fag (og relaterede karakterer) slettet.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved sletning af fag:[/] {Escape(ex.Message)}");
        }
    }

    #endregion

    #region Grades Operations

    private static async Task ListGradesForStudentAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int studentId = PromptInt("Indtast elev ID", 0, allowZero: false);
            IReadOnlyList<StudentClassRepetitionOnClass> grades = await repo.GetGradesByStudentAsync(studentId, cancellationToken);
            if (grades.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Ingen karakterer fundet for eleven.[/]");
                return;
            }

            Table table = new Table
            {
                Title = new TableTitle($"[bold yellow]Karakterer for elev {studentId}[/]"),
                Border = TableBorder.Rounded
            };
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]ElevID[/]");
            table.AddColumn("[bold]KlasseID[/]");
            table.AddColumn("[bold]FagID[/]");
            table.AddColumn("[bold]Karakter[/]");

            foreach (StudentClassRepetitionOnClass g in grades.OrderBy(g => g.StudentClassID))
            {
                table.AddRow(
                    $"[white]{g.StudentClassID}[/]",
                    $"[white]{g.StudentID}[/]",
                    $"[white]{g.ClassID}[/]",
                    $"[white]{g.CourseID}[/]",
                    $"[green]{g.Grade}[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved visning af karakterer:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task UpdateGradeAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int gradeId = PromptInt("Indtast karakter ID (StudentClassID)", 0, allowZero: false);
            int newGrade = PromptInt("Indtast ny karakter", 0, allowZero: false);

            int rows = await repo.UpdateGradeAsync(gradeId, newGrade, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Karakter opdateret.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved opdatering af karakter:[/] {Escape(ex.Message)}");
        }
    }

    private static async Task DeleteGradeAsync(SchoolRepository repo, CancellationToken cancellationToken)
    {
        try
        {
            int gradeId = PromptInt("Indtast karakter ID (StudentClassID) der skal slettes", 0, allowZero: false);
            if (!PromptConfirm("Er du sikker på at du vil slette karakteren?"))
            {
                AnsiConsole.MarkupLine("[yellow]Annulleret.[/]");
                return;
            }
            int rows = await repo.DeleteGradeAsync(gradeId, cancellationToken);
            AnsiConsole.MarkupLine(rows > 0 ? "[green]Karakter slettet.[/]" : "[yellow]Ingen ændring.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fejl ved sletning af karakter:[/] {Escape(ex.Message)}");
        }
    }

    #endregion

    #region Shared Helpers (Prompts / Output)

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
            prompt.ValidationErrorMessage("[red]Værdi er påkrævet[/]");
            prompt.Validate(input =>
            {
                return string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Error("[red]Værdi er påkrævet[/]")
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
            .ValidationErrorMessage("[red]Indtast et gyldigt ikke-negativt tal[/]")
            .Validate(v =>
            {
                if (v < 0)
                {
                    return ValidationResult.Error("[red]Tal skal være >= 0[/]");
                }

                if (!allowZero && v == 0)
                {
                    return ValidationResult.Error("[red]Nul er ikke tilladt her[/]");
                }

                return ValidationResult.Success();
            });

        int value = AnsiConsole.Prompt(prompt);
        return value;
    }

    private static bool PromptConfirm(string message)
    {
        return AnsiConsole.Confirm($"[yellow]{Escape(message)}[/]");
    }

    private static string Escape(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        return Markup.Escape(input);
    }

    #endregion
}