namespace AlexaController.Utils
{
    public class StringNormalization
    {
        public static string ValidateSpeechQueryString(string input)
        {
            /*
             * Put common Alexa speech mis-recognitions here
             */
            try
            {
                input = input.EndsWith(" in") ? input.Substring(0, input.Length - 3) : input;
            }
            catch { }

            input = input.EndsWith(" junior") ? input.Replace("junior", "jr.") : input;
            //input = input.ToLowerInvariant().StartsWith("falcon") ? "The Falcon and the Winter Soldier" : input;
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
