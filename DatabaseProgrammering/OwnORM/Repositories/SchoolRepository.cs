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

        public Task<IReadOnlyList<FullStudentInfoRow>> GetFullStudentInfoViewAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT StudentID, StudentName, StudentAddress, StudentNumberOfCourses, ClassName, CourseName FROM dbo.FullStudentInfoView";
            return _db.QueryAsync<FullStudentInfoRow>(sql, null, cancellationToken);
        }

        public Task<IReadOnlyList<FullStudentInfoRow>> GetFullStudentInfoSpByIdAsync(int studentId, CancellationToken cancellationToken)
        {
            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@StudentId", studentId }
            };

            return _db.QueryStoredAsync<FullStudentInfoRow>("dbo.FullStudentInfo_SP_By_ID", p, cancellationToken);
        }


        public async Task<int> AddStudentAsync(string studentName, string studentAddress, int classId, StudentType studentType, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(studentName))
                throw new ArgumentException("Student name must not be empty.", nameof(studentName));

            string sql = @"INSERT INTO dbo.Student (StudentName, StudentAddress, ClassID, StudentNumberOfCourses, StudentSumOfAllCharacters, StudentType)
VALUES (@StudentName, @StudentAddress, @ClassID, 0, 0, @StudentType);";

            Dictionary<string, object> p = new Dictionary<string, object>
            {
                {"@StudentName", studentName },
                {"@StudentAddress", studentAddress ?? string.Empty },
                {"@ClassID", classId },
                {"@StudentType", (int)studentType }
            };

            return await _db.ExecuteAsync(sql, p, cancellationToken).ConfigureAwait(false);
        }

        public Task<int> UpdateStudentWithTypeStudentTypeAsync(int studentId, string studentName, string studentAddress, int classId, StudentType studentType, CancellationToken cancellationToken)
        {
            string sql = @"
UPDATE dbo.Student
SET StudentName = @StudentName,
    StudentAddress = @StudentAddress,
    ClassID = @ClassID,
    StudentType = @StudentType
WHERE StudentID = @StudentID;";

            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@StudentID", studentId },
                { "@StudentName", string.IsNullOrWhiteSpace(studentName) ? string.Empty : studentName },
                { "@StudentAddress", studentAddress ?? string.Empty },
                { "@ClassID", classId },
                { "@StudentType", (int)studentType }
            };

            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public async Task<Student> GetStudentByIdAsync(int studentId, CancellationToken cancellationToken)
        {
            string sql = @"SELECT StudentID, StudentName, StudentAddress, ClassID, StudentNumberOfCourses, StudentSumOfAllCharacters, StudentType 
                           FROM dbo.Student WHERE StudentID = @StudentID;";
            Dictionary<string, object> p = new Dictionary<string, object> { { "@StudentID", studentId } };
            IReadOnlyList<Student> rows = await _db.QueryAsync<Student>(sql, p, cancellationToken).ConfigureAwait(false);
            return rows.FirstOrDefault();
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

        public Task<int> RemoveEnrollmentAsync(int studentId, int classId, CancellationToken cancellationToken)
        {
            string sql = @"DELETE FROM dbo.Student_Class_Collection WHERE StudentID = @StudentID AND ClassID = @ClassID;";
            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@StudentID", studentId },
                { "@ClassID", classId }
            };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT StudentID, StudentName, StudentAddress, ClassID, StudentNumberOfCourses, StudentSumOfAllCharacters, StudentType FROM dbo.Student";
            return _db.QueryAsync<Student>(sql, null, cancellationToken);
        }

        public Task<int> DeleteStudentCascadeAsync(int studentId, CancellationToken cancellationToken)
        {
            var p = new Dictionary<string, object> { { "@StudentID", studentId } };

            var statements = new (string Sql, IDictionary<string, object> Parameters)[]
            {
                ("DELETE FROM dbo.StudentClass_RepetitionOnClass WHERE StudentID = @StudentID", p),
                ("DELETE FROM dbo.Student_Class_Collection WHERE StudentID = @StudentID", p),
                ("DELETE FROM dbo.Student WHERE StudentID = @StudentID", p)
            };

            return _db.ExecuteBatchInTransactionAsync(statements, cancellationToken);
        }


        public Task<IReadOnlyList<Class>> GetClassesAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT ClassID, ClassName, ClassDescription FROM dbo.Class";
            return _db.QueryAsync<Class>(sql, null, cancellationToken);
        }

        public Task<int> AddClassAsync(string name, string description, CancellationToken cancellationToken)
        {
            string sql = @"INSERT INTO dbo.Class (ClassName, ClassDescription) VALUES (@Name, @Desc);";
            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@Name", name ?? string.Empty },
                { "@Desc", description ?? string.Empty }
            };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<int> UpdateClassAsync(int classId, string name, string description, CancellationToken cancellationToken)
        {
            string sql = @"UPDATE dbo.Class SET ClassName = @Name, ClassDescription = @Desc WHERE ClassID = @ClassID;";
            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@ClassID", classId },
                { "@Name", name ?? string.Empty },
                { "@Desc", description ?? string.Empty }
            };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<int> DeleteClassCascadeAsync(int classId, CancellationToken cancellationToken)
        {
            var p = new Dictionary<string, object> { { "@ClassID", classId } };

            var statements = new (string Sql, IDictionary<string, object> Parameters)[]
            {
                // Slet grades for alle students i klassen
                (@"DELETE FROM dbo.StudentClass_RepetitionOnClass 
                   WHERE StudentID IN (SELECT StudentID FROM dbo.Student WHERE ClassID = @ClassID)", p),

                // Slet enrollment records for klassen
                ("DELETE FROM dbo.Student_Class_Collection WHERE ClassID = @ClassID", p),

                // Slet selve students i klassen
                ("DELETE FROM dbo.Student WHERE ClassID = @ClassID", p),

                // Slet klassen
                ("DELETE FROM dbo.Class WHERE ClassID = @ClassID", p)
            };

            return _db.ExecuteBatchInTransactionAsync(statements, cancellationToken);
        }


        public Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken)
        {
            string sql = "SELECT CourseID, CourseName FROM dbo.Course";
            return _db.QueryAsync<Course>(sql, null, cancellationToken);
        }

        public Task<int> AddCourseAsync(string courseName, CancellationToken cancellationToken)
        {
            string sql = "INSERT INTO dbo.Course (CourseName) VALUES (@Name);";
            Dictionary<string, object> p = new Dictionary<string, object> { { "@Name", courseName ?? string.Empty } };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<int> UpdateCourseAsync(int courseId, string courseName, CancellationToken cancellationToken)
        {
            string sql = "UPDATE dbo.Course SET CourseName = @Name WHERE CourseID = @CourseID;";
            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@CourseID", courseId },
                { "@Name", courseName ?? string.Empty }
            };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<int> DeleteCourseCascadeAsync(int courseId, CancellationToken cancellationToken)
        {
            var p = new Dictionary<string, object> { { "@CourseID", courseId } };

            var statements = new (string Sql, IDictionary<string, object> Parameters)[]
            {
                ("DELETE FROM dbo.StudentClass_RepetitionOnClass WHERE CourseID = @CourseID", p),
                ("DELETE FROM dbo.Course WHERE CourseID = @CourseID", p)
            };

            return _db.ExecuteBatchInTransactionAsync(statements, cancellationToken);
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

        public Task<IReadOnlyList<StudentClassRepetitionOnClass>> GetGradesByStudentAsync(int studentId, CancellationToken cancellationToken)
        {
            string sql = @"SELECT StudentClassID, StudentID, ClassID, Grade, CourseID 
                           FROM dbo.StudentClass_RepetitionOnClass
                           WHERE StudentID = @StudentID;";
            Dictionary<string, object> p = new Dictionary<string, object> { { "@StudentID", studentId } };
            return _db.QueryAsync<StudentClassRepetitionOnClass>(sql, p, cancellationToken);
        }

        public Task<int> UpdateGradeAsync(int studentClassId, int grade, CancellationToken cancellationToken)
        {
            string sql = @"UPDATE dbo.StudentClass_RepetitionOnClass SET Grade = @Grade WHERE StudentClassID = @Id;";
            Dictionary<string, object> p = new Dictionary<string, object>
            {
                { "@Id", studentClassId },
                { "@Grade", grade }
            };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        public Task<int> DeleteGradeAsync(int studentClassId, CancellationToken cancellationToken)
        {
            string sql = @"DELETE FROM dbo.StudentClass_RepetitionOnClass WHERE StudentClassID = @Id;";
            Dictionary<string, object> p = new Dictionary<string, object> { { "@Id", studentClassId } };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }


        public void Dispose()
        {
            _db.Dispose();
        }
    }
}