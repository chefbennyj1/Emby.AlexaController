namespace AlexaController.Utils
{
    public class StringNormalization
    {
        public static string NormalizeText(string input)
        {
            /*
             * Alexa will understand the BrowseLibraryItem phrase:
             * "to show the movie {Movie} in the {Room}"
             * as the movie name with " in" at the end.
             * Example: "Iron man in"
             * Remove the rouge " in" if it exists.
             *
             */

            input = input.EndsWith(" in") ? input.Substring(0, input.Length - 3) : input;

            return input

                .Replace("&", " and")
                .Replace("@", "at")
                .Replace("ask home theater", "")
                .Replace(":", string.Empty)
                .Replace("show the ", string.Empty)
                .Replace("_", " ")
                .Replace("saga", "")
                .Replace("collection", "")
                .Replace("home theater", string.Empty);

        }
    }
}
