using System.Net.Http.Json;

namespace StudentsMcpServer.Models;
public class StudentService {
  readonly HttpClient _httpClient = new();
  private List<Student>? _studentsCache = null;
  private DateTime _cacheTime;
  private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10); // adjust as needed

  private async Task<List<Student>> FetchStudentsFromApi() {
    try {
      var response = await _httpClient.GetAsync("https://apipool.azurewebsites.net/api/students");
      if (response.IsSuccessStatusCode) {
        var studentsFromApi = await response.Content.ReadFromJsonAsync<List<Student>>(StudentContext.Default.ListStudent);
        return studentsFromApi ?? [];
        }
    } catch (Exception ex) {
      await Console.Error.WriteLineAsync($"Error fetching students from API: {ex.Message}");
    }
    return [];
  }

  public async Task<List<Student>> GetStudents() {
    if (_studentsCache == null || DateTime.UtcNow - _cacheTime > _cacheDuration) {
      _studentsCache = await FetchStudentsFromApi();
      _cacheTime = DateTime.UtcNow;
    }
    return _studentsCache;
  }

  public async Task<Student?> GetStudentByFullName(string name) {
    var students = await GetStudents();

    var nameParts = name.Split(' ', 2);
    if (nameParts.Length != 2) {
      Console.WriteLine("Name does not contain two parts");
      return null;
    }

    var firstName = nameParts[0].Trim();
    var lastName = nameParts[1].Trim();

    foreach (var s in students.Where(s => s.FirstName?.Contains(firstName, StringComparison.OrdinalIgnoreCase) == true)) {
      Console.WriteLine($"Found partial first name match: '{s.FirstName}' '{s.LastName}'");
    }

    var student = students.FirstOrDefault(m => {
      var firstNameMatch = string.Equals(m.FirstName, firstName, StringComparison.OrdinalIgnoreCase);
      var lastNameMatch = string.Equals(m.LastName, lastName, StringComparison.OrdinalIgnoreCase);
      return firstNameMatch && lastNameMatch;
    });

    return student;
  }

  public async Task<Student?> GetStudentById(int id) {
    var students = await GetStudents();
    var student = students.FirstOrDefault(s => s.StudentId == id);

    Console.WriteLine(student == null ? $"No student found with ID {id}" : $"Found student: {student}");
    return student;
  }

  public async Task<List<Student>> GetStudentsBySchoolJson(string school) {
    var students = await GetStudents();
    var filteredStudents = students.Where(s => s.School?.Equals(school, StringComparison.OrdinalIgnoreCase) == true).ToList();

    Console.WriteLine(filteredStudents.Count == 0
        ? $"No students found for school: {school}"
        : $"Found {filteredStudents.Count} students for school: {school}");

    return filteredStudents;
  }

  public async Task<List<Student>> GetStudentsByLastName(string lastName) {
    var students = await GetStudents();
    var filteredStudents = students.Where(s => s.LastName?.Equals(lastName, StringComparison.OrdinalIgnoreCase) == true).ToList();

    Console.WriteLine(filteredStudents.Count == 0
      ? $"No students found with last name: {lastName}"
      : $"Found {filteredStudents.Count} students with last name: {lastName}");

    return filteredStudents;
  }

  public async Task<List<Student>> GetStudentsByFirstName(string firstName) {
    var students = await GetStudents();
    var filteredStudents = students.Where(s => s.FirstName?.Equals(firstName, StringComparison.OrdinalIgnoreCase) == true).ToList();

    Console.WriteLine(filteredStudents.Count == 0
      ? $"No students found with first name: {firstName}"
      : $"Found {filteredStudents.Count} students with first name: {firstName}");

    return filteredStudents;
  }

  public async Task<string> GetStudentsJson() {
    var students = await GetStudents();
    return System.Text.Json.JsonSerializer.Serialize(students);
  }
}


