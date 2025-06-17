using System.ComponentModel;
using ModelContextProtocol.Server;

namespace StudentsMcpServer.Models;

[McpServerToolType]
public class StudentTools(StudentService studentService) {
  [McpServerTool, Description("Get a list of students and return as JSON array")]
  public Task<string> GetStudentsJson() {
    return studentService.GetStudentsJson();
  }    
  
  [McpServerTool, Description("Get a list of students")]
  public Task<List<Student>> GetStudents() {
    return studentService.GetStudents();
  }

  [McpServerTool, Description("Get a student by name")]
  public Task<Student?> GetStudent([Description("The name of the student to get details for")] string name) {
    return studentService.GetStudentByFullName(name);
  }
  
  [McpServerTool, Description("Get a student by ID")]
  public Task<Student?> GetStudentById([Description("The ID of the student to get details for")] int id) {
    return studentService.GetStudentById(id);
  }
  
  [McpServerTool, Description("Get a student by ID and return as JSON")]
  public async Task<string?> GetStudentByIdJson([Description("The ID of the student to get details for")] int id){
    var student = await studentService.GetStudentById(id);
    if (student == null) {
      return null;
    }
    
    return System.Text.Json.JsonSerializer.Serialize(student, StudentContext.Default.Student);
  }
  
  [McpServerTool, Description("Get a student by name and return as JSON")]
  public async Task<string?> GetStudentJson([Description("The name of the student to get details for")] string name) {
    var student = await studentService.GetStudentByFullName(name);
    if (student == null) {
      return null;
    }
    
    return System.Text.Json.JsonSerializer.Serialize(student, StudentContext.Default.Student);
  }
  
  [McpServerTool, Description("Get students by school")]
  public async Task<List<Student>> GetStudentsBySchool([Description("The name of the school to filter students by")] string school) {
    var students = await studentService.GetStudentsBySchoolJson(school);
    return students;
  }

  [McpServerTool, Description("Get a student by Last Name")]
  public async Task<List<Student>> GetStudentsByLastName([Description("The last name of the student to filter by")] string lastName) {
    var students = await studentService.GetStudentsByLastName(lastName);
    return students;
  }

  [McpServerTool, Description("Get a student by First Name")]
  public async Task<List<Student>> GetStudentsByFirstName([Description("The first name of the student to filter by")] string firstName) {
    var students = await studentService.GetStudentsByFirstName(firstName);
    return students;
  }
}


