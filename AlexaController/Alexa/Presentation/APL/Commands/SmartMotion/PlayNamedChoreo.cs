using System.Diagnostics.CodeAnalysis;

namespace AlexaController.Alexa.Presentation.APL.Commands.SmartMotion
{
    // ReSharper disable once UnusedType.Global
    public class PlayNamedChoreo : ICommand
    {
        public object type => "SmartMotion:PlayNamedChoreo";
        public Choreo name { get; set; }
        public string when { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Choreo
    {
        ScreenImpactCenter,
        ClockwiseMediumSweep,
        CounterClockwiseSlowSweep,
        MixedExpressiveShakes
    }
}
