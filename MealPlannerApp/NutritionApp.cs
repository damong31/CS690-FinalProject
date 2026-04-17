using System;
using System.Collections.Generic;
using System.Linq;

namespace MealPlannerApp
{
    public class NutritionApp
    {
        private readonly ProfileService _profileService;
        private readonly RecipeService _recipeService;
        private readonly MealPlanService _mealPlanService;

        private UserProfile _userProfile = null!;
        private readonly List<FoodEntry> _dailyFoodEntries = new();

        public NutritionApp()
        {
            var fileService = new FileService();
            _profileService = new ProfileService(fileService);
            _recipeService = new RecipeService(fileService);
            _mealPlanService = new MealPlanService(fileService, _recipeService);
        }

        public void Run()
        {
            _userProfile = _profileService.LoadOrCreateUserProfile();
            _recipeService.LoadRecipes();
            _mealPlanService.LoadWeeklyMealPlan();

            bool running = true;

            while (running)
            {
                DisplayMainMenu();
                string input = Console.ReadLine()?.Trim() ?? string.Empty;
                running = HandleMenuChoice(input);
            }
        }

        private void DisplayMainMenu()
        {
            Console.WriteLine($"\n---- Welcome, {_userProfile.Name}! ----");
            Console.WriteLine($"Current Day: {_userProfile.CurrentDay}");
            Console.WriteLine("1. View User Macro Goals");
            Console.WriteLine("2. Add Daily Food Entry");
            Console.WriteLine("3. View Daily Macro Summary");
            Console.WriteLine("4. View Recipes");
            Console.WriteLine("5. Add New Recipe");
            Console.WriteLine("6. Edit Recipe");
            Console.WriteLine("7. Delete Recipe");
            Console.WriteLine("8. Create Weekly Meal Plan");
            Console.WriteLine("9. Edit Weekly Meal Plan");
            Console.WriteLine("10. View Weekly Meal Plan");
            Console.WriteLine("11. Generate Grocery List");
            Console.WriteLine("12. Update Profile");
            Console.WriteLine("13. Set Current Day");
            Console.WriteLine("14. View Current Day Meal Plan");
            Console.WriteLine("15. Exit");
            Console.Write("Choose an option: ");
        }

        private bool HandleMenuChoice(string input)
        {
            switch (input)
            {
                case "1":
                    ViewUserGoals();
                    break;
                case "2":
                    AddDailyFoodEntry();
                    break;
                case "3":
                    ViewDailyMacroSummary();
                    break;
                case "4":
                    ViewRecipes();
                    break;
                case "5":
                    AddRecipe();
                    break;
                case "6":
                    EditRecipe();
                    break;
                case "7":
                    DeleteRecipe();
                    break;
                case "8":
                    CreateWeeklyMealPlan();
                    break;
                case "9":
                    EditWeeklyMealPlan();
                    break;
                case "10":
                    ViewWeeklyMealPlan();
                    break;
                case "11":
                    GenerateGroceryList();
                    break;
                case "12":
                    UpdateUserProfile();
                    break;
                case "13":
                    SetCurrentDay();
                    break;
                case "14":
                    ViewCurrentDayMealPlan();
                    break;
                case "15":
                    Console.WriteLine("Goodbye!");
                    return false;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }

            return true;
        }

        private void ViewUserGoals()
        {
            Console.WriteLine("\n---- User Goals ----");
            Console.WriteLine($"Name: {_userProfile.Name}");
            Console.WriteLine($"Current Day: {_userProfile.CurrentDay}");
            Console.WriteLine($"Calories: {_userProfile.CalorieGoal}");
            Console.WriteLine($"Protein: {_userProfile.ProteinGoal}");
            Console.WriteLine($"Carbs: {_userProfile.CarbGoal}");
            Console.WriteLine($"Fats: {_userProfile.FatGoal}");
        }

        private void AddDailyFoodEntry()
        {
            Console.WriteLine("\n---- Add Daily Food Entry ----");

            string name = ReadRequiredText("Food name: ", "Unknown Food");
            int calories = ReadNonNegativeInt("Calories: ");
            int protein = ReadNonNegativeInt("Protein: ");
            int carbs = ReadNonNegativeInt("Carbs: ");
            int fat = ReadNonNegativeInt("Fat: ");

            _dailyFoodEntries.Add(new FoodEntry(name, calories, protein, carbs, fat));
            Console.WriteLine("Food entry added.");
        }

