using System;
using System.Collections.Generic;
using System.Linq;

namespace MealPlannerApp
{
    public class MealPlanService
    {
        private const string WeeklyMealPlanFilePath = "weeklyMealPlan.txt";

        private readonly FileService _fileService;
        private readonly RecipeService _recipeService;

        public WeeklyMealPlan WeeklyMealPlan { get; private set; } = new();

        public MealPlanService(FileService fileService, RecipeService recipeService)
        {
            _fileService = fileService;
            _recipeService = recipeService;
        }

        public void LoadWeeklyMealPlan()
        {
            WeeklyMealPlan = new WeeklyMealPlan();

            if (!_fileService.Exists(WeeklyMealPlanFilePath))
            {
                SaveWeeklyMealPlan();
                return;
            }

            foreach (string line in _fileService.ReadAllLines(WeeklyMealPlanFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split('|');
                if (parts.Length < 2)
                {
                    continue;
                }

                string day = parts[0];
                List<Recipe> meals = ResolveRecipeNames(parts[1]);

                WeeklyMealPlan.SetMeals(day, meals);
            }
        }

        public void SaveWeeklyMealPlan()
        {
            IEnumerable<string> lines = WeeklyMealPlan.DaysOfWeek.Select(day =>
            {
                List<Recipe> meals = WeeklyMealPlan.GetMeals(day);
                string mealNames = string.Join(",", meals.Select(m => m.Name));
                return $"{day}|{mealNames}";
            });

            _fileService.WriteAllLines(WeeklyMealPlanFilePath, lines);
        }

        public void SetMeals(string day, List<Recipe> meals)
        {
            WeeklyMealPlan.SetMeals(day, meals);
        }

        public List<Recipe> GetMeals(string day)
        {
            return WeeklyMealPlan.GetMeals(day);
        }

        public void ReplaceRecipeNameInMealPlan(string oldName, string newName)
        {
            foreach (List<Recipe> dayMeals in WeeklyMealPlan.Meals.Values)
            {
                foreach (Recipe meal in dayMeals)
                {
                    if (meal.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                    {
                        meal.Name = newName;
                    }
                }
            }
        }

        public void RemoveRecipeFromMealPlan(string recipeName)
        {
            foreach (string day in WeeklyMealPlan.DaysOfWeek)
            {
                List<Recipe> filteredMeals = WeeklyMealPlan
                    .GetMeals(day)
                    .Where(meal => !meal.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                WeeklyMealPlan.SetMeals(day, filteredMeals);
            }
        }

        public Dictionary<string, double> GenerateGroceryList()
        {
            return WeeklyMealPlan.Meals.Values
                .SelectMany(meals => meals)
                .SelectMany(recipe => recipe.Ingredients)
                .GroupBy(ingredient => ingredient.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.First().Name,
                    group => group.Sum(i => i.Quantity));
        }

        private List<Recipe> ResolveRecipeNames(string recipeNamesRaw)
        {
            List<Recipe> meals = new();

            if (string.IsNullOrWhiteSpace(recipeNamesRaw))
            {
                return meals;
            }

            string[] recipeNames = recipeNamesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (string recipeName in recipeNames)
            {
                Recipe? recipe = _recipeService.FindByName(recipeName);

                if (recipe != null && meals.Count < 3)
                {
                    meals.Add(recipe);
                }
            }

            return meals;
        }
    }
}