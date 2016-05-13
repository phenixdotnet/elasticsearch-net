using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nest
{
	[JsonConverter(typeof(AttachmentConverter))]
	public class Attachment
	{
		/// <summary>
		/// The author.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// The base64 encoded content. Can be explicitly set
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// The length of the content before text extraction.
		/// </summary>
		[JsonProperty("content_length")]
		public long? ContentLength { get; set; }

		/// <summary>
		/// The content type of the attachment. Can be explicitly set
		/// </summary>
		[JsonProperty("content_type")]
		public string ContentType { get; set; }

		/// <summary>
		/// The date of the attachment.
		/// </summary>
		public DateTime? Date { get; set; }

		/// <summary>
		/// The keywords in the attachment.
		/// </summary>
		public string Keywords { get; set; }

		/// <summary>
		/// The language of the attachment. Can be explicitly set; this requires 
		/// <see cref="DetectLanguage"/> to be set to <c>true</c>
		/// </summary>
		public string Language { get; set; }

		/// <summary>
		/// Detect the language of the attachment. Language detection is 
		/// disabled by default.
		/// </summary>
		[JsonProperty("detect_language")]
		public bool DetectLanguage { get; set; }

		/// <summary>
		/// The name of the attachment. Can be explicitly set
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The title of the attachment.
		/// </summary>
		public string Title { get; set; }

		[JsonIgnore]
		public bool ContainsMetadata => !Name.IsNullOrEmpty() ||
		                                !ContentType.IsNullOrEmpty() ||
		                                !Language.IsNullOrEmpty() ||
										DetectLanguage;
	}

	internal class AttachmentConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var attachment = (Attachment)value;
			var topLevel = false;
			if (writer.WriteState == WriteState.Start)
			{
				topLevel = true;
				writer.WriteStartObject();
			}

			if (!attachment.ContainsMetadata)
			{
				if (topLevel)
				{
					writer.WritePropertyName("content");
				}

				writer.WriteValue(attachment.Content);
			}
			else
			{
				writer.WriteStartObject();
				writer.WritePropertyName("_content");
				writer.WriteValue(attachment.Content);

				if (!string.IsNullOrEmpty(attachment.Name))
				{
					writer.WritePropertyName("_name");
					writer.WriteValue(attachment.Name);
				}

				if (!string.IsNullOrEmpty(attachment.ContentType))
				{
					writer.WritePropertyName("_content_type");
					writer.WriteValue(attachment.ContentType);
				}

				if (!string.IsNullOrEmpty(attachment.Language))
				{
					writer.WritePropertyName("_language");
					writer.WriteValue(attachment.Language);
				}

				if (attachment.DetectLanguage)
				{
					writer.WritePropertyName("_detect_language");
					writer.WriteValue(attachment.DetectLanguage);
				}

				writer.WriteEndObject();
			}

			if (topLevel)
			{
				writer.WriteEndObject();
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.String)
			{
				return new Attachment { Content = (string)reader.Value };
			}

			if (reader.TokenType == JsonToken.StartObject)
			{
				var attachment = new Attachment();
				while (reader.Read())
				{
					if (reader.TokenType == JsonToken.PropertyName)
					{
						var propertyName = (string)reader.Value;
						switch (propertyName)
						{
							case "_content":
								attachment.Content = reader.ReadAsString();
								break;
							case "_name":
								attachment.Name = reader.ReadAsString();
								break;
							case "_content_type":
								attachment.ContentType = reader.ReadAsString();
								break;
							case "_language":
								attachment.Language = reader.ReadAsString();
								break;							
							case "_detect_language":
								attachment.DetectLanguage = reader.ReadAsBoolean() ?? false;
								break;
						}
					}
					if (reader.TokenType == JsonToken.EndObject)
					{
						break;
					}
				}
				return attachment;
			}
			return null;
		}

		public override bool CanConvert(Type objectType) => objectType == typeof(Attachment);
	}
}
