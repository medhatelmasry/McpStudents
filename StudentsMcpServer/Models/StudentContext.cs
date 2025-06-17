using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudentsMcpServer.Models;
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<Student>))]
[JsonSerializable(typeof(Student))]
internal sealed partial class StudentContext : JsonSerializerContext { }

