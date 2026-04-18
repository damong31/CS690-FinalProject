namespace MealPlannerApp
{
    public class UserProfile
    {
        public string Name { get; set; }
        public int CalorieGoal { get; set; }
        public int ProteinGoal { get; set; }
        public int CarbGoal { get; set; }
        public int FatGoal { get; set; }
        public string CurrentDay { get; set; }

        public UserProfile(string name, int calorieGoal, int proteinGoal, int carbGoal, int fatGoal, string currentDay)
        {
            Name = name;
            CalorieGoal = calorieGoal;
            ProteinGoal = proteinGoal;
            CarbGoal = carbGoal;
            FatGoal = fatGoal;
            CurrentDay = currentDay;
        }
    }
}