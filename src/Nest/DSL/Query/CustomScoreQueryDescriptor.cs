﻿using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface ICustomScoreQuery
	{
		[JsonProperty(PropertyName = "lang")]
		string Lang { get; set; }

		[JsonProperty(PropertyName = "script")]
		string Script { get; set; }

		[JsonProperty(PropertyName = "params")]
		[JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
		Dictionary<string, object> Params { get; set; }

		[JsonProperty(PropertyName = "query")]
		IQueryDescriptor Query { get; set; }
	}

	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public class CustomScoreQueryDescriptor<T> : IQuery, ICustomScoreQuery where T : class
	{
		string ICustomScoreQuery.Lang { get; set; }

		string ICustomScoreQuery.Script { get; set; }

		Dictionary<string, object> ICustomScoreQuery.Params { get; set; }

		IQueryDescriptor ICustomScoreQuery.Query { get; set; }

		bool IQuery.IsConditionless
		{
			get
			{
				return ((ICustomScoreQuery)this).Query == null || ((ICustomScoreQuery)this).Query.IsConditionless;
			}
		}

		public CustomScoreQueryDescriptor<T> Lang(string lang)
		{
			((ICustomScoreQuery)this).Lang = lang;
			return this;
		}

		public CustomScoreQueryDescriptor<T> Query(Func<QueryDescriptor<T>, BaseQuery> querySelector)
		{
			querySelector.ThrowIfNull("querySelector");
			var query = new QueryDescriptor<T>();
			var q = querySelector(query);

			((ICustomScoreQuery)this).Query = q;
			return this;
		}

		/// <summary>
		/// Scripts are cached for faster execution. If the script has parameters that it needs to take into account, it is preferable to use the same script, and provide parameters to it:
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>
		public CustomScoreQueryDescriptor<T> Script(string script)
		{
			((ICustomScoreQuery)this).Script = script;
			return this;
		}

		public CustomScoreQueryDescriptor<T> Params(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> paramDictionary)
		{
			paramDictionary.ThrowIfNull("paramDictionary");
			((ICustomScoreQuery)this).Params = paramDictionary(new FluentDictionary<string, object>());
			return this;
		}
	}
}
