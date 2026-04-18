using System.Collections.Generic;

namespace MealPlannerApp
{
    public class Recipe
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
        public List<Ingredient> Ingredients { get; set; }

        public Recipe(string name, int calories, int protein, int carbs, int fat, List<Ingredient> ingredients, string category)
        {
            Name = name;
            Calories = calories;
            Protein = protein;
            Carbs = carbs;
            Fat = fat;
            Ingredients = ingredients;
            Category = category;
        }
    }
}