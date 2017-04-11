using System;
using System.Net.Http;

namespace ScraperClientLib.Engine.Interventions
{
    public class InterventionResult
    {
        public InterventionResultState State { get; set; }
        public HttpRequestMessage IntermediateTask { get; set; }
        public Action Callback { get; set; }

        public InterventionResult(InterventionResultState state, HttpRequestMessage intermediateTask = null, Action callback = null)
        {
            State = state;
            IntermediateTask = intermediateTask;
            Callback = callback;
        }
    }
}
