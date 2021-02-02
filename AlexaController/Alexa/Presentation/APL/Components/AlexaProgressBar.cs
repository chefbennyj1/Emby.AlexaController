namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaProgressBar
    {
        public object type => typeof(AlexaProgressBar);
        public int bufferValue { get; set; }
        public bool isLoading { get; set; }
        public ProgressBarType progressBarType { get; set; }
        public int progressValue { get; set; }
        public int totalValue { get; set; }
    }

    // ReSharper disable twice InconsistentNaming
    public enum ProgressBarType
    {
        determinate,
        indeterminate
    }
}
