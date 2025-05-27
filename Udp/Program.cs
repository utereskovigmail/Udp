using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

Console.InputEncoding = Encoding.UTF8; // Встановлюємо кодування для консолі
Console.OutputEncoding = Encoding.UTF8; // Встановлюємо кодування для виводу в консоль



List<Product> products = new List<Product>()
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

var udpClient = new UdpClient(1028); //0 - 65535 - 2 байти
Console.WriteLine("UDP Server is running on port 1028...");


int limitPerMinute = 4;
Queue<DateTime> times = new Queue<DateTime>();

while(true)
{
    DateTime now = DateTime.UtcNow;
    
    //Очікуємо повідомлення від клієнта
    var result = await udpClient.ReceiveAsync();
    string text = Encoding.UTF8.GetString(result.Buffer); // отримуємо байти
    
    
    if (times.Count < limitPerMinute)
    {
        times.Enqueue(now);
    }
    else
    {
        times.Dequeue(); 

        TimeSpan diff = now - times.Peek();
        if (diff.TotalSeconds <= 60)
        {
            Console.WriteLine("Too many requests per minute!");
            
            byte[] resp = Encoding.UTF8.GetBytes("Too many requests per minute!");
            udpClient.Send(resp, result.RemoteEndPoint);
            udpClient.Close();
            break;
        }
        
        times.Enqueue(now);
    }
    
    if (text.Trim().StartsWith("ShowAll"))
    {
        var names = products.Select(p => $"{p.Company} {p.Component}");
        string message = string.Join("\n", names);
        udpClient.Send(Encoding.UTF8.GetBytes(message), result.RemoteEndPoint);
    }
    else
    {
        if (products.Find(p => p.Component == text) != null)
        {
            var p = products.Find(p => p.Component == text);
            string txt = p.Company + " " + p.Component + " --- " + p.Price;
            udpClient.Send(Encoding.UTF8.GetBytes(txt), result.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine("Incorrect message");
            udpClient.Send(Encoding.UTF8.GetBytes("Incorrect message"), result.RemoteEndPoint);
        }
    }
    // Console.WriteLine($"Отримали повідомлення: {text} від {result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port}");
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