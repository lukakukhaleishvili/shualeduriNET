using System;
using System.Collections.Generic;
using System.Linq;

public record Grade(string Course, int Score);

public class Gradebook
{
    private readonly Dictionary<string, List<Grade>> students =
        new Dictionary<string, List<Grade>>(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> courses =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".NET", "DB", "Math"
        };

    private readonly Stack<(string Name, Grade Grade)> undoStack = new();

    private readonly Queue<string> history = new();

    public string AddScore(string name, string course, int score)
    {
        if (!courses.Contains(course))
            return $"Unknown course: {course}";

        if (score < 0 || score > 100)
            return $"Invalid score: {score}";

        if (!students.ContainsKey(name))
            students[name] = new List<Grade>();

        string actualName = students.Keys.First(
            x => x.Equals(name, StringComparison.OrdinalIgnoreCase));

        Grade grade = new Grade(
            courses.First(x => x.Equals(course, StringComparison.OrdinalIgnoreCase)),
            score);

        students[actualName].Add(grade);
        undoStack.Push((actualName, grade));

        AddHistory($"add {actualName} {grade.Course} {grade.Score}");

        return $"Added: {actualName} {grade.Course} {grade.Score}";
    }

    public string Undo()
    {
        if (undoStack.Count == 0)
            return "Nothing to undo";

        var last = undoStack.Pop();

        students[last.Name].Remove(last.Grade);

        AddHistory($"undo {last.Name} {last.Grade.Course} {last.Grade.Score}");

        return $"Undone: {last.Name} {last.Grade.Course} {last.Grade.Score}";
    }

    private void AddHistory(string action)
    {
        history.Enqueue(action);

        if (history.Count > 5)
            history.Dequeue();
    }

    public void ShowTable()
    {
        Console.WriteLine(
            $"{"Name",-8}{"Count",6}{"Average",9}{"Max",5}  Last 3");

        foreach (var student in students)
        {
            List<Grade> grades = student.Value;

            if (grades.Count == 0)
                continue;

            double average = grades.Average(x => x.Score);
            int max = grades.Max(x => x.Score);

            var lastThree = grades.Count > 3
                ? grades.Skip(grades.Count - 3).ToList()
                : grades;

            string last = string.Join(" ",
                lastThree.Select(x => x.Score));

            Console.WriteLine(
                $"{student.Key,-8}{grades.Count,6}{average,9:F2}{max,5}  {last}");
        }
    }

    public void SearchStudent(string name)
    {
        var student = students.FirstOrDefault(
            x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (student.Value == null)
        {
            Console.WriteLine("not found");
            return;
        }

        var averages = student.Value
            .GroupBy(x => x.Course)
            .OrderBy(x => x.Key);

        Console.Write($"{student.Key}: ");

        Console.WriteLine(string.Join(", ",
            averages.Select(x =>
                $"{x.Key}={x.Average(g => g.Score):F2}")));
    }

    public void ShowTopThree()
    {
        Console.WriteLine("Top 3:");

        var top = students
            .Where(x => x.Value.Count > 0)
            .Select(x => new
            {
                Name = x.Key,
                Average = x.Value.Average(g => g.Score)
            })
            .OrderByDescending(x => x.Average)
            .ThenBy(x => x.Name)
            .Take(3);

        foreach (var student in top)
            Console.WriteLine($"  {student.Name,-8} {student.Average:F2}");
    }

    public void ShowCourseStatistics()
    {
        foreach (string course in courses.OrderBy(x => x))
        {
            var grades = students
                .SelectMany(x => x.Value.Select(g => new
                {
                    Student = x.Key,
                    g.Course,
                    g.Score
                }))
                .Where(x => x.Course == course)
                .ToList();

            if (grades.Count == 0)
                continue;

            var best = grades
                .GroupBy(x => x.Student)
                .Select(x => new
                {
                    Name = x.Key,
                    Average = x.Average(g => g.Score)
                })
                .OrderByDescending(x => x.Average)
                .ThenBy(x => x.Name)
                .First();

            Console.WriteLine(
                $"{course,-6} students: {grades.Select(x => x.Student).Distinct().Count()}, " +
                $"average: {grades.Average(x => x.Score):F2}, " +
                $"best: {best.Name} ({best.Average:F2})");
        }
    }

    public void ShowHistory()
    {
        Console.WriteLine("History:");

        foreach (string item in history)
            Console.WriteLine($"  {item}");
    }

    public void ShowDistribution()
    {
        SortedDictionary<char, int> distribution =
            new SortedDictionary<char, int>
            {
                ['A'] = 0,
                ['B'] = 0,
                ['C'] = 0,
                ['D'] = 0,
                ['E'] = 0,
                ['F'] = 0
            };

        foreach (var grade in students.Values.SelectMany(x => x))
        {
            char letter = grade.Score switch
            {
                >= 91 => 'A',
                >= 81 => 'B',
                >= 71 => 'C',
                >= 61 => 'D',
                >= 51 => 'E',
                _ => 'F'
            };

            distribution[letter]++;
        }

        Console.WriteLine(
            "Distribution: " +
            string.Join(", ", distribution.Select(x => $"{x.Key}={x.Value}")));
    }
}

public static class Ex3
{
    public static class Demo
    {
        public static void Run()
        {
            Gradebook gradebook = new Gradebook();

            gradebook.AddScore("Ana", ".NET", 95);
            gradebook.AddScore("Ana", "DB", 88);
            gradebook.AddScore("Ana", "Math", 100);
            gradebook.AddScore("Gio", ".NET", 72);
            gradebook.AddScore("Gio", "DB", 81);
            gradebook.AddScore("luka", ".NET", 65);
            gradebook.AddScore("Nino", "Math", 91);
            gradebook.AddScore("Nino", ".NET", 67);
            gradebook.AddScore("Nino", "DB", 79);
            gradebook.AddScore("Ana", ".NET", 98);

            bool running = true;

            while (running)
            {
                Console.WriteLine("\n1. Add score");
                Console.WriteLine("2. Show table");
                Console.WriteLine("3. Search student");
                Console.WriteLine("4. Top 3");
                Console.WriteLine("5. Undo");
                Console.WriteLine("6. Course statistics");
                Console.WriteLine("7. History");
                Console.WriteLine("0. Exit");

                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Name: ");
                        string name = Console.ReadLine();

                        Console.Write("Course: ");
                        string course = Console.ReadLine();

                        Console.Write("Score: ");
                        if (int.TryParse(Console.ReadLine(), out int score))
                        {
                            Console.WriteLine(
                                gradebook.AddScore(name, course, score));
                        }
                        else
                        {
                            Console.WriteLine("Invalid score");
                        }
                        break;

                    case "2":
                        gradebook.ShowTable();
                        break;

                    case "3":
                        Console.Write("Student name: ");
                        gradebook.SearchStudent(Console.ReadLine());
                        break;

                    case "4":
                        gradebook.ShowTopThree();
                        break;

                    case "5":
                        Console.WriteLine(gradebook.Undo());
                        break;

                    case "6":
                        gradebook.ShowCourseStatistics();
                        break;

                    case "7":
                        gradebook.ShowHistory();
                        break;

                    case "0":
                        gradebook.ShowDistribution();
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
            }
        }
    }
}