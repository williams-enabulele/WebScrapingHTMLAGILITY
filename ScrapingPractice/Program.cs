using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CsvHelper;
using HtmlAgilityPack;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ScrapingPractice
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://books.toscrape.com/catalogue/category/books/classics_6/index.html";
            
            var links = GetBookLinks(url);
            var books =  GetBooks(links);
            ExportCSV(books);
            ExportJSON(books);

        }

        // Create an instance of an internal browser
        static HtmlDocument GetDocument(string url)
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }


        static List<string> GetBookLinks(string url)
        {
            var doc = GetDocument(url);
            var linkNodes = doc.DocumentNode.SelectNodes("//h3/a");
            var baseUri = new Uri(url);
            var links = new List<string>();
            foreach(var node in linkNodes)
            {
                var link = node.Attributes["href"].Value;
                link = new Uri(baseUri, link).AbsoluteUri;
                links.Add(link);

            }
            return links;
        }

        private static List<Book> GetBooks(List<string> links)
        {
            var books = new List<Book>();
            foreach (var link in links)
            {
                var book = new Book();
                var doc = GetDocument(link);
                var xpath = "//*[@class=\"col-sm-6 product_main\"]/*[@class=\"price_color\"]";
                book.Title = doc.DocumentNode.SelectSingleNode("//h1").InnerText;
                var price_raw = doc.DocumentNode.SelectSingleNode(xpath).InnerText;
                book.Price = ExtractPrice(price_raw);
                books.Add(book);

            }
            return books;  
        }

        static double ExtractPrice(string raw)
        {
            var reg = new Regex(@"[\d\.,]+", RegexOptions.Compiled);
            var m = reg.Match(raw);
            if (!m.Success)
            {
                return 0;
            }
            return Convert.ToDouble(m.Value);
        }

        private static void ExportCSV(List<Book> books)
        {
            var enviroment = System.Environment.CurrentDirectory;
            string basePath = Directory.GetParent(enviroment).Parent.FullName;
            var path = basePath.Substring(0, basePath.Length - 4);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Title, Price");
            foreach (var item in books)
            {
                sb.AppendLine(item.ToString());
            }
            File.WriteAllText($"{path}/books.csv", sb.ToString());
        }

        private static void ExportJSON(List<Book> books)
        {


            var enviroment = System.Environment.CurrentDirectory;
            string basePath = Directory.GetParent(enviroment).Parent.FullName;
            var path = basePath.Substring(0, basePath.Length - 4);
            string json = JsonConvert.SerializeObject(books, Formatting.Indented);
            File.WriteAllText($"{path}/books.json", json);

            /*var enviroment = System.Environment.CurrentDirectory;
            string basePath = Directory.GetParent(enviroment).Parent.FullName;
            var path = basePath.Substring(0, basePath.Length - 4);
            string json = JsonSerializer.Serialize(books);
            File.WriteAllText($"{path}/books.json", json);*/
        }
    }
}
 