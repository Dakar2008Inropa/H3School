using OwnORM.Data;
using OwnORM.Models;
using OwnORM.Models.Views;
using System.Reflection;
using System.Globalization;

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

        private sealed class EntityMap
        {
            public string Table { get; set; }
            public string Key { get; set; }
            public HashSet<string> IgnoredOnUpdate { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Dictionary<Type, EntityMap> _maps = new Dictionary<Type, EntityMap>
        {
            { typeof(Student), new EntityMap {
                    Table = "dbo.Student",
                    Key = "StudentID",
                    IgnoredOnUpdate = { "StudentNumberOfCourses", "StudentSumOfAllCharacters" }
                }
            },
            { typeof(Class), new EntityMap { Table = "dbo.Class", Key = "ClassID" } },
            { typeof(Course), new EntityMap { Table = "dbo.Course", Key = "CourseID" } },
            { typeof(StudentClassRepetitionOnClass), new EntityMap { Table = "dbo.StudentClass_RepetitionOnClass", Key = "StudentClassID" } }
        };

        private static EntityMap GetMap(Type t)
        {
            if (_maps.TryGetValue(t, out EntityMap map))
                return map;

            string typeName = t.Name;
            return new EntityMap
            {
                Table = $"dbo.{typeName}",
                Key = $"{typeName}ID"
            };
        }

        public Task<IReadOnlyList<T>> GetAllAsync<T>(CancellationToken cancellationToken) where T : new()
        {
            EntityMap map = GetMap(typeof(T));
            string sql = $"SELECT * FROM {map.Table}";
            return _db.QueryAsync<T>(sql, null, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken) where T : new()
        {
            EntityMap map = GetMap(typeof(T));
            string sql = $"SELECT TOP (1) * FROM {map.Table} WHERE {map.Key} = @Id;";
            Dictionary<string, object> p = new Dictionary<string, object> { { "@Id", id } };
            IReadOnlyList<T> rows = await _db.QueryAsync<T>(sql, p, cancellationToken).ConfigureAwait(false);
            return rows.FirstOrDefault();
        }

        public Task<int> InsertAsync<T>(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            EntityMap map = GetMap(typeof(T));
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();

            List<PropertyInfo> insertProps = props
                .Where(p => !string.Equals(p.Name, map.Key, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (insertProps.Count == 0)
                throw new InvalidOperationException("No insertable properties found.");

            string[] cols = insertProps.Select(p => p.Name).ToArray();
            string[] paramNames = insertProps.Select(p => "@" + p.Name).ToArray();

            string sql = $"INSERT INTO {map.Table} ({string.Join(", ", cols)}) VALUES ({string.Join(", ", paramNames)});";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            foreach (PropertyInfo p in insertProps)
            {
                object raw = p.GetValue(entity);
                object val = NormalizeParameterValue(p.PropertyType, raw);
                parameters["@" + p.Name] = val;
            }

            return _db.ExecuteAsync(sql, parameters, cancellationToken);
        }

        public Task<int> UpdateAsync<T>(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            EntityMap map = GetMap(typeof(T));

            PropertyInfo keyProp = typeof(T).GetProperty(map.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (keyProp == null)
                throw new InvalidOperationException($"Key property '{map.Key}' not found on type '{typeof(T).Name}'.");

            object keyValue = keyProp.GetValue(entity);
            if (keyValue == null)
                throw new InvalidOperationException("Key value must not be null.");

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();

            List<PropertyInfo> updateProps = props
                .Where(p =>
                    !string.Equals(p.Name, map.Key, StringComparison.OrdinalIgnoreCase) &&
                    !map.IgnoredOnUpdate.Contains(p.Name))
                .ToList();

            if (updateProps.Count == 0)
                throw new InvalidOperationException("No updatable properties found.");

            string setClause = string.Join(", ", updateProps.Select(p => $"{p.Name} = @{p.Name}"));
            string sql = $"UPDATE {map.Table} SET {setClause} WHERE {map.Key} = @__Key;";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@__Key", NormalizeParameterValue(keyProp.PropertyType, keyValue) }
            };

            foreach (PropertyInfo p in updateProps)
            {
                object raw = p.GetValue(entity);
                object val = NormalizeParameterValue(p.PropertyType, raw);
                parameters["@" + p.Name] = val;
            }

            return _db.ExecuteAsync(sql, parameters, cancellationToken);
        }

        public Task<int> DeleteByIdAsync<T>(int id, CancellationToken cancellationToken)
        {
            EntityMap map = GetMap(typeof(T));
            string sql = $"DELETE FROM {map.Table} WHERE {map.Key} = @Id;";
            Dictionary<string, object> p = new Dictionary<string, object> { { "@Id", id } };
            return _db.ExecuteAsync(sql, p, cancellationToken);
        }

        private static object NormalizeParameterValue(Type propertyType, object raw)
        {
            if (raw == null)
            {
                if (propertyType == typeof(string))
                    return string.Empty;

                return DBNull.Value;
            }

            Type underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (underlying.IsEnum)
                return Convert.ToInt32(raw, CultureInfo.InvariantCulture);

            return raw;
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

        public Task<int> DeleteClassCascadeAsync(int classId, CancellationToken cancellationToken)
        {
            var p = new Dictionary<string, object> { { "@ClassID", classId } };

            var statements = new (string Sql, IDictionary<string, object> Parameters)[]
            {
                (@"DELETE FROM dbo.StudentClass_RepetitionOnClass 
                   WHERE StudentID IN (SELECT StudentID FROM dbo.Student WHERE ClassID = @ClassID)", p),

                ("DELETE FROM dbo.Student_Class_Collection WHERE ClassID = @ClassID", p),

                ("DELETE FROM dbo.Student WHERE ClassID = @ClassID", p),

                ("DELETE FROM dbo.Class WHERE ClassID = @ClassID", p)
            };

            return _db.ExecuteBatchInTransactionAsync(statements, cancellationToken);
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