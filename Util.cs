using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;

namespace SimpleDiscord
{
	/// <summary>
	/// Provides utility methods used throughout this program.
	/// </summary>
	static class Util
	{
		/// <summary>
		/// Creates JSON text using a <see cref="Utf8JsonWriter"/>.
		/// </summary>
		/// <param name="write">Writes the data to the JSON. The initial object start and the closing object end do not have to be written.</param>
		/// <returns>The written JSON text in UTF-8 encoding.</returns>
		/// <exception cref="InvalidOperationException">After <paramref name="write"/> was called, writing the end of the object produced invalid JSON.</exception>
		internal static ReadOnlyMemory<byte> CreateJson(Action<Utf8JsonWriter> write)
		{
			ArrayBufferWriter<byte> buffer = new();
			using Utf8JsonWriter writer = new(buffer);
			writer.WriteStartObject();
			write(writer);
			writer.WriteEndObject();
			writer.Flush();
			return buffer.WrittenMemory;
		}

		/// <summary>
		/// Creates JSON text using a <see cref="Utf8JsonWriter"/>.
		/// </summary>
		/// <param name="write">Writes the data to the JSON. The initial array start and the closing object end do not have to be written.</param>
		/// <returns>The written JSON text in UTF-8 encoding.</returns>
		/// <exception cref="InvalidOperationException">After <paramref name="write"/> was called, writing the end of the array produced invalid JSON.</exception>
		internal static ReadOnlyMemory<byte> CreateJsonArray(Action<Utf8JsonWriter> write)
		{
			ArrayBufferWriter<byte> buffer = new();
			using Utf8JsonWriter writer = new(buffer);
			writer.WriteStartArray();
			write(writer);
			writer.WriteEndArray();
			writer.Flush();
			return buffer.WrittenMemory;
		}

		/// <summary>
		/// Writes a JSON array of objects with a property name specified as a string as the key.
		/// </summary>
		/// <remarks>
		/// <para>If <paramref name="array"/> is <see langword="null"/>, nothing is written.</para>
		/// </remarks>
		/// <typeparam name="T">The type of object in the array.</typeparam>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="propertyName">The UTF-16 encoded property name of the JSON array to be transcoded and written as UTF-8.</param>
		/// <param name="array">The array to write. If <see langword="null"/>, nothing is written.</param>
		/// <param name="writeObjectValues">A delegate that writes one instance of <typeparamref name="T"/> to the <paramref name="writer"/>. It does not need to call <see cref="Utf8JsonWriter.WriteStartObject()"/> or <see cref="Utf8JsonWriter.WriteEndObject()"/>, because that will already be done for you.</param>
		/// <exception cref="ArgumentException"><paramref name="propertyName"/> is too large.</exception>
		/// <exception cref="InvalidOperationException">The depth of the JSON exceeds the maximum depth of 1,000.</exception>
		/// <exception cref="InvalidOperationException">Validation is enabled, and this write operation would produce invalid JSON.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is <see langword="null"/>.</exception>
		internal static void WriteObjectArray<T>(this Utf8JsonWriter writer, string propertyName, IEnumerable<T>? array, Action<T> writeObjectValues)
		{
			if (array is null) return;

			writer.WriteStartArray(propertyName);
			foreach (T element in array)
			{
				writer.WriteStartObject();
				writeObjectValues(element);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		/// <inheritdoc cref="WriteObjectArray{T}(Utf8JsonWriter, string, IEnumerable{T}?, Action{T})"/>
		internal static void WriteObjectArray<T>(this Utf8JsonWriter writer, string propertyName, IEnumerable<T>? array, Action<T, Utf8JsonWriter> writeObjectValues)
		{
			if (array is null) return;

			writer.WriteStartArray(propertyName);
			foreach (T element in array)
			{
				writer.WriteStartObject();
				writeObjectValues(element, writer);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		/// <summary>
		/// Throws an <see cref="ArgumentException"/> if a <see href="https://discord.com/developers/docs/reference#snowflakes">snowflake ID</see> is invalid.
		/// </summary>
		/// <param name="id">The string to test.</param>
		/// <param name="paramName">The name of the parameter to mention in the exception.</param>
		/// <param name="allowNull"><see langword="true"/> if this method should not throw if <paramref name="id"/> is <see langword="null"/>; <see langword="false"/> if this method should throw in that case.</param>
		/// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="id"/> is not a valid ID.</exception>
		internal static void ThrowIfInvalidId(string? id, string paramName, bool allowNull = false)
		{
			if (id is null)
			{
				if (allowNull) return;
				throw new ArgumentNullException(paramName);
			}
			if (id.Length == 0)
			{
				throw new ArgumentException($"{paramName} is an empty string.", paramName);
			}
			foreach (char @char in id)
			{
				if (@char is < '0' or > '9') throw new ArgumentException($"{paramName} is not a valid ID.", paramName);
			}
		}
	}
}
