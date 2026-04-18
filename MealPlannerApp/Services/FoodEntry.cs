namespace MealPlannerApp
{
    public class FoodEntry
    {
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }

        public FoodEntry(string name, int calories, int protein, int carbs, int fat)
        {
            Name = name;
            Calories = calories;
            Protein = protein;
            Carbs = carbs;
            Fat = fat;
        }
    }
}