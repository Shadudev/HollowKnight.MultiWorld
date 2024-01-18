using MultiWorldServer.Loggers;
using System;

namespace MultiWorldServer
{
    internal class Program
    {
        private static Server Server;

        private static void Main()
        {
            Config config = Config.Load();
            string logName = "ServerLog" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            LogWriter logWriter = LogWriterFactory.CreateLogger(logName, config.MaxGeneralLogSize);

            Server = new Server(logWriter, config);
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write("> ");

            while (Server.Running)
            {

                string input = Console.ReadLine();

                logWriter.Log($"> " + input);

                try
                {
                    string[] commands = input.Split(' ');
                    switch (commands[0])
                    {
                        case "give":
                            if (commands.Length != 4)
                            {
                                Console.WriteLine("Usage: give <item> <session id> <player id>");
                                break;
                            }
                            Server.GiveItem(commands[1], int.Parse(commands[2]), int.Parse(commands[3]));
                            break;
                        case "ready":
                            Server.ListReady();
                            break;
                        case "list":
                            Server.ListSessions();
                            break;
                        default:
                            Console.WriteLine($"Unrecognized command: '{commands[0]}'");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error processing command: {e.Message}");
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("> ");
            }
        }
    }
}
