using System;
using System.Collections.Generic;
using System.Linq;

namespace MealPlannerApp
{
    public class DailyFoodEntryService
    {
        private const string DailyFoodEntriesFilePath = "dailyFoodEntries.txt";
        private readonly FileService _fileService;

        public List<FoodEntry> FoodEntries { get; } = new();

        public DailyFoodEntryService(FileService fileService)
        {
            _fileService = fileService;
        }

        public void LoadFoodEntries()
        {
            FoodEntries.Clear();

            if (!_fileService.Exists(DailyFoodEntriesFilePath))
            {
                _fileService.WriteAllText(DailyFoodEntriesFilePath, string.Empty);
                return;
            }

            foreach (string line in _fileService.ReadAllLines(DailyFoodEntriesFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                FoodEntry? entry = ParseFoodEntry(line);
                if (entry != null)
                {
                    FoodEntries.Add(entry);
                }
            }
        }

        public void SaveFoodEntries()
        {
            IEnumerable<string> lines = FoodEntries.Select(entry =>
                $"{entry.Day}|{entry.Name}|{entry.Calories}|{entry.Protein}|{entry.Carbs}|{entry.Fat}");

            _fileService.WriteAllLines(DailyFoodEntriesFilePath, lines);
        }

        public void AddFoodEntry(FoodEntry entry)
        {
            FoodEntries.Add(entry);
            SaveFoodEntries();
        }

        public void UpdateFoodEntry(int index, FoodEntry updatedEntry)
        {
            if (index < 0 || index >= FoodEntries.Count)
            {
                return;
            }

            FoodEntries[index] = updatedEntry;
            SaveFoodEntries();
        }

        public void DeleteFoodEntry(int index)
        {
            if (index < 0 || index >= FoodEntries.Count)
            {
                return;
            }

            FoodEntries.RemoveAt(index);
            SaveFoodEntries();
        }

        public List<FoodEntry> GetEntriesForDay(string day)
        {
            return FoodEntries
                .Where(entry => entry.Day.Equals(day, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private FoodEntry? ParseFoodEntry(string line)
        {
            string[] parts = line.Split('|');

            if (parts.Length < 6)
            {
                return null;
            }

            if (!int.TryParse(parts[2], out int calories) ||
                !int.TryParse(parts[3], out int protein) ||
                !int.TryParse(parts[4], out int carbs) ||
                !int.TryParse(parts[5], out int fat))
            {
                return null;
            }

            return new FoodEntry(parts[0], parts[1], calories, protein, carbs, fat);
        }
    }
}