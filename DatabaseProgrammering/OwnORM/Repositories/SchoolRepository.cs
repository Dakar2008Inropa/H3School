using OwnORM.Data;
using OwnORM.Models;
using OwnORM.Models.Views;

namespace OwnORM.Repositories
{
    public sealed class SchoolRepository : IDisposable
    {
        private const string DefaultConnectionString = "Server=.;Database=H3PD100125;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly SqlDb _db;

        public SchoolRepository(string connectionString = DefaultConnectionString)
        {
            _db = new SqlDb(connectionString);
        }

        public Task<IReadOnlyList<StudentClassViewRow>> GetStudentClassViewAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT StudentID, StudentName, StudentAddress, ClassName FROM dbo.StudentClassView";
            return _db.QueryAsync<StudentClassViewRow>(sql, null, cancellationToken);
        }

        public Task<IReadOnlyList<FullStudentInfoRow>> GetFullStudentInfoViewAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT StudentID, StudentName, StudentAddress, StudentNumberOfCourses, ClassName, CourseName FROM dbo.FullStudentInfoView";
            return _db.QueryAsync<FullStudentInfoRow>(sql, null, cancellationToken);
        }

        public Task<IReadOnlyList<TestViewRow>> GetTestViewAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT CourseName, StudentName, ClassID, Grade, ClassName FROM dbo.Test_View";
            return _db.QueryAsync<TestViewRow>(sql, null, cancellationToken);
        }

        public Task<IReadOnlyList<FullStudentInfoRow>> GetFullStudentInfoSpAsync(CancellationToken cancellationToken)
        {
            return _db.QueryStoredAsync<FullStudentInfoRow>("dbo.FullStudentInfo_SP", null, cancellationToken);
        }

        public Task<IReadOnlyList<FullStudentInfoRow>> GetFullStudentInfoSpByIdAsync(int studentId, CancellationToken cancellationToken)
        {
            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@StudentId", studentId }
            };

            return _db.QueryStoredAsync<FullStudentInfoRow>("dbo.FullStudentInfo_SP_By_ID", p, cancellationToken);
        }

        public async Task<int> AddStudentAsync(string studentName, string studentAddress, int classId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(studentName))
                throw new ArgumentException("Student name must not be empty.", nameof(studentName));

            string sql = @"INSERT INTO dbo.Student (StudentName, StudentAddress, ClassID, StudentNumberOfCourses, StudentSumOfAllCharacters) VALUES (@StudentName, @StudentAddress, @ClassID, 0, 0);";

            Dictionary<string, object> p = new Dictionary<string, object>
            {
                {"@StudentName", studentName },
                {"@StudentAddress", studentAddress ?? string.Empty },
                {"@ClassID", classId }
            };

            return await _db.ExecuteAsync(sql, p, cancellationToken).ConfigureAwait(false);
        }

        public Task<int> UpdateStudentAsync(int studentId, string studentName, string studentAddress, int classId, CancellationToken cancellationToken)
        {
            string sql = @"
UPDATE dbo.Student
SET StudentName = @StudentName,
    StudentAddress = @StudentAddress,
    ClassID = @ClassID
WHERE StudentID = @StudentID;";

            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@StudentID", studentId },
                { "@StudentName", string.IsNullOrWhiteSpace(studentName) ? string.Empty : studentName },
                { "@StudentAddress", studentAddress ?? string.Empty },
                { "@ClassID", classId }
            };

            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<int> EnrollStudentInClassAsync(int studentId, int classId, DateTime startDate, CancellationToken cancellationToken)
        {
            string sql = @"
MERGE dbo.Student_Class_Collection AS target
USING (SELECT @StudentID AS StudentID, @ClassID AS ClassID) AS src
ON (target.StudentID = src.StudentID AND target.ClassID = src.ClassID)
WHEN MATCHED THEN 
    UPDATE SET StartDate = @StartDate
WHEN NOT MATCHED THEN
    INSERT (StudentID, ClassID, StartDate) VALUES (@StudentID, @ClassID, @StartDate);";

            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@StudentID", studentId },
                { "@ClassID", classId },
                { "@StartDate", startDate }
            };

            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<int> AddGradeAsync(int studentId, int classId, int courseId, int grade, CancellationToken cancellationToken)
        {
            string sql = @"
INSERT INTO dbo.StudentClass_RepetitionOnClass (StudentID, ClassID, Grade, CourseID)
VALUES (@StudentID, @ClassID, @Grade, @CourseID);";

            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@StudentID", studentId },
                { "@ClassID", classId },
                { "@Grade", grade },
                { "@CourseID", courseId }
            };

            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT StudentID, StudentName, StudentAddress, ClassID, StudentNumberOfCourses, StudentSumOfAllCharacters FROM dbo.Student";
            return _db.QueryAsync<Student>(sql, null, cancellationToken);
        }

        public Task<IReadOnlyList<Class>> GetClassesAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT ClassID, ClassName, ClassDescription FROM dbo.Class";
            return _db.QueryAsync<Class>(sql, null, cancellationToken);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}