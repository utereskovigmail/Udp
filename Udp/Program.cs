using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Net;




class Program
{
    static List<Product> products = new List<Product>()
{
    new Product("Intel", "CPU", 250.99m),
    new Product("AMD", "GPU", 399.50m),
    new Product("Corsair", "RAM", 89.99m),
    new Product("Samsung", "SSD", 120.00m),
    new Product("Asus", "Motherboard", 180.75m),
    new Product("Seagate", "HDD", 70.20m),
    new Product("EVGA", "Power Supply", 110.49m),
    new Product("Logitech", "Keyboard", 45.00m),
    new Product("Razer", "Mouse", 55.00m),
    new Product("Dell", "Monitor", 230.00m),
    new Product("MSI", "Graphics Card", 499.99m),
    new Product("NZXT", "Case", 99.99m),
    new Product("Cooler Master", "CPU Cooler", 65.50m),
    new Product("Kingston", "RAM", 75.25m),
    new Product("Gigabyte", "Motherboard", 150.00m)
};

static int limitPerMinute = 4;
static Queue<DateTime> times = new Queue<DateTime>();
static ConcurrentDictionary<IPEndPoint, DateTime> clientLastMessageTime = new();

// Ліміт клієнтів
static int maxNumberOfClients = 2;
static async Task Main(string[] args)
{
    int port = 1028;
    using UdpClient udpClient = new UdpClient(port);
    Console.WriteLine($"UDP Server is running on port {port}...");

    int max_number_of_clients = 2;
    
    _ = Task.Run(() => CheckClientTimeoutsAsync(TimeSpan.FromSeconds(15), udpClient));
    
    
    while (true)
    {
        var result = await udpClient.ReceiveAsync();

        // Оновлюємо час останнього повідомлення цього клієнта
        clientLastMessageTime[result.RemoteEndPoint] = DateTime.UtcNow;

        if (clientLastMessageTime.Count > maxNumberOfClients)
        {
            string msg = "Too many clients connected.";
            Console.WriteLine(msg);
            await udpClient.SendAsync(Encoding.UTF8.GetBytes(msg), result.RemoteEndPoint);
            continue; // не обробляємо повідомлення далі
        }
        
        _ = Task.Run(() => ProcessMessageAsync(result.Buffer, result.RemoteEndPoint, udpClient));
    }
}





static async Task ProcessMessageAsync(byte[] data, IPEndPoint remoteEP, UdpClient udpClient)
{
    string text = Encoding.UTF8.GetString(data).Trim();

    if (text.StartsWith("ShowAll", StringComparison.OrdinalIgnoreCase))
    {
        var names = products.Select(p => $"{p.Company} {p.Component}");
        string message = string.Join("\n", names);
        await udpClient.SendAsync(Encoding.UTF8.GetBytes(message), remoteEP);
    }
    else
    {
        var product = products.Find(p => p.Component.Equals(text, StringComparison.OrdinalIgnoreCase));
        if (product != null)
        {
            string response = $"{product.Company} {product.Component} --- {product.Price}";
            await udpClient.SendAsync(Encoding.UTF8.GetBytes(response), remoteEP);
        }
        else
        {
            string response = "Incorrect message";
            await udpClient.SendAsync(Encoding.UTF8.GetBytes(response), remoteEP);
        }
    }
}

static async Task CheckClientTimeoutsAsync(TimeSpan timeout, UdpClient udpClient)
{
    while (true)
    {
        var now = DateTime.UtcNow;
        var timedOutClients = clientLastMessageTime.Where(kvp => now - kvp.Value > timeout).ToList();

        foreach (var kvp in timedOutClients)
        {
            Console.WriteLine($"Client {kvp.Key} timed out.");
            clientLastMessageTime.TryRemove(kvp.Key, out _);

            string msg = "Too long without messages";
            await udpClient.SendAsync(Encoding.UTF8.GetBytes(msg), kvp.Key);
        }

        await Task.Delay(1000); 
    }
}


}



class Product
{
    public string Company { get; set; }
    public string Component { get; set; }
    public decimal Price { get; set; }

    public Product(string company, string component, decimal price)
    {
        Company = company;
        Component = component;
        Price = price;
    }
}