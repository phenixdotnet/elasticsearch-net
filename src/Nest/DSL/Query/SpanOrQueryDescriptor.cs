﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Elasticsearch.Net;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface ISpanOrQuery
	{
		[JsonProperty(PropertyName = "clauses")]
		IEnumerable<ISpanQuery> Clauses { get; set; }
	}

	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public class SpanOrQueryDescriptor<T> : ISpanSubQuery, IQuery, ISpanOrQuery where T : class
	{
		IEnumerable<ISpanQuery> ISpanOrQuery.Clauses { get; set; }

		bool IQuery.IsConditionless
		{
			get
			{
				return !((ISpanOrQuery)this).Clauses.HasAny() 
					|| ((ISpanOrQuery)this).Clauses.Cast<IQuery>().All(q => q.IsConditionless);
			}
		}

		public SpanOrQueryDescriptor<T> Clauses(params Func<SpanQuery<T>, SpanQuery<T>>[] selectors)
		{
			selectors.ThrowIfNull("selector");
			var descriptors = (
				from selector in selectors 
				let span = new SpanQuery<T>() 
				select selector(span) into q 
				where !(q as IQuery).IsConditionless 
				select q
			).ToList();
			((ISpanOrQuery)this).Clauses = descriptors.HasAny() ? descriptors : null;
			return this;
		}
	}
}
