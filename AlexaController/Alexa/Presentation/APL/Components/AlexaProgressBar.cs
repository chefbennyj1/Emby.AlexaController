namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaProgressBar : VisualBaseComponent
    {
        public object type => nameof(AlexaProgressBar);
        public long bufferValue { get; set; }
        public bool isLoading { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ProgressBarType progressBarType { get; set; }
        public double progressValue { get; set; }
        public double totalValue { get; set; }
    }

    // ReSharper disable twice InconsistentNaming
    public enum ProgressBarType
    {
        determinate,
        indeterminate
    }
}
