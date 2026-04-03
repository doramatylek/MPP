using System;
using System.Threading.Tasks;

namespace MyProject.Services
{
    public class DataProcessor
    {
        private readonly IStorage _storage;
        private readonly ILogger _logger;

        public DataProcessor(IStorage storage, ILogger logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public void ProcessData(string input)
        {
            _logger.Log("Processing...");
            _storage.Save(input);
        }

        public int GetDataCount() => 42;
    }

    public class HelperUtils
    {
        public string FormatName(string name)
        {
            return name.Trim().ToUpper();
        }

        public bool Validate(int value)
        {
            return value > 0;
        }
    }

    public interface IStorage { void Save(string data); }
    public interface ILogger { void Log(string message); }
}

namespace MyProject.Additional
{
    public class AnalyticsService
    {
        public double CalculateMean(int[] numbers)
        {
            return 0.0;
        }
    }
}