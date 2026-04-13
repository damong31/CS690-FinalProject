using System.Collections.Generic;
using MealPlannerApp;
using Xunit;

namespace MealPlannerApp.Tests
{
    public class WeeklyMealPlanTests
    {
        [Fact]
        public void SetMeals_ShouldOnlyKeepFirstThreeMeals()
        {
            // Arrange
            WeeklyMealPlan mealPlan = new WeeklyMealPlan();

            List<Recipe> recipes = new List<Recipe>
            {
                new Recipe("Meal 1", 100, 10, 10, 5, new List<Ingredient>(), "Breakfast"),
                new Recipe("Meal 2", 200, 20, 20, 10, new List<Ingredient>(), "Lunch"),
                new Recipe("Meal 3", 300, 30, 30, 15, new List<Ingredient>(), "Dinner"),
                new Recipe("Meal 4", 400, 40, 40, 20, new List<Ingredient>(), "Snack")
            };

            // Act
            mealPlan.SetMeals("Monday", recipes);
            List<Recipe> result = mealPlan.GetMeals("Monday");

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Meal 1", result[0].Name);
            Assert.Equal("Meal 2", result[1].Name);
            Assert.Equal("Meal 3", result[2].Name);
        }

        [Fact]
        public void GetMeals_InvalidDay_ShouldReturnEmptyList()
        {
            // Arrange
            WeeklyMealPlan mealPlan = new WeeklyMealPlan();

            // Act
            List<Recipe> result = mealPlan.GetMeals("NotADay");

            // Assert
            Assert.Empty(result);
        }
        [Fact]
public void NewWeeklyMealPlan_ShouldInitializeAllDaysAsEmpty()
{
    WeeklyMealPlan mealPlan = new WeeklyMealPlan();

    foreach (string day in WeeklyMealPlan.DaysOfWeek)
    {
        Assert.Empty(mealPlan.GetMeals(day));
    }
}

[Fact]
public void SetMeals_ValidDay_ShouldStoreMeals()
{
    WeeklyMealPlan mealPlan = new WeeklyMealPlan();

    List<Recipe> recipes = new List<Recipe>
    {
        new Recipe("Oatmeal", 250, 10, 40, 5, new List<Ingredient>(), "Breakfast")
    };

    mealPlan.SetMeals("Tuesday", recipes);

    List<Recipe> result = mealPlan.GetMeals("Tuesday");

    Assert.Single(result);
    Assert.Equal("Oatmeal", result[0].Name);
}
    }
}