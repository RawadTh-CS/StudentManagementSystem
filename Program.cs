using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace StudentManagementApp
{
    // 1. Model Class
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Grade { get; set; }
    }

    class Program
    {
        // 2. Connection String 
        // Iza 3andak Windows Authentication:
        private static string connectionString = @"Server=localhost;Database=SchoolDB;Trusted_Connection=True;TrustServerCertificate=True;";

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Student Management System ===");

            // --- A. CREATE (Insert) ---
            Console.WriteLine("\n[1] Inserting new student: 'Maya' (Grade: 88)...");
            InsertStudent("Maya", 88.0m);

            // --- B. READ ASYNC (ADO.NET + Async) ---
            Console.WriteLine("\n[2] Fetching all students from DB asynchronously...");
            List<Student> students = await GetAllStudentsAsync();

            Console.WriteLine($"Total students loaded: {students.Count}");
            foreach (var s in students)
            {
                Console.WriteLine($"ID: {s.Id} | Name: {s.Name} | Grade: {s.Grade}");
            }

            // --- C. UPDATE ---
            Console.WriteLine("\n[3] Updating Maya's grade to 95.0...");
            UpdateStudentGrade("Maya", 95.0m);

            // --- D. DELETE ---
            Console.WriteLine("\n[4] Deleting student 'Omar'...");
            DeleteStudentByName("Omar");

            // --- E. LINQ PROCESSING ---
            Console.WriteLine("\n[5] Processing Data with LINQ (Students with Grade >= 80, Ordered by Grade Descending)...");

            // Re-fetch to get latest state from DB
            List<Student> updatedStudents = await GetAllStudentsAsync();

            // LINQ: Where (filter) + OrderByDescending (sort)
            var topStudents = updatedStudents
                .Where(s => s.Grade >= 80)
                .OrderByDescending(s => s.Grade)
                .ToList();

            foreach (var student in topStudents)
            {
                Console.WriteLine($"[Honor Roll] {student.Name} - {student.Grade}");
            }

            Console.WriteLine("\n=== Done! Press any key to exit ===");
            Console.ReadKey();
        }

        // ================= CRUD METHODS (ADO.NET) =================

        // CREATE
        public static void InsertStudent(string name, decimal grade)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Students (Name, Grade) VALUES (@name, @grade)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@grade", grade);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // READ (ASYNC)
        public static async Task<List<Student>> GetAllStudentsAsync()
        {
            var studentList = new List<Student>();

            using (var connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Name, Grade FROM Students";
                using (var command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync(); // Async connection open
                    using (var reader = await command.ExecuteReaderAsync()) // Async reader
                    {
                        while (await reader.ReadAsync())
                        {
                            studentList.Add(new Student
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Grade = reader.GetDecimal(2)
                            });
                        }
                    }
                }
            }

            return studentList;
        }

        // UPDATE
        public static void UpdateStudentGrade(string name, decimal newGrade)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Students SET Grade = @newGrade WHERE Name = @name";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newGrade", newGrade);
                    command.Parameters.AddWithValue("@name", name);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // DELETE
        public static void DeleteStudentByName(string name)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Students WHERE Name = @name";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}