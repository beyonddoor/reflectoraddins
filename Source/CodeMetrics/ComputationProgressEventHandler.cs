using System;

namespace Reflector.CodeMetrics
{
    public sealed class ComputationProgressEventArgs : EventArgs
    {
		private int percentComplete;

        public ComputationProgressEventArgs(int count, int totalCount)
        {
            this.percentComplete = (int)((double)count / totalCount*100);
        }

        public int PercentComplete
        {
			get 
			{ 
				return this.percentComplete; 
			}
        }
    }

    public delegate void ComputationProgressEventHandler(object sender, ComputationProgressEventArgs e);
}