        private void ViewDailyMacroSummary()
        {
            Console.WriteLine("\n---- Daily Macro Summary ----");

            if (!_dailyFoodEntries.Any())
            {
                Console.WriteLine("No food entries logged.");
                return;
            }

            foreach (FoodEntry entry in _dailyFoodEntries)
            {
                Console.WriteLine($"- {entry.Name}: {entry.Calories} Calories, Protein:{entry.Protein} Carbs:{entry.Carbs} Fats:{entry.Fat}");
            }

            int totalCalories = _dailyFoodEntries.Sum(entry => entry.Calories);
            int totalProtein = _dailyFoodEntries.Sum(entry => entry.Protein);
            int totalCarbs = _dailyFoodEntries.Sum(entry => entry.Carbs);
            int totalFat = _dailyFoodEntries.Sum(entry => entry.Fat);

            Console.WriteLine($"\nCalories: {totalCalories}/{_userProfile.CalorieGoal}");
            Console.WriteLine($"Protein: {totalProtein}/{_userProfile.ProteinGoal}");
            Console.WriteLine($"Carbs: {totalCarbs}/{_userProfile.CarbGoal}");
            Console.WriteLine($"Fats: {totalFat}/{_userProfile.FatGoal}");
        }

        private void ViewRecipes()
        {
            Console.WriteLine("\n---- Recipes ----");

            if (!_recipeService.Recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            for (int i = 0; i < _recipeService.Recipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {FormatRecipe(_recipeService.Recipes[i])}");
            }
        }

        private void AddRecipe()
        {
            Console.WriteLine("\n---- Add New Recipe ----");

            string name = ReadRequiredText("Recipe name: ", "Unnamed Recipe");
            int calories = ReadNonNegativeInt("Calories: ");
            int protein = ReadNonNegativeInt("Protein: ");
            int carbs = ReadNonNegativeInt("Carbs: ");
            int fat = ReadNonNegativeInt("Fat: ");
            string category = ReadRequiredText("Category: ", "General");
            List<Ingredient> ingredients = ReadIngredients();

            _recipeService.AddRecipe(new Recipe(name, calories, protein, carbs, fat, ingredients, category));
            Console.WriteLine("Recipe saved.");
        }

        private void EditRecipe()
        {
            Console.WriteLine("\n---- Edit Recipe ----");

            if (!_recipeService.Recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            ViewRecipes();
            int choice = ReadNonNegativeInt("Enter recipe number to edit: ");

            if (choice < 1 || choice > _recipeService.Recipes.Count)
            {
                Console.WriteLine("Invalid recipe number.");
                return;
            }

            Recipe recipe = _recipeService.Recipes[choice - 1];
            string oldName = recipe.Name;

            recipe.Name = ReadOptionalText($"Name ({recipe.Name}): ", recipe.Name);
            recipe.Calories = ReadOptionalInt($"Calories ({recipe.Calories}): ", recipe.Calories);
            recipe.Protein = ReadOptionalInt($"Protein ({recipe.Protein}): ", recipe.Protein);
            recipe.Carbs = ReadOptionalInt($"Carbs ({recipe.Carbs}): ", recipe.Carbs);
            recipe.Fat = ReadOptionalInt($"Fats ({recipe.Fat}): ", recipe.Fat);
            recipe.Category = ReadOptionalText($"Category ({recipe.Category}): ", recipe.Category);

            if (Confirm("Do you want to replace ingredients? (y/n): "))
            {
                recipe.Ingredients = ReadIngredients();
            }

            _mealPlanService.ReplaceRecipeNameInMealPlan(oldName, recipe.Name);
            _recipeService.SaveRecipes();
            _mealPlanService.SaveWeeklyMealPlan();

            Console.WriteLine("Recipe updated.");
        }

        private void DeleteRecipe()
        {
            Console.WriteLine("\n---- Delete Recipe ----");

            if (!_recipeService.Recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            ViewRecipes();
            int choice = ReadNonNegativeInt("Enter recipe number to delete: ");

            if (choice < 1 || choice > _recipeService.Recipes.Count)
            {
                Console.WriteLine("Invalid recipe number.");
                return;
            }

            Recipe recipe = _recipeService.Recipes[choice - 1];

            if (!Confirm($"Are you sure you want to delete '{recipe.Name}'? (y/n): "))
            {
                Console.WriteLine("Delete canceled.");
                return;
            }

            _mealPlanService.RemoveRecipeFromMealPlan(recipe.Name);
            _recipeService.DeleteRecipe(choice - 1);
            _mealPlanService.SaveWeeklyMealPlan();

            Console.WriteLine("Recipe deleted.");
        }

        private void CreateWeeklyMealPlan()
        {
            Console.WriteLine("\n---- Create Weekly Meal Plan ----");

            if (!_recipeService.Recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            foreach (string day in WeeklyMealPlan.DaysOfWeek)
            {
                Console.WriteLine($"\n--- {day} ---");
                List<Recipe> mealsForDay = SelectMealsForDay(day);
                _mealPlanService.SetMeals(day, mealsForDay);
            }

            _mealPlanService.SaveWeeklyMealPlan();
            Console.WriteLine("Weekly meal plan saved.");
        }

        private void EditWeeklyMealPlan()
        {
            Console.WriteLine("\n---- Edit Weekly Meal Plan ----");

            if (!_recipeService.Recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            DisplayDaysOfWeek();
            int dayChoice = ReadNonNegativeInt("Choose a day to edit: ");

            if (dayChoice < 1 || dayChoice > WeeklyMealPlan.DaysOfWeek.Length)
            {
                Console.WriteLine("Invalid day choice.");
                return;
            }

            string selectedDay = WeeklyMealPlan.DaysOfWeek[dayChoice - 1];
            List<Recipe> currentMeals = _mealPlanService.GetMeals(selectedDay);

            Console.WriteLine($"\nCurrent meals for {selectedDay}:");

            if (!currentMeals.Any())
            {
                Console.WriteLine("No meals planned.");
            }
            else
            {
                for (int i = 0; i < currentMeals.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {currentMeals[i].Name}");
                }
            }

            List<Recipe> newMeals = SelectMealsForDay(selectedDay);
            _mealPlanService.SetMeals(selectedDay, newMeals);
            _mealPlanService.SaveWeeklyMealPlan();

            Console.WriteLine($"{selectedDay} meal plan updated.");
        }

        private void ViewWeeklyMealPlan()
        {
            Console.WriteLine("\n---- Weekly Meal Plan ----");

            bool hasAnyMeals = WeeklyMealPlan.DaysOfWeek.Any(day => _mealPlanService.GetMeals(day).Any());

            if (!hasAnyMeals)
            {
                Console.WriteLine("No meals planned yet.");
                return;
            }

            foreach (string day in WeeklyMealPlan.DaysOfWeek)
            {
                Console.WriteLine($"\n{day}:");
                DisplayMealList(_mealPlanService.GetMeals(day), "  No meals planned.", false, "  ");
            }
        }

        private void GenerateGroceryList()
        {
            Console.WriteLine("\n---- Grocery List ----");

            var groceryTotals = _mealPlanService.GenerateGroceryList();

            if (!groceryTotals.Any())
            {
                Console.WriteLine("No meal plan available.");
                return;
            }

            foreach (var item in groceryTotals.OrderBy(i => i.Key))
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }

        private void UpdateUserProfile()
        {
            Console.WriteLine("\n---- Update Profile ----");

            _userProfile.Name = ReadOptionalText($"Name ({_userProfile.Name}): ", _userProfile.Name);
            _userProfile.CalorieGoal = ReadOptionalInt($"Calories ({_userProfile.CalorieGoal}): ", _userProfile.CalorieGoal);
            _userProfile.ProteinGoal = ReadOptionalInt($"Protein ({_userProfile.ProteinGoal}): ", _userProfile.ProteinGoal);
            _userProfile.CarbGoal = ReadOptionalInt($"Carbs ({_userProfile.CarbGoal}): ", _userProfile.CarbGoal);
            _userProfile.FatGoal = ReadOptionalInt($"Fat ({_userProfile.FatGoal}): ", _userProfile.FatGoal);

            _profileService.SaveUserProfile(_userProfile);
            Console.WriteLine("Profile updated.");
        }

        private void SetCurrentDay()
        {
            Console.WriteLine("\n---- Set Current Day ----");
            DisplayDaysOfWeek();

            int choice = ReadNonNegativeInt("Choose the current day: ");

            if (choice < 1 || choice > WeeklyMealPlan.DaysOfWeek.Length)
            {
                Console.WriteLine("Invalid day choice.");
                return;
            }

            _userProfile.CurrentDay = WeeklyMealPlan.DaysOfWeek[choice - 1];
            _profileService.SaveUserProfile(_userProfile);

            Console.WriteLine($"Current day set to {_userProfile.CurrentDay}.");
        }

        private void ViewCurrentDayMealPlan()
        {
            Console.WriteLine($"\n---- Meal Plan For {_userProfile.CurrentDay} ----");

            List<Recipe> meals = _mealPlanService.GetMeals(_userProfile.CurrentDay);
            DisplayMealList(meals, "No meals planned for this day.", true);
        }

        private List<Recipe> SelectMealsForDay(string day)
        {
            Console.WriteLine("You can add up to 3 meals.");
            Console.WriteLine("Enter 0 when you are done.");

            List<Recipe> selectedMeals = new();

            for (int mealNumber = 1; mealNumber <= 3; mealNumber++)
            {
                ViewRecipes();
                int choice = ReadNonNegativeInt($"Select recipe #{mealNumber} for {day} (0 to stop): ");

                if (choice == 0)
                {
                    break;
                }

                if (choice < 1 || choice > _recipeService.Recipes.Count)
                {
                    Console.WriteLine("Invalid choice.");
                    mealNumber--;
                    continue;
                }

                Recipe recipe = _recipeService.Recipes[choice - 1];
                selectedMeals.Add(recipe);
                Console.WriteLine($"{recipe.Name} added to {day}.");
            }

            return selectedMeals;
        }

        private List<Ingredient> ReadIngredients()
        {
            List<Ingredient> ingredients = new();

            Console.WriteLine("Add ingredients one by one.");
            Console.WriteLine("Type 'done' when finished.");

            while (true)
            {
                Console.Write("Ingredient: ");
                string input = Console.ReadLine()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Ingredient name cannot be empty.");
                    continue;
                }

                if (input.Equals("done", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                double quantity = ReadNonNegativeDouble("Quantity: ");
                ingredients.Add(new Ingredient(input, quantity));
            }

            return ingredients;
        }

        private void DisplayMealList(List<Recipe> meals, string emptyMessage, bool includeTotals, string indent = "")
        {
            if (!meals.Any())
            {
                Console.WriteLine(emptyMessage);
                return;
            }

            int totalCalories = 0;
            int totalProtein = 0;
            int totalCarbs = 0;
            int totalFat = 0;

            for (int i = 0; i < meals.Count; i++)
            {
                Recipe recipe = meals[i];
                Console.WriteLine($"{indent}Meal {i + 1}: {FormatRecipe(recipe)}");

                totalCalories += recipe.Calories;
                totalProtein += recipe.Protein;
                totalCarbs += recipe.Carbs;
                totalFat += recipe.Fat;
            }

            if (includeTotals)
            {
                Console.WriteLine("\nTotals for the day:");
                Console.WriteLine($"Calories: {totalCalories}");
                Console.WriteLine($"Protein: {totalProtein}");
                Console.WriteLine($"Carbs: {totalCarbs}");
                Console.WriteLine($"Fats: {totalFat}");
            }
        }

        private string FormatRecipe(Recipe recipe)
        {
            return $"{recipe.Name} [{recipe.Category}] - {recipe.Calories} Calories, Protein:{recipe.Protein} Carbs:{recipe.Carbs} Fats:{recipe.Fat}";
        }

        private void DisplayDaysOfWeek()
        {
            for (int i = 0; i < WeeklyMealPlan.DaysOfWeek.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {WeeklyMealPlan.DaysOfWeek[i]}");
            }
        }

        private bool Confirm(string message)
        {
            Console.Write(message);
            string input = Console.ReadLine()?.Trim() ?? string.Empty;
            return input.Equals("y", StringComparison.OrdinalIgnoreCase);
        }

        private string ReadRequiredText(string message, string fallbackValue)
        {
            Console.Write(message);
            string input = Console.ReadLine() ?? string.Empty;
            return string.IsNullOrWhiteSpace(input) ? fallbackValue : input.Trim();
        }

        private string ReadOptionalText(string message, string currentValue)
        {
            Console.Write(message);
            string input = Console.ReadLine() ?? string.Empty;
            return string.IsNullOrWhiteSpace(input) ? currentValue : input.Trim();
        }

        private int ReadOptionalInt(string message, int currentValue)
        {
            Console.Write(message);
            string input = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return currentValue;
            }

            return int.TryParse(input, out int value) && value >= 0
                ? value
                : currentValue;
        }

        private int ReadNonNegativeInt(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine() ?? string.Empty;

                if (int.TryParse(input, out int value) && value >= 0)
                {
                    return value;
                }

                Console.WriteLine("Enter a valid non-negative whole number.");
            }
        }

        private double ReadNonNegativeDouble(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine() ?? string.Empty;

                if (double.TryParse(input, out double value) && value >= 0)
                {
                    return value;
                }

                Console.WriteLine("Enter a valid non-negative number.");
            }
        }
    }
}