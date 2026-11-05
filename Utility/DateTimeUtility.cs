namespace Letter_Request_Management_System.Utility
{
    public static class DateTimeUtility
    {
        public static string FormatLetterDate(DateTime date)
        {
            return date.ToString("dd MMMM yyyy");
        }

        public static string FormatJoinDate(DateTime? joinDate)
        {
            return joinDate.HasValue
                ? joinDate.Value.ToString("dd MMMM yyyy")
                : "Not available";
        }
    }
}