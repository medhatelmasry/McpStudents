using System.ComponentModel;
using ModelContextProtocol.Server;

namespace StudentsMcpServer.Models;

[McpServerToolType]
public static class StudentTools
{
  private static readonly StudentService _studentService = new StudentService();

  [McpServerTool, Description("Get a list of students and return as JSON array")]
  public static string GetStudentsJson()
  {
    var task = _studentService.GetStudentsJson();
    return task.GetAwaiter().GetResult();
  }

  [McpServerTool, Description("Get a student by name and return as JSON")]
  public static string GetStudentJson([Description("The name of the student to get details for")] string name)
  {
    var task = _studentService.GetStudentByFullName(name);
    var student = task.GetAwaiter().GetResult();
    if (student == null)
    {
      return "Student not found";
    }

    return System.Text.Json.JsonSerializer.Serialize(student, StudentContext.Default.Student);
  }

  [McpServerTool, Description("Get a student by ID and return as JSON")]
  public static string GetStudentByIdJson([Description("The ID of the student to get details for")] int id)
  {
    var task = _studentService.GetStudentById(id);
    var student = task.GetAwaiter().GetResult();
    if (student == null)
    {
      return "Student not found";
    }

    return System.Text.Json.JsonSerializer.Serialize(student, StudentContext.Default.Student);
  }

  [McpServerTool, Description("Get students by school and return as JSON")]
  public static string GetStudentsBySchoolJson([Description("The name of the school to filter students by")] string school)
  {
    var task = _studentService.GetStudentsBySchoolJson(school);
    var students = task.GetAwaiter().GetResult();
    return System.Text.Json.JsonSerializer.Serialize(students, StudentContext.Default.ListStudent);
  }

  [McpServerTool, Description("Get students by last name and return as JSON")]
  public static string GetStudentsByLastNameJson([Description("The last name of the student to filter by")] string lastName)
  {
    var task = _studentService.GetStudentsByLastName(lastName);
    var students = task.GetAwaiter().GetResult();
    return System.Text.Json.JsonSerializer.Serialize(students, StudentContext.Default.ListStudent);
  }

  [McpServerTool, Description("Get students by first name and return as JSON")]
  public static string GetStudentsByFirstNameJson([Description("The first name of the student to filter by")] string firstName)
  {
    var task = _studentService.GetStudentsByFirstName(firstName);
    var students = task.GetAwaiter().GetResult();
    return System.Text.Json.JsonSerializer.Serialize(students, StudentContext.Default.ListStudent);
  }

  [McpServerTool, Description("Get count of total students")]
  public static int GetStudentCount()
  {
    var task = _studentService.GetStudents();
    var students = task.GetAwaiter().GetResult();
    return students.Count;
  }
}


