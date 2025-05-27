using System.Net;
using System.Net.Sockets;
using System.Text;



class Program
{
    static async Task Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        using UdpClient udpClient = new UdpClient();

        string ip = "127.0.0.1";
        int port = 1028;
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);

        while (true)
        {
            Console.Write("Вкажіть повідомлення: ");
            string message = Console.ReadLine();

            byte[] bytes = Encoding.UTF8.GetBytes(message);

            // Відправляємо повідомлення
            await udpClient.SendAsync(bytes, bytes.Length, remoteEP);

            // Отримуємо відповідь
            UdpReceiveResult result = await udpClient.ReceiveAsync();
            string response = Encoding.UTF8.GetString(result.Buffer);

            Console.WriteLine($"Отримано відповідь: {response}");
        }
    }
}