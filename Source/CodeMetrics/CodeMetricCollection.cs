namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;

	public sealed class CodeMetricCollection : CollectionBase
    {
        public CodeMetricCollection()
        {
		}

		public void Add(CodeMetric value)
		{
			this.List.Add(value);
		}

		public int IndexOf(CodeMetric value)
		{
			return this.List.IndexOf(value);
		}

        public CodeMetric this[int index]
        {
            get 
			{ 
				return this.List[index] as CodeMetric; 
			}
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            
			CodeMetric codeMetric = value as CodeMetric;
			if (codeMetric == null)
			{
				throw new ArgumentNullException("value");
			}
        }

    }
}
