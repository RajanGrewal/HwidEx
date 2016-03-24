namespace HwidEx
{
    public static class Program
    {
        static void Main(string[] args)
        {
            string connection = "server=localhost;userid=root;password=;database=vip_trainer";

            if (args.Length > 0)
                connection = args[0];

            using (Server.Instance = new Server(7575, connection))
            {
                Server.Instance.Run();
            }
        }
    }
}
