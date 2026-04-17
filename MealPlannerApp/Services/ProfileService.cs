using System;

namespace MealPlannerApp
{
    public class ProfileService
    {
        private const string ProfileFilePath = "userProfile.txt";
        private const string DefaultDay = "Monday";

        private readonly FileService _fileService;

        public ProfileService(FileService fileService)
        {
            _fileService = fileService;
        }

        public UserProfile LoadOrCreateUserProfile()
        {
            if (_fileService.Exists(ProfileFilePath) && TryLoadUserProfile(out UserProfile? profile))
            {
                Console.WriteLine("Profile loaded.");
                return profile!;
            }

            UserProfile newProfile = CreateUserProfile();
            SaveUserProfile(newProfile);
            return newProfile;
        }

        public void SaveUserProfile(UserProfile profile)
        {
            _fileService.WriteAllLines(ProfileFilePath, new[]
            {
                profile.Name,
                profile.CalorieGoal.ToString(),
                profile.ProteinGoal.ToString(),
                profile.CarbGoal.ToString(),
                profile.FatGoal.ToString(),
                profile.CurrentDay
            });
        }

        private bool TryLoadUserProfile(out UserProfile? profile)
        {
            profile = null;

            try
            {
                string[] lines = _fileService.ReadAllLines(ProfileFilePath);

                if (lines.Length >= 6)
                {
                    profile = new UserProfile(
                        lines[0],
                        int.Parse(lines[1]),
                        int.Parse(lines[2]),
                        int.Parse(lines[3]),
                        int.Parse(lines[4]),
                        lines[5]);
                    return true;
                }

                if (lines.Length >= 5)
                {
                    profile = new UserProfile(
                        lines[0],
                        int.Parse(lines[1]),
                        int.Parse(lines[2]),
                        int.Parse(lines[3]),
                        int.Parse(lines[4]),
                        DefaultDay);

                    SaveUserProfile(profile);
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("Profile file was invalid. Creating a new one.");
            }

            return false;
        }

        private UserProfile CreateUserProfile()
        {
            Console.WriteLine("\n---- Set Up Your Profile ----");

            string name = ReadRequiredText("Enter your name: ", "User");
            int calories = ReadNonNegativeInt("Calories: ");
            int protein = ReadNonNegativeInt("Protein: ");
            int carbs = ReadNonNegativeInt("Carbs: ");
            int fat = ReadNonNegativeInt("Fat: ");

            Console.WriteLine("Profile created.");
            return new UserProfile(name, calories, protein, carbs, fat, DefaultDay);
        }

        private string ReadRequiredText(string message, string fallbackValue)
        {
            Console.Write(message);
            string input = Console.ReadLine() ?? string.Empty;
            return string.IsNullOrWhiteSpace(input) ? fallbackValue : input.Trim();
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
    }
}