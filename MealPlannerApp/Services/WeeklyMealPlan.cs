using System.Collections.Generic;
using System.Linq;

namespace MealPlannerApp
{
    public class WeeklyMealPlan
    {
        public static readonly string[] DaysOfWeek =
        {
            "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
        };

        public Dictionary<string, List<Recipe>> Meals { get; }

        public WeeklyMealPlan()
        {
            Meals = DaysOfWeek.ToDictionary(day => day, _ => new List<Recipe>());
        }

        public void SetMeals(string day, List<Recipe> recipes)
        {
            Meals[day] = recipes.Take(3).ToList();
        }

        public List<Recipe> GetMeals(string day)
        {
            return Meals.TryGetValue(day, out List<Recipe>? meals)
                ? meals
                : new List<Recipe>();
        }
    }
}