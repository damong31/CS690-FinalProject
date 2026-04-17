using System;
using System.Collections.Generic;
using System.Linq;

namespace MealPlannerApp
{
    public class RecipeService
    {
        private const string RecipesFilePath = "recipes.txt";

        private readonly FileService _fileService;

        public List<Recipe> Recipes { get; } = new();

        public RecipeService(FileService fileService)
        {
            _fileService = fileService;
        }

        public void LoadRecipes()
        {
            Recipes.Clear();

            if (!_fileService.Exists(RecipesFilePath))
            {
                _fileService.WriteAllText(RecipesFilePath, string.Empty);
                return;
            }

            foreach (string line in _fileService.ReadAllLines(RecipesFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                Recipe? recipe = ParseRecipe(line);
                if (recipe != null)
                {
                    Recipes.Add(recipe);
                }
            }
        }

        public void SaveRecipes()
        {
            IEnumerable<string> lines = Recipes.Select(recipe =>
            {
                string ingredients = string.Join(",",
                    recipe.Ingredients.Select(i => $"{i.Name}:{i.Quantity}"));

                return $"{recipe.Name}|{recipe.Calories}|{recipe.Protein}|{recipe.Carbs}|{recipe.Fat}|{recipe.Category}|{ingredients}";
            });

            _fileService.WriteAllLines(RecipesFilePath, lines);
        }

        public void AddRecipe(Recipe recipe)
        {
            Recipes.Add(recipe);
            SaveRecipes();
        }

        public void DeleteRecipe(int index)
        {
            Recipes.RemoveAt(index);
            SaveRecipes();
        }

        public Recipe? FindByName(string recipeName)
        {
            return Recipes.FirstOrDefault(r =>
                r.Name.Equals(recipeName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private Recipe? ParseRecipe(string line)
        {
            string[] parts = line.Split('|');
            if (parts.Length < 7)
            {
                return null;
            }

            if (!int.TryParse(parts[1], out int calories) ||
                !int.TryParse(parts[2], out int protein) ||
                !int.TryParse(parts[3], out int carbs) ||
                !int.TryParse(parts[4], out int fat))
            {
                return null;
            }

            List<Ingredient> ingredients = ParseIngredients(parts[6]);

            return new Recipe(parts[0], calories, protein, carbs, fat, ingredients, parts[5]);
        }

        private List<Ingredient> ParseIngredients(string rawIngredients)
        {
            List<Ingredient> ingredients = new();

            if (string.IsNullOrWhiteSpace(rawIngredients))
            {
                return ingredients;
            }

            foreach (string item in rawIngredients.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length == 2 && double.TryParse(parts[1], out double quantity))
                {
                    ingredients.Add(new Ingredient(parts[0], quantity));
                }
            }

            return ingredients;
        }
    }
}