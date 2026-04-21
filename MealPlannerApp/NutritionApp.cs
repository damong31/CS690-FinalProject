using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

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
                Console.Clear();

                DisplayMainMenu();
                string input = Console.ReadLine()?.Trim() ?? string.Empty;

                Console.Clear();

                running = HandleMenuChoice(input);
                if (running)
                {
                    Console.WriteLine("\nPress any key to continue");
                    Console.ReadKey();
                }
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
                    Console.Clear();
                    AnsiConsole.MarkupLine("[bold green]---- Daily Macro Summary ----[/]");
                    AnsiConsole.MarkupLine($"[bold]Current Day:[/] {_userProfile.CurrentDay}");
                    AnsiConsole.WriteLine();

                    List<Recipe> plannedMeals = _mealPlanService.GetMeals(_userProfile.CurrentDay);

                    bool hasFoodEntries = _dailyFoodEntries.Any();
                    bool hasPlannedMeals = plannedMeals.Any();

                    if (!hasFoodEntries && !hasPlannedMeals)
                    {
                        AnsiConsole.MarkupLine("[red]No food entries or planned meals found for today.[/]");
                        return;
                    }

                    int totalCalories = 0;
                    int totalProtein = 0;
                    int totalCarbs = 0;
                    int totalFat = 0;

                    if (hasPlannedMeals)
                    {
                        AnsiConsole.MarkupLine("[bold yellow]Planned Meals[/]");

                        var mealsTable = new Table();
                        mealsTable.Border(TableBorder.Rounded);
                        mealsTable.AddColumn("[bold]Meal[/]");
                        mealsTable.AddColumn("[bold]Category[/]");
                        mealsTable.AddColumn("[bold]Calories[/]");
                        mealsTable.AddColumn("[bold]Protein[/]");
                        mealsTable.AddColumn("[bold]Carbs[/]");
                        mealsTable.AddColumn("[bold]Fats[/]");

                        foreach (Recipe meal in plannedMeals)
                        {
                            mealsTable.AddRow(
                                meal.Name,
                                meal.Category,
                                meal.Calories.ToString(),
                                meal.Protein.ToString(),
                                meal.Carbs.ToString(),
                                meal.Fat.ToString()
                            );

                            totalCalories += meal.Calories;
                            totalProtein += meal.Protein;
                            totalCarbs += meal.Carbs;
                            totalFat += meal.Fat;
                        }

                        AnsiConsole.Write(mealsTable);
                        AnsiConsole.WriteLine();
                    }

                    if (hasFoodEntries)
                    {
                        AnsiConsole.MarkupLine("[bold cyan]Daily Food Entries[/]");

                        var foodTable = new Table();
                        foodTable.Border(TableBorder.Rounded);
                        foodTable.AddColumn("[bold]Food[/]");
                        foodTable.AddColumn("[bold]Calories[/]");
                        foodTable.AddColumn("[bold]Protein[/]");
                        foodTable.AddColumn("[bold]Carbs[/]");
                        foodTable.AddColumn("[bold]Fats[/]");

                        foreach (FoodEntry entry in _dailyFoodEntries)
                        {
                            foodTable.AddRow(
                                entry.Name,
                                entry.Calories.ToString(),
                                entry.Protein.ToString(),
                                entry.Carbs.ToString(),
                                entry.Fat.ToString()
                            );

                            totalCalories += entry.Calories;
                            totalProtein += entry.Protein;
                            totalCarbs += entry.Carbs;
                            totalFat += entry.Fat;
                        }

                        AnsiConsole.Write(foodTable);
                        AnsiConsole.WriteLine();
                    }

                    AnsiConsole.MarkupLine("[bold green]Combined Totals[/]");
                    AnsiConsole.MarkupLine($"[yellow]Calories:[/] {totalCalories}/{_userProfile.CalorieGoal}");
                    AnsiConsole.MarkupLine($"[green]Protein:[/] {totalProtein}/{_userProfile.ProteinGoal}");
                    AnsiConsole.MarkupLine($"[blue]Carbs:[/] {totalCarbs}/{_userProfile.CarbGoal}");
                    AnsiConsole.MarkupLine($"[orange1]Fats:[/] {totalFat}/{_userProfile.FatGoal}");
                    AnsiConsole.WriteLine();

                    DisplayMacroPercentageChart(totalCalories, totalProtein, totalCarbs, totalFat);
                }

        private void ViewRecipes()
                {
                    Console.Clear();
                    AnsiConsole.MarkupLine("[bold green]---- Recipes ----[/]");
                    AnsiConsole.WriteLine();

                    if (!_recipeService.Recipes.Any())
                    {
                        AnsiConsole.MarkupLine("[red]No recipes available.[/]");
                        return;
                    }

                    var table = new Table();
                    table.Border(TableBorder.Rounded);
                    table.Expand();

                    table.AddColumn("[bold]#[/]");
                    table.AddColumn("[bold]Recipe[/]");
                    table.AddColumn("[bold]Category[/]");
                    table.AddColumn("[bold]Calories[/]");
                    table.AddColumn("[bold]Protein[/]");
                    table.AddColumn("[bold]Carbs[/]");
                    table.AddColumn("[bold]Fats[/]");
                    table.AddColumn("[bold]Ingredients[/]");

                    for (int i = 0; i < _recipeService.Recipes.Count; i++)
                    {
                        Recipe recipe = _recipeService.Recipes[i];

                        string ingredientsText = recipe.Ingredients.Any()
                            ? string.Join(", ", recipe.Ingredients.Select(i => $"{i.Name} ({i.Quantity})"))
                            : "[grey]None[/]";

                        table.AddRow(
                            (i + 1).ToString(),
                            recipe.Name,
                            recipe.Category,
                            recipe.Calories.ToString(),
                            recipe.Protein.ToString(),
                            recipe.Carbs.ToString(),
                            recipe.Fat.ToString(),
                            ingredientsText
                        );
                    }

                    AnsiConsole.Write(table);
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
                    Console.Clear();
                    AnsiConsole.MarkupLine("[bold green]---- Weekly Meal Plan ----[/]");
                    AnsiConsole.WriteLine();

                    bool hasAnyMeals = WeeklyMealPlan.DaysOfWeek.Any(day => _mealPlanService.GetMeals(day).Any());

                    if (!hasAnyMeals)
                    {
                        AnsiConsole.MarkupLine("[red]No meals planned yet.[/]");
                        return;
                    }

                    int weeklyCalories = 0;
                    int weeklyProtein = 0;
                    int weeklyCarbs = 0;
                    int weeklyFat = 0;

                    foreach (string day in WeeklyMealPlan.DaysOfWeek)
                    {
                        List<Recipe> meals = _mealPlanService.GetMeals(day);

                        AnsiConsole.Write(new Rule($"[yellow]{day}[/]").RuleStyle("grey").LeftJustified());

                        if (!meals.Any())
                        {
                            AnsiConsole.MarkupLine("[grey]No meals planned.[/]");
                            AnsiConsole.WriteLine();
                            continue;
                        }

                        var table = new Table();
                        table.Border(TableBorder.Rounded);
                        table.Expand();
                        table.AddColumn("[bold]#[/]");
                        table.AddColumn("[bold]Meal[/]");
                        table.AddColumn("[bold]Category[/]");
                        table.AddColumn("[bold]Calories[/]");
                        table.AddColumn("[bold]Protein[/]");
                        table.AddColumn("[bold]Carbs[/]");
                        table.AddColumn("[bold]Fats[/]");

                        int totalCalories = 0;
                        int totalProtein = 0;
                        int totalCarbs = 0;
                        int totalFat = 0;

                        for (int i = 0; i < meals.Count; i++)
                        {
                            Recipe recipe = meals[i];

                            table.AddRow(
                                (i + 1).ToString(),
                                recipe.Name,
                                recipe.Category,
                                recipe.Calories.ToString(),
                                recipe.Protein.ToString(),
                                recipe.Carbs.ToString(),
                                recipe.Fat.ToString()
                            );

                            totalCalories += recipe.Calories;
                            totalProtein += recipe.Protein;
                            totalCarbs += recipe.Carbs;
                            totalFat += recipe.Fat;
                        }

                        weeklyCalories += totalCalories;
                        weeklyProtein += totalProtein;
                        weeklyCarbs += totalCarbs;
                        weeklyFat += totalFat;

                        AnsiConsole.Write(table);

                        var totalsPanel = new Panel(
                            $"[yellow]Calories:[/] {totalCalories}    " +
                            $"[green]Protein:[/] {totalProtein}    " +
                            $"[blue]Carbs:[/] {totalCarbs}    " +
                            $"[orange1]Fats:[/] {totalFat}")
                        {
                            Header = new PanelHeader($"{day} Totals"),
                            Border = BoxBorder.Rounded
                        };

                        AnsiConsole.Write(totalsPanel);
                        AnsiConsole.WriteLine();
                    }

                    AnsiConsole.Write(new Rule("[bold green]Weekly Totals[/]").RuleStyle("green"));

                    var weeklyPanel = new Panel(
                        $"[yellow]Calories:[/] {weeklyCalories}\n" +
                        $"[green]Protein:[/] {weeklyProtein}\n" +
                        $"[blue]Carbs:[/] {weeklyCarbs}\n" +
                        $"[orange1]Fats:[/] {weeklyFat}")
                    {
                        Header = new PanelHeader("Summary"),
                        Border = BoxBorder.Double
                    };

                    AnsiConsole.Write(weeklyPanel);
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
        private void DisplayMacroChart(int totalCalories, int totalProtein, int totalCarbs, int totalFat)
            {
                var chart = new BarChart()
                    .Width(60)
                    .Label("[green bold]Daily Macros[/]")
                    .CenterLabel();

                chart.AddItem($"Calories ({totalCalories}/{_userProfile.CalorieGoal})", totalCalories,
                    totalCalories > _userProfile.CalorieGoal ? Color.Red : Color.Yellow);

                chart.AddItem($"Protein ({totalProtein}/{_userProfile.ProteinGoal})", totalProtein,
                    totalProtein > _userProfile.ProteinGoal ? Color.Red : Color.Green);

                chart.AddItem($"Carbs ({totalCarbs}/{_userProfile.CarbGoal})", totalCarbs,
                    totalCarbs > _userProfile.CarbGoal ? Color.Red : Color.Blue);

                chart.AddItem($"Fats ({totalFat}/{_userProfile.FatGoal})", totalFat,
                    totalFat > _userProfile.FatGoal ? Color.Red : Color.Orange1);

                AnsiConsole.Write(chart);
            }

            private void DisplayMacroRemainingChart(int totalCalories, int totalProtein, int totalCarbs, int totalFat)
                {
                    int remainingCalories = Math.Max(0, _userProfile.CalorieGoal - totalCalories);
                    int remainingProtein = Math.Max(0, _userProfile.ProteinGoal - totalProtein);
                    int remainingCarbs = Math.Max(0, _userProfile.CarbGoal - totalCarbs);
                    int remainingFat = Math.Max(0, _userProfile.FatGoal - totalFat);

                    var chart = new BarChart()
                        .Width(60)
                        .Label("[green bold]Remaining Macros[/]")
                        .CenterLabel();

                    chart.AddItem("Calories Left", remainingCalories, Color.Yellow);
                    chart.AddItem("Protein Left", remainingProtein, Color.Green);
                    chart.AddItem("Carbs Left", remainingCarbs, Color.Blue);
                    chart.AddItem("Fats Left", remainingFat, Color.Orange1);

                    AnsiConsole.Write(chart);
                }

                private void DisplayMacroPercentageChart(int totalCalories, int totalProtein, int totalCarbs, int totalFat)
                    {
                        double caloriesPercent = _userProfile.CalorieGoal == 0 ? 0 : (double)totalCalories / _userProfile.CalorieGoal * 100;
                        double proteinPercent = _userProfile.ProteinGoal == 0 ? 0 : (double)totalProtein / _userProfile.ProteinGoal * 100;
                        double carbsPercent = _userProfile.CarbGoal == 0 ? 0 : (double)totalCarbs / _userProfile.CarbGoal * 100;
                        double fatPercent = _userProfile.FatGoal == 0 ? 0 : (double)totalFat / _userProfile.FatGoal * 100;

                        var chart = new BarChart()
                            .Width(60)
                            .Label("[green bold]Macro Goal Progress (%) [/]")
                            .CenterLabel();

                        chart.AddItem($"Calories {caloriesPercent:F0}%", caloriesPercent, caloriesPercent > 100 ? Color.Red : Color.Yellow);
                        chart.AddItem($"Protein {proteinPercent:F0}%", proteinPercent, proteinPercent > 100 ? Color.Red : Color.Green);
                        chart.AddItem($"Carbs {carbsPercent:F0}%", carbsPercent, carbsPercent > 100 ? Color.Red : Color.Blue);
                        chart.AddItem($"Fats {fatPercent:F0}%", fatPercent, fatPercent > 100 ? Color.Red : Color.Orange1);

                        AnsiConsole.Write(chart);
                    }
    }
}