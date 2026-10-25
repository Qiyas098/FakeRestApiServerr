using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ClientFakeApiTest
{
    public class FakeHttpClient
    {
        private readonly string _host;
        private readonly int _port;

        public FakeHttpClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task<FakeHttpResponse> SendAsync(string method, string path, object? body = null)
        {
            using var client = new TcpClient();

            await client.ConnectAsync(IPAddress.Parse(_host), _port);

            await using var stream = client.GetStream();

            var requestText = $"{method} {path}";

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body);
                requestText += "\n" + json;
            }

            var requestBytes = Encoding.UTF8.GetBytes(requestText);

            Console.WriteLine("\n---- RAW TCP REQUEST ----");
            Console.WriteLine(requestText);

            await stream.WriteAsync(requestBytes);

            var buffer = new byte[8192];
            var count = await stream.ReadAsync(buffer);

            var responseText = Encoding.UTF8.GetString(buffer, 0, count);

            return new FakeHttpResponse
            {
                Raw = responseText
            };
        }
    }

    public class FakeHttpResponse
    {
        public string Raw { get; set; } = "";
    }

    public class Program
    {
        static FakeHttpClient client = new FakeHttpClient("192.168.100.248", 27001);
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("MINI Fake Postman Client");
                Console.WriteLine("1. GET /products");
                Console.WriteLine("2. GET /products/{id}");
                Console.WriteLine("3. POST /products");
                Console.WriteLine("4. PUT /products/{id}");
                Console.WriteLine("5. DELETE /products/{id}");
                Console.WriteLine("0. Exit");
                Console.Write("Choose: ");
                var choice = Console.ReadLine();

                if (choice == "0")
                {
                    break;
                }
                try
                {
                    switch (choice)
                    {
                        case "1":
                            {
                                await SendAsync("GET", "/products");
                                break;
                            }
                        case "2":
                            {
                                Console.Write("Product ID: ");
                                var id = Console.ReadLine();

                                await SendAsync("GET", $"/products/{id}");
                                break;
                            }
                        case "3":
                            {
                                Console.Write("Enter name: ");
                                string name = Console.ReadLine() ?? "";
                                Console.Write("Enter price: ");
                                decimal price = decimal.Parse(Console.ReadLine() ?? "0");
                                await SendAsync("POST", "/products", new { name, price });
                                break;
                            }
                        case "4":
                            {
                                Console.Write("Product ID: ");
                                var id = Console.ReadLine();
                                Console.Write("Enter new name: ");
                                string name = Console.ReadLine() ?? "";
                                Console.Write("Enter new price: ");
                                decimal price = decimal.Parse(Console.ReadLine() ?? "0");
                                await SendAsync("PUT", $"/products/{id}", new { name, price });
                                break;
                            }
                        case "5":
                            {
                                Console.Write("Product ID: ");
                                var id = Console.ReadLine();
                                await SendAsync("DELETE", $"/products/{id}");
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Yanlis secim!");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private static async Task SendAsync(string method, string path, object? body = null)
        {
            var response = await client.SendAsync(method, path, body);
            Console.WriteLine();
            Console.WriteLine("------ RESPONSE RAW -------");
            Console.WriteLine(response.Raw);
        }
    }
}
