namespace AlexaController.Utils
{
    public class StringNormalization
    {
        public static string ValidateSpeechQueryString(string input)
        {
            /*
             * Put common Alexa speech mis-recognitions here
             */

            input = input.EndsWith(" in") ? input.Substring(0, input.Length - 3) : input;
            input = input.EndsWith(" junior") ? input.Replace("junior", "jr.") : input;
            return input
                .Replace("&", " and")
                .Replace("@", "at")
                .Replace("ask home theater", "")
                .Replace(":", string.Empty)
                .Replace("show the ", string.Empty)
                .Replace("saga", "")
                .Replace("collection", "")
                .Replace("1 division", "wandavision")
                .Replace("home theater", string.Empty);
        }
    }
}
