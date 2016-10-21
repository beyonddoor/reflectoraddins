namespace Reflector.CodeMetrics
{
	using System;

	public delegate void CodeMetricEventHandler(object sender, CodeMetricEventArgs e);

	public sealed class CodeMetricEventArgs  : EventArgs
    {
        private CodeMetric codeMetric;

		public CodeMetricEventArgs(CodeMetric codeMetric)
        {
			if (codeMetric == null)
			{
				throw new ArgumentNullException("codeMetric");
			}

            this.codeMetric = codeMetric;
        }

        public CodeMetric CodeMetric
        {
            get 
			{ 
				return this.codeMetric; 
			}
        }
    }
}
