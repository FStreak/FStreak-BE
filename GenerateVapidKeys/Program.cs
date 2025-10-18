using System;
using WebPush;

class Program
{
    static void Main(string[] args)
    {
        var vapidKeys = VapidHelper.GenerateVapidKeys();
        Console.WriteLine("VAPID Keys Generated:");
        Console.WriteLine($"Public Key: {vapidKeys.PublicKey}");
        Console.WriteLine($"Private Key: {vapidKeys.PrivateKey}");
    }
}