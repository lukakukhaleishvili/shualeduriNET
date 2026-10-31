using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonBook
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }

    [JsonIgnore]
    public decimal Price { get; set; }

    public JsonBook(string title, string author, int year, decimal price)
    {
        Title = title;
        Author = author;
        Year = year;
        Price = price;
    }
}

public static class Ex2
{
    public static class Demo
    {
        public static void Run()
        {
            List<JsonBook> books = new List<JsonBook>
            {
                new JsonBook("Clean Code", "Robert Martin", 2008, 45.50m),
                new JsonBook("The Pragmatic Programmer", "Andrew Hunt", 1999, 39.90m),
                new JsonBook("C# in Depth", "Jon Skeet", 2019, 55.00m)
            };

            string json = JsonSerializer.Serialize(
                books,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText("books.json", json);

            Console.WriteLine("Books saved to books.json");

            string readJson = File.ReadAllText("books.json");

            List<JsonBook> loadedBooks =
                JsonSerializer.Deserialize<List<JsonBook>>(readJson);

            Console.WriteLine("\nBooks loaded from JSON:");

            foreach (JsonBook book in loadedBooks)
            {
                Console.WriteLine(
                    $"Title: {book.Title}, Author: {book.Author}, " +
                    $"Year: {book.Year}, Price: {book.Price}");
            }

        }
    }
}