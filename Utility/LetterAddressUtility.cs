namespace Letter_Request_Management_System.Utilities
{
    public static class LetterAddressUtility
    {
        public static string CompanyName =>
            "SriLankan Airlines Limited";

        public static string[] CompanyAddress =>
        [
            "Airline Centre",
            "Bandaranaike International Airport",
            "Katunayake",
            "Sri Lanka"
        ];

        public static string GetCompanyAddress()
        {
            return string.Join(
                Environment.NewLine,
                CompanyName,
                string.Join(Environment.NewLine, CompanyAddress));
        }
    }
}