namespace TmsApi.Infrastructure.Caching;

public static class CacheKeys
{
    private const string SchemaVersion = "v2";
    
    // Course Keys
    public static string Course(string code) => $"{SchemaVersion}:course:{code}";
    public static string CoursesAll => $"{SchemaVersion}:courses:all";
    public const string CoursesTag = "courses";
    
    // Student Keys
    public static string Student(string id) => $"{SchemaVersion}:student:{id}";
    public static string StudentsAll => $"{SchemaVersion}:students:all";
    public const string StudentsTag = "students";
    
    // Certificate Keys
    public static string Certificate(string id) => $"{SchemaVersion}:certificate:{id}";
    public static string CertificatesAll => $"{SchemaVersion}:certificates:all";
    public const string CertificatesTag = "certificates";
    
    // Assessment Keys
    public static string Assessment(string id) => $"{SchemaVersion}:assessment:{id}";
    public static string AssessmentsAll => $"{SchemaVersion}:assessments:all";
    public const string AssessmentsTag = "assessments";
    
    // Enrollment Keys
    public static string Enrollment(int id) => $"{SchemaVersion}:enrollment:{id}";
    public static string EnrollmentsAll => $"{SchemaVersion}:enrollments:all";
    public const string EnrollmentsTag = "enrollments";
}

