namespace MealPlannerApp
{
    public class Ingredient
    {
        public string Name { get; set; }
        public double Quantity { get; set; }

        public Ingredient(string name, double quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    }
}