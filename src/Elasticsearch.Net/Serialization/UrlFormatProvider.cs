using Elasticsearch.Net.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Serialization
{
	class UrlFormatProvider : IFormatProvider, ICustomFormatter
	{
		private readonly IConnectionConfigurationValues _settings;

		public UrlFormatProvider(IConnectionConfigurationValues settings)
		{
			_settings = settings;
		}

		public object GetFormat(Type formatType) => formatType == typeof(ICustomFormatter) ? this : null;

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (arg == null)
				throw new ArgumentNullException();
			if (format == "r")
				return arg.ToString();
			return this.GetStringValue(arg);
		}

		public string GetStringValue(object valueType)
		{
			var s = valueType as string;
			if (s != null)
				return s;

			var ss = valueType as string[];
			if (ss != null)
				return string.Join(",", ss);

			var pns = valueType as IEnumerable<object>;
			if (pns != null)
				return string.Join(",", pns.Select(AttemptTheRightToString));

			var e = valueType as Enum;
			if (e != null) return e.GetStringValue();

			if (valueType is bool)
				return ((bool)valueType) ? "true" : "false";

			return AttemptTheRightToString(valueType);
		}
		
		public string AttemptTheRightToString(object value)
		{
			var explicitImplementation = this.QueryStringValueType(value as IQueryStringValue);
			if (explicitImplementation != null) return explicitImplementation;
			

			return value.ToString();
		}

		public string QueryStringValueType(IQueryStringValue value)
		{
			if (value == null) return null;
			return value.ToQueryStringValue(this._settings);
		}


	}
}