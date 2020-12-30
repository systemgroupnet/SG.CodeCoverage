using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProjectForTest
{
    public class App
    {
        public IReadOnlyList<Command> Commands { get; }
        private bool _shouldExit = false;

        public App()
        {
            Commands = new List<Command>()
            {
                new Command( "exit", "exit", Exit),
                new Command("isprime", "IsPrime", IsPrime)
            };
        }

        public void Run(string[] args)
        {
            if (args.Length > 0)
                RunCommand(args);

            Console.WriteLine("Commands:");
            foreach (var cmd in Commands)
                Console.WriteLine("  " + cmd.Help);

            while (!_shouldExit)
            {
                Console.WriteLine("Enter command:");
                var line = Console.ReadLine();
                var parts = line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
                RunCommand(parts);
            }
        }

        private void RunCommand(string[] parts)
        {
            bool found = false;
            foreach (var cmd in Commands)
            {
                if (parts[0].Equals(cmd.CommandText, StringComparison.OrdinalIgnoreCase))
                {
                    cmd.Operation(parts.Length > 0 ? parts[1] : null);
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new Exception($"Command '{parts[0]}' not found.");
        }

        private void Exit(string _)
        {
            _shouldExit = true;
        }

        private void IsPrime(string input)
        {
            var number = int.Parse(input);
            var isPrime = new PrimeCalculator().IsPrime(number);
            Console.WriteLine($"{number} is {(isPrime ? "" : "not ")}prime.");
        }
    }

    public class Command
    {
        public Command(string commandText, string help, Action<string> operation)
        {
            CommandText = commandText;
            Help = help;
            Operation = operation;
        }

        public string CommandText { get; }
        public string Help { get; }
        public Action<string> Operation { get; }
    }
}
