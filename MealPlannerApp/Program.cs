using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MealPlannerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NutritionApp app = new NutritionApp();
            app.Run();
        }
    }

    public class NutritionApp
    {
        private UserProfile _userProfile = null!;
        private readonly string _profileFilePath = "userProfile.txt";
        private readonly string _recipesFilePath = "recipes.txt";
        private readonly string _weeklyMealPlanFilePath = "weeklyMealPlan.txt";

        private List<FoodEntry> _dailyFoodEntries = new List<FoodEntry>();
        private List<Recipe> _recipes = new List<Recipe>();
        private WeeklyMealPlan _weeklyMealPlan = new WeeklyMealPlan();

        public void Run()
        {
            LoadOrCreateUserProfile();
            LoadRecipes();
            LoadWeeklyMealPlan();

            bool running = true;

            while (running)
            {
                Console.WriteLine($"\n---- Welcome, {_userProfile.Name}! ----");
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
                Console.WriteLine("13. Exit");
                Console.Write("Choose an option: ");

                string? input = Console.ReadLine();

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
                        running = false;
                        Console.WriteLine("Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
        }

        private void LoadOrCreateUserProfile()
        {
            if (File.Exists(_profileFilePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(_profileFilePath);

                    if (lines.Length >= 5)
                    {
                        _userProfile = new UserProfile(
                            lines[0],
                            int.Parse(lines[1]),
                            int.Parse(lines[2]),
                            int.Parse(lines[3]),
                            int.Parse(lines[4])
                        );

                        Console.WriteLine("Profile loaded from file.");
                        return;
                    }
                }
                catch
                {
                    Console.WriteLine("Profile file was invalid. Creating a new one.");
                }
            }

            SetupUserProfile();
            SaveUserProfile();
        }

        private void SetupUserProfile()
        {
            Console.WriteLine("\n---- Set Up Your Profile ----");

            Console.Write("Enter your name: ");
            string name = Console.ReadLine() ?? "User";

            if (string.IsNullOrWhiteSpace(name))
                name = "User";

            int calories = ReadInt("Calories: ");
            int protein = ReadInt("Protein: ");
            int carbs = ReadInt("Carbs: ");
            int fat = ReadInt("Fat: ");

            _userProfile = new UserProfile(name, calories, protein, carbs, fat);
            Console.WriteLine("Profile created.");
        }

        private void SaveUserProfile()
        {
            File.WriteAllLines(_profileFilePath, new string[]
            {
                _userProfile.Name,
                _userProfile.CalorieGoal.ToString(),
                _userProfile.ProteinGoal.ToString(),
                _userProfile.CarbGoal.ToString(),
                _userProfile.FatGoal.ToString()
            });
        }

        private void UpdateUserProfile()
        {
            Console.WriteLine("\n---- Update Profile ----");

            Console.Write($"Name ({_userProfile.Name}): ");
            string? n = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(n))
                _userProfile.Name = n;

            Console.Write($"Calories ({_userProfile.CalorieGoal}): ");
            string? caloriesInput = Console.ReadLine();
            if (int.TryParse(caloriesInput, out int calories) && calories >= 0)
                _userProfile.CalorieGoal = calories;

            Console.Write($"Protein ({_userProfile.ProteinGoal}): ");
            string? proteinInput = Console.ReadLine();
            if (int.TryParse(proteinInput, out int protein) && protein >= 0)
                _userProfile.ProteinGoal = protein;

            Console.Write($"Carbs ({_userProfile.CarbGoal}): ");
            string? carbsInput = Console.ReadLine();
            if (int.TryParse(carbsInput, out int carbs) && carbs >= 0)
                _userProfile.CarbGoal = carbs;

            Console.Write($"Fat ({_userProfile.FatGoal}): ");
            string? fatInput = Console.ReadLine();
            if (int.TryParse(fatInput, out int fat) && fat >= 0)
                _userProfile.FatGoal = fat;

            SaveUserProfile();
            Console.WriteLine("Profile updated.");
        }

        private void LoadRecipes()
        {
            _recipes.Clear();

            if (!File.Exists(_recipesFilePath))
            {
                File.WriteAllText(_recipesFilePath, "");
                return;
            }

            foreach (string line in File.ReadAllLines(_recipesFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('|');
                if (parts.Length < 7)
                    continue;

                List<Ingredient> ingredients = new List<Ingredient>();

                if (!string.IsNullOrWhiteSpace(parts[6]))
                {
                    ingredients = parts[6]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(i =>
                        {
                            var p = i.Split(':');
                            return new Ingredient(p[0], double.Parse(p[1]));
                        })
                        .ToList();
                }

                _recipes.Add(new Recipe(
                    parts[0],
                    int.Parse(parts[1]),
                    int.Parse(parts[2]),
                    int.Parse(parts[3]),
                    int.Parse(parts[4]),
                    ingredients,
                    parts[5]
                ));
            }
        }

        private void SaveRecipes()
        {
            var lines = _recipes.Select(r =>
            {
                string ingredients = string.Join(",",
                    r.Ingredients.Select(i => $"{i.Name}:{i.Quantity}"));

                return $"{r.Name}|{r.Calories}|{r.Protein}|{r.Carbs}|{r.Fat}|{r.Category}|{ingredients}";
            });

            File.WriteAllLines(_recipesFilePath, lines);
        }

        private void AddRecipe()
        {
            Console.WriteLine("\n---- Add New Recipe ----");

            Console.Write("Recipe name: ");
            string name = Console.ReadLine() ?? "Unnamed Recipe";
            if (string.IsNullOrWhiteSpace(name))
                name = "Unnamed Recipe";

            int calories = ReadInt("Calories: ");
            int protein = ReadInt("Protein: ");
            int carbs = ReadInt("Carbs: ");
            int fat = ReadInt("Fat: ");

            Console.Write("Category: ");
            string category = Console.ReadLine() ?? "General";
            if (string.IsNullOrWhiteSpace(category))
                category = "General";

            List<Ingredient> ingredients = ReadIngredients();

            _recipes.Add(new Recipe(name, calories, protein, carbs, fat, ingredients, category));
            SaveRecipes();

            Console.WriteLine("Recipe saved.");
        }

        private void EditRecipe()
        {
            Console.WriteLine("\n---- Edit Recipe ----");

            if (!_recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            ViewRecipes();
            int choice = ReadInt("Enter recipe number to edit: ");

            if (choice < 1 || choice > _recipes.Count)
            {
                Console.WriteLine("Invalid recipe number.");
                return;
            }

            Recipe recipe = _recipes[choice - 1];
            string oldName = recipe.Name;

            Console.Write($"Name ({recipe.Name}): ");
            string? nameInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nameInput))
                recipe.Name = nameInput;

            Console.Write($"Calories ({recipe.Calories}): ");
            string? caloriesInput = Console.ReadLine();
            if (int.TryParse(caloriesInput, out int calories) && calories >= 0)
                recipe.Calories = calories;

            Console.Write($"Protein ({recipe.Protein}): ");
            string? proteinInput = Console.ReadLine();
            if (int.TryParse(proteinInput, out int protein) && protein >= 0)
                recipe.Protein = protein;

            Console.Write($"Carbs ({recipe.Carbs}): ");
            string? carbsInput = Console.ReadLine();
            if (int.TryParse(carbsInput, out int carbs) && carbs >= 0)
                recipe.Carbs = carbs;

            Console.Write($"Fats ({recipe.Fat}): ");
            string? fatInput = Console.ReadLine();
            if (int.TryParse(fatInput, out int fat) && fat >= 0)
                recipe.Fat = fat;

            Console.Write($"Category ({recipe.Category}): ");
            string? categoryInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(categoryInput))
                recipe.Category = categoryInput;

            Console.Write("Do you want to replace ingredients? (y/n): ");
            string? replaceIngredients = Console.ReadLine();

            if (replaceIngredients?.Trim().ToLower() == "y")
            {
                recipe.Ingredients = ReadIngredients();
            }

            ReplaceRecipeNameInMealPlan(oldName, recipe.Name);
            SaveRecipes();
            SaveWeeklyMealPlan();

            Console.WriteLine("Recipe updated.");
        }

        private void DeleteRecipe()
        {
            Console.WriteLine("\n---- Delete Recipe ----");

            if (!_recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            ViewRecipes();
            int choice = ReadInt("Enter recipe number to delete: ");

            if (choice < 1 || choice > _recipes.Count)
            {
                Console.WriteLine("Invalid recipe number.");
                return;
            }

            Recipe recipe = _recipes[choice - 1];

            Console.Write($"Are you sure you want to delete '{recipe.Name}'? (y/n): ");
            string? confirm = Console.ReadLine();

            if (confirm?.Trim().ToLower() == "y")
            {
                RemoveRecipeFromMealPlan(recipe.Name);
                _recipes.RemoveAt(choice - 1);
                SaveRecipes();
                SaveWeeklyMealPlan();
                Console.WriteLine("Recipe deleted.");
            }
            else
            {
                Console.WriteLine("Delete canceled.");
            }
        }

        private List<Ingredient> ReadIngredients()
        {
            List<Ingredient> ingredients = new List<Ingredient>();

            Console.WriteLine("Add ingredients one by one.");
            Console.WriteLine("Type 'done' when finished.");

            while (true)
            {
                Console.Write("Ingredient: ");
                string? ingredientName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(ingredientName))
                {
                    Console.WriteLine("Ingredient name cannot be empty.");
                    continue;
                }

                if (ingredientName.Trim().ToLower() == "done")
                    break;

                double quantity = ReadDouble("Quantity: ");
                ingredients.Add(new Ingredient(ingredientName, quantity));
            }

            return ingredients;
        }

        private void LoadWeeklyMealPlan()
        {
            _weeklyMealPlan = new WeeklyMealPlan();

            if (!File.Exists(_weeklyMealPlanFilePath))
            {
                SaveWeeklyMealPlan();
                return;
            }

            foreach (string line in File.ReadAllLines(_weeklyMealPlanFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('|');
                if (parts.Length < 2)
                    continue;

                string day = parts[0];
                List<Recipe> meals = new List<Recipe>();

                if (!string.IsNullOrWhiteSpace(parts[1]))
                {
                    string[] recipeNames = parts[1]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);

                    foreach (string recipeName in recipeNames)
                    {
                        Recipe? recipe = _recipes.FirstOrDefault(r =>
                            r.Name.Equals(recipeName.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (recipe != null && meals.Count < 3)
                        {
                            meals.Add(recipe);
                        }
                    }
                }

                _weeklyMealPlan.SetMeals(day, meals);
            }
        }

        private void SaveWeeklyMealPlan()
        {
            List<string> lines = new List<string>();

            foreach (string day in WeeklyMealPlan.DaysOfWeek)
            {
                List<Recipe> meals = _weeklyMealPlan.GetMeals(day);
                string mealNames = string.Join(",", meals.Select(m => m.Name));
                lines.Add($"{day}|{mealNames}");
            }

            File.WriteAllLines(_weeklyMealPlanFilePath, lines);
        }

        private void CreateWeeklyMealPlan()
        {
            Console.WriteLine("\n---- Create Weekly Meal Plan ----");

            if (!_recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            foreach (string day in WeeklyMealPlan.DaysOfWeek)
            {
                Console.WriteLine($"\n--- {day} ---");
                Console.WriteLine("You can add up to 3 meals.");
                Console.WriteLine("Enter 0 when you are done with this day.");

                List<Recipe> mealsForDay = new List<Recipe>();

                for (int mealNumber = 1; mealNumber <= 3; mealNumber++)
                {
                    ViewRecipes();
                    int choice = ReadInt($"Select recipe #{mealNumber} for {day} (0 to stop): ");

                    if (choice == 0)
                        break;

                    if (choice < 1 || choice > _recipes.Count)
                    {
                        Console.WriteLine("Invalid choice.");
                        mealNumber--;
                        continue;
                    }

                    Recipe selectedRecipe = _recipes[choice - 1];
                    mealsForDay.Add(selectedRecipe);
                    Console.WriteLine($"{selectedRecipe.Name} added to {day}.");
                }

                _weeklyMealPlan.SetMeals(day, mealsForDay);
            }

            SaveWeeklyMealPlan();
            Console.WriteLine("Weekly meal plan saved.");
        }

        private void EditWeeklyMealPlan()
        {
            Console.WriteLine("\n---- Edit Weekly Meal Plan ----");

            if (!_recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            for (int i = 0; i < WeeklyMealPlan.DaysOfWeek.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {WeeklyMealPlan.DaysOfWeek[i]}");
            }

            int dayChoice = ReadInt("Choose a day to edit: ");

            if (dayChoice < 1 || dayChoice > WeeklyMealPlan.DaysOfWeek.Length)
            {
                Console.WriteLine("Invalid day choice.");
                return;
            }

            string selectedDay = WeeklyMealPlan.DaysOfWeek[dayChoice - 1];
            List<Recipe> currentMeals = _weeklyMealPlan.GetMeals(selectedDay);

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

            Console.WriteLine("\nEnter up to 3 new meals for this day.");
            Console.WriteLine("This will replace the current meals.");
            Console.WriteLine("Enter 0 when you are done.");

            List<Recipe> newMeals = new List<Recipe>();

            for (int mealNumber = 1; mealNumber <= 3; mealNumber++)
            {
                ViewRecipes();
                int recipeChoice = ReadInt($"Select recipe #{mealNumber} for {selectedDay} (0 to stop): ");

                if (recipeChoice == 0)
                    break;

                if (recipeChoice < 1 || recipeChoice > _recipes.Count)
                {
                    Console.WriteLine("Invalid choice.");
                    mealNumber--;
                    continue;
                }

                Recipe selectedRecipe = _recipes[recipeChoice - 1];
                newMeals.Add(selectedRecipe);
                Console.WriteLine($"{selectedRecipe.Name} added to {selectedDay}.");
            }

            _weeklyMealPlan.SetMeals(selectedDay, newMeals);
            SaveWeeklyMealPlan();

            Console.WriteLine($"{selectedDay} meal plan updated.");
        }

        private void ViewWeeklyMealPlan()
        {
            Console.WriteLine("\n---- Weekly Meal Plan ----");

            bool hasAnyMeals = _weeklyMealPlan.Meals.Values.Any(dayMeals => dayMeals.Any());

            if (!hasAnyMeals)
            {
                Console.WriteLine("No meals planned yet.");
                return;
            }

            foreach (string day in WeeklyMealPlan.DaysOfWeek)
            {
                List<Recipe> meals = _weeklyMealPlan.GetMeals(day);

                Console.WriteLine($"\n{day}:");

                if (!meals.Any())
                {
                    Console.WriteLine("  No meals planned.");
                    continue;
                }

                for (int i = 0; i < meals.Count; i++)
                {
                    Recipe recipe = meals[i];
                    Console.WriteLine($"  Meal {i + 1}: {recipe.Name} [{recipe.Category}] - {recipe.Calories} Calories, Protein:{recipe.Protein} Carbs:{recipe.Carbs} Fats:{recipe.Fat}");
                }
            }
        }

        private void GenerateGroceryList()
        {
            Console.WriteLine("\n---- Grocery List ----");

            bool hasAnyMeals = _weeklyMealPlan.Meals.Values.Any(dayMeals => dayMeals.Any());

            if (!hasAnyMeals)
            {
                Console.WriteLine("No meal plan available.");
                return;
            }

            Dictionary<string, double> groceryTotals = new Dictionary<string, double>();

            foreach (List<Recipe> meals in _weeklyMealPlan.Meals.Values)
            {
                foreach (Recipe recipe in meals)
                {
                    foreach (Ingredient ingredient in recipe.Ingredients)
                    {
                        if (groceryTotals.ContainsKey(ingredient.Name))
                            groceryTotals[ingredient.Name] += ingredient.Quantity;
                        else
                            groceryTotals[ingredient.Name] = ingredient.Quantity;
                    }
                }
            }

            foreach (var item in groceryTotals.OrderBy(i => i.Key))
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }

        private void ReplaceRecipeNameInMealPlan(string oldName, string newName)
        {
            foreach (var dayMeals in _weeklyMealPlan.Meals.Values)
            {
                foreach (var meal in dayMeals)
                {
                    if (meal.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                    {
                        meal.Name = newName;
                    }
                }
            }
        }

        private void RemoveRecipeFromMealPlan(string recipeName)
        {
            foreach (string day in WeeklyMealPlan.DaysOfWeek)
            {
                List<Recipe> meals = _weeklyMealPlan.GetMeals(day)
                    .Where(m => !m.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _weeklyMealPlan.SetMeals(day, meals);
            }
        }

        private void ViewRecipes()
        {
            Console.WriteLine("\n---- Recipes ----");

            if (!_recipes.Any())
            {
                Console.WriteLine("No recipes available.");
                return;
            }

            for (int i = 0; i < _recipes.Count; i++)
            {
                Recipe recipe = _recipes[i];
                Console.WriteLine($"{i + 1}. {recipe.Name} [{recipe.Category}] - {recipe.Calories} Calories, Protein:{recipe.Protein} Carbs:{recipe.Carbs} Fats:{recipe.Fat}");
            }
        }

        private void ViewUserGoals()
        {
            Console.WriteLine("\n---- User Goals ----");
            Console.WriteLine($"Name: {_userProfile.Name}");
            Console.WriteLine($"Calories: {_userProfile.CalorieGoal}");
            Console.WriteLine($"Protein: {_userProfile.ProteinGoal}");
            Console.WriteLine($"Carbs: {_userProfile.CarbGoal}");
            Console.WriteLine($"Fats: {_userProfile.FatGoal}");
        }

        private void AddDailyFoodEntry()
        {
            Console.WriteLine("\n---- Add Daily Food Entry ----");

            Console.Write("Food name: ");
            string name = Console.ReadLine() ?? "Unknown Food";
            if (string.IsNullOrWhiteSpace(name))
                name = "Unknown Food";

            int calories = ReadInt("Calories: ");
            int protein = ReadInt("Protein: ");
            int carbs = ReadInt("Carbs: ");
            int fat = ReadInt("Fat: ");

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

            foreach (var entry in _dailyFoodEntries)
            {
                Console.WriteLine($"- {entry.Name}: {entry.Calories} Calories, Protein:{entry.Protein} Carbs:{entry.Carbs} Fats:{entry.Fat}");
            }

            int totalCalories = _dailyFoodEntries.Sum(f => f.Calories);
            int totalProtein = _dailyFoodEntries.Sum(f => f.Protein);
            int totalCarbs = _dailyFoodEntries.Sum(f => f.Carbs);
            int totalFat = _dailyFoodEntries.Sum(f => f.Fat);

            Console.WriteLine($"\nCalories: {totalCalories}/{_userProfile.CalorieGoal}");
            Console.WriteLine($"Protein: {totalProtein}/{_userProfile.ProteinGoal}");
            Console.WriteLine($"Carbs: {totalCarbs}/{_userProfile.CarbGoal}");
            Console.WriteLine($"Fats: {totalFat}/{_userProfile.FatGoal}");
        }

        private int ReadInt(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value >= 0)
                    return value;

                Console.WriteLine("Enter a valid non-negative whole number.");
            }
        }

        private double ReadDouble(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                string? input = Console.ReadLine();

                if (double.TryParse(input, out double value) && value >= 0)
                    return value;

                Console.WriteLine("Enter a valid non-negative number.");
            }
        }
    }

    public class UserProfile
    {
        public string Name { get; set; }
        public int CalorieGoal { get; set; }
        public int ProteinGoal { get; set; }
        public int CarbGoal { get; set; }
        public int FatGoal { get; set; }

        public UserProfile(string n, int c, int p, int cb, int f)
        {
            Name = n;
            CalorieGoal = c;
            ProteinGoal = p;
            CarbGoal = cb;
            FatGoal = f;
        }
    }

    public class FoodEntry
    {
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }

        public FoodEntry(string n, int c, int p, int cb, int f)
        {
            Name = n;
            Calories = c;
            Protein = p;
            Carbs = cb;
            Fat = f;
        }
    }

    public class Recipe
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
        public List<Ingredient> Ingredients { get; set; }

        public Recipe(string n, int c, int p, int cb, int f, List<Ingredient> i, string cat)
        {
            Name = n;
            Calories = c;
            Protein = p;
            Carbs = cb;
            Fat = f;
            Ingredients = i;
            Category = cat;
        }
    }

    public class Ingredient
    {
        public string Name { get; set; }
        public double Quantity { get; set; }

        public Ingredient(string n, double q)
        {
            Name = n;
            Quantity = q;
        }
    }

    public class WeeklyMealPlan
    {
        public static readonly string[] DaysOfWeek =
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        public Dictionary<string, List<Recipe>> Meals { get; set; }

        public WeeklyMealPlan()
        {
            Meals = new Dictionary<string, List<Recipe>>();

            foreach (string day in DaysOfWeek)
            {
                Meals[day] = new List<Recipe>();
            }
        }

        public void SetMeals(string day, List<Recipe> recipes)
        {
            Meals[day] = recipes.Take(3).ToList();
        }

        public List<Recipe> GetMeals(string day)
        {
            if (Meals.ContainsKey(day))
                return Meals[day];

            return new List<Recipe>();
        }
    }
}