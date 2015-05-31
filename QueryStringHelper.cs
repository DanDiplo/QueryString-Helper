using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;

namespace Diplo.Helpers
{
	/// <summary>
	/// Used to parse and manipulate an HTTP querystring [GET request]
	/// </summary>
	public class QueryStringHelper
	{
		/// <summary>
		/// Gets the current query string collection
		/// </summary>
		public NameValueCollection QueryStringCollection { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringHelper"/> class using the passed querystring
		/// </summary>
		/// <param name="qs">The querystring to use</param>
		public QueryStringHelper(string qs)
		{
			this.QueryStringCollection = HttpUtility.ParseQueryString(qs);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringHelper"/> class using the querystring in the current HttpContext.Request
		/// </summary>
		public QueryStringHelper()
		{
			if (HttpContext.Current == null || HttpContext.Current.Request == null)
				throw new NullReferenceException("There is no HttpContext or the Request is null");

			this.QueryStringCollection = new NameValueCollection(HttpContext.Current.Request.QueryString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringHelper"/> class from an HttpRequest
		/// </summary>
		/// <param name="request">The HTTP request to use</param>
		public QueryStringHelper(HttpRequest request)
		{
			this.QueryStringCollection = new NameValueCollection(request.QueryString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringHelper"/> class from a NameValueCollection
		/// </summary>
		/// <param name="nvCollection">The name value collection</param>
		public QueryStringHelper(NameValueCollection nvCollection)
		{
			this.QueryStringCollection = new NameValueCollection(nvCollection);
		}

		/// <summary>
		/// Checks whether a key with the name exists
		/// </summary>
		/// <param name="name">The name to check</param>
		/// <returns>True if it exists or false if it doesn't</returns>
		public bool NameExists(string name)
		{
			return !String.IsNullOrEmpty(GetValueByName(name));
		}

		/// <summary>
		/// Gets a querystring value by name
		/// </summary>
		/// <param name="name">The name of the key to get</param>
		/// <returns>The string value associated with the key</returns>
		public string GetValueByName(string name)
		{
			return QueryStringCollection.Get(name);
		}

        /// <summary>
        /// Gets a string value by name and if it is null or empty returns a default value instead
        /// </summary>
        /// <param name="name">The value</param>
        /// <param name="defaultValue">The default value to return when null or empty</param>
        /// <returns>A value</returns>
        public string GetValueByName(string name, string defaultValue)
        {
            string value = QueryStringCollection.Get(name);

            if (!String.IsNullOrEmpty(value))
                return value;
            else
                return defaultValue;
        }

		/// <summary>
		/// Gets a querystring value and converts it to the destination type
		/// </summary>
		/// <typeparam name="T">The type to convert it to</typeparam>
        /// <param name="name">The name of the key to get from the query string</param>
		/// <returns>The value of the key converted to a type of T</returns>
		/// <exception cref="System.FormatException">Throws a System.FormatException if the type cannot be converted</exception>
		public T GetValueByName<T>(string name) where T : struct
		{
			string value = GetValueByName(name);

			if (String.IsNullOrEmpty(value))
			{
                return default(T);
			}

			return (T)Convert.ChangeType(value, typeof(T));
		}

        /// <summary>
        /// Gets a querystring value and converts it to the destination type using the converter
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <param name="name">The name of the key to get from the query string</param>
        /// <param name="converter">The delegate to perform the conversion</param>
        /// <param name="throwOnError">Whether to throw an exception when there is an error converting (default = false)</param>
        /// <returns>The value of the key converted to a type of T</returns>
        /// <example>var cats = qs.GetMultipleValuesByName&lt;int&gt;("cat", x => Convert.ToInt32(x))</example>
        public T GetValueByName<T>(string name, Func<string, T> converter, bool throwOnError = false)
        {
            if (converter == null)
                throw new ArgumentNullException("converter", "The delegate converter cannot be null");

            string value = GetValueByName(name);

            if (String.IsNullOrEmpty(value) && throwOnError == false)
            {
                return default(T);
            }

            try
            {
                return converter(value);
            }
            catch (Exception)
            {
                if (throwOnError)
                    throw;
                 else
                    return default(T);
            }
        }

		/// <summary>
		/// Gets a querystring value and converts it to the destination type. If it cannot be converted then the defaultValue is returned instead.
		/// </summary>
		/// <typeparam name="T">The type to convert it to</typeparam>
		/// <param name="name">The name of the key to get</param>
		/// <param name="defaultValue">The default value to return if it cannot be converted</param>
		/// <returns>The value of the key converted to a type of T or the default value</returns>
        public T GetValueByName<T>(string name, T defaultValue) where T : struct
		{
			if (String.IsNullOrEmpty(GetValueByName(name)))
			{
				return defaultValue;
			}

			try
			{
				return GetValueByName<T>(name);
			}
			catch (FormatException)
			{
				return defaultValue;
			}
		}

        /// <summary>
        /// Gets the named querystring value multiple times as a string collection
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>An enumerable (which can be empty if there are no matches)</returns>
        public IEnumerable<string> GetMultipleValuesByName(string name)
        {
            var vals = QueryStringCollection.GetValues(name);

            if (vals == null)
            {
                return Enumerable.Empty<string>();
            }
            else
            {
                return vals;
            }
        }

        /// <summary>
        /// Gets multiple query string values for the given name (eg. ?cat=123&amp;cat=434&amp;cat=454)
        /// </summary>
        /// <typeparam name="T">The Type you want to convert the value to</typeparam>
        /// <param name="name">The name of the query string</param>
        /// <returns>An enumerable of type T. If a value cannot be converted it is not returned.</returns>
        public IEnumerable<T> GetMultipleValuesByName<T>(string name) where T : struct
        {
            var vals = QueryStringCollection.GetValues(name);

            if (vals == null)
            {
                return Enumerable.Empty<T>();
            }
            else
            {
                var list = new List<T>();

                foreach (var value in vals)
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        T converted;

                        try
                        {
                            converted = (T)Convert.ChangeType(value, typeof(T));
                            list.Add(converted);
                        }
                        catch (FormatException)
                        {
                            // do nothing
                        }
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// Gets multiple query string values for the given name (eg. ?cat=123&amp;cat=434&amp;cat=454)
        /// </summary>
        /// <typeparam name="T">The Type you want to convert the value to</typeparam>
        /// <param name="name">The name of the query string</param>
        /// <param name="defaultValue">A default value that is returned if it cannot be converted</param>
        /// <returns>An enumerable of type T. If a value cannot be converted then default value is returned instead.</returns>
        public IEnumerable<T> GetMultipleValuesByName<T>(string name, T defaultValue) where T : struct
        {
            var vals = QueryStringCollection.GetValues(name);

            if (vals == null)
            {
                return Enumerable.Empty<T>();
            }
            else
            {
                var list = new List<T>();

                foreach (var value in vals)
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        T converted;

                        try
                        {
                            converted = (T)Convert.ChangeType(value, typeof(T));
                        }
                        catch (FormatException)
                        {
                            converted = defaultValue;
                        }

                        list.Add(converted);
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// Gets multiple query string values for the given name (eg. ?cat=123&amp;cat=434&amp;cat=454)
        /// </summary>
        /// <typeparam name="T">The Type you want to convert the value to</typeparam>
        /// <param name="name">The name of the query string</param>
        /// <param name="converter">A delegate to perform the conversion</param>
        /// <param name="throwOnError">Whether to throw an exception when an error occurs during conversion</param>
        /// <returns>An enumerable of type T</returns>
        public IEnumerable<T> GetMultipleValuesByName<T>(string name, Func<string, T> converter, bool throwOnError = false)
        {
            if (converter == null)
                throw new ArgumentNullException("converter", "The delegate converter cannot be null");

            var vals = QueryStringCollection.GetValues(name);

            if (vals == null)
            {
                return Enumerable.Empty<T>();
            }
            else
            {
                var list = new List<T>();

                foreach (var value in vals)
                {
                    try
                    {
                        list.Add(converter(value));
                    }
                    catch (Exception)
                    {
                        if (throwOnError)
                            throw;
                    }
                }

                return list;
            }
        }

		/// <summary>
		/// Adds the specified name and value to the querystring. If the value already exists it is added with a comma.
		/// </summary>
		/// <param name="name">The name to add</param>
		/// <param name="value">The value to add</param>
		public void Add(string name, string value)
		{
			QueryStringCollection.Add(name, value);
		}

		/// <summary>
		/// Adds the specified name and value (converted to a string) to the querystring. If the value already exists it is added with a comma.
		/// </summary>
		/// <param name="name">The name to add</param>
		/// <param name="value">The value to add</param>
		public void Add(string name, object value)
		{
			this.Add(name, value.ToString());
		}

		/// <summary>
		/// Adds a new value or replaces it if a key with that value already exists
		/// </summary>
		/// <param name="name">The name to add</param>
		/// <param name="value">The value to add</param>
		public void AddOrReplace(string name, string value)
		{
			QueryStringCollection.Set(name, value);
		}


		/// <summary>
		/// Adds a new value or replaces it if a key with that value already exists. The value is converted to a string.
		/// </summary>
		/// <param name="name">The name to add</param>
		/// <param name="value">The value to add</param>
		public void AddOrReplace(string name, object value)
		{
			AddOrReplace(name, value.ToString());
		}

		/// <summary>
		/// Removes the key with the name
		/// </summary>
		/// <param name="name">The name of the key to remove</param>
		public void RemoveByName(string name)
		{
			QueryStringCollection.Remove(name);
		}

		/// <summary>
		/// Removes all the keys that have a specific value
		/// </summary>
		/// <param name="value">The value to check for</param>
		/// <returns>A count of the keys removed</returns>
		public int RemoveByValue(string value)
		{
			int removed = 0;

			for (int i = QueryStringCollection.Keys.Count - 1; i > 0; i--)
			{
				if (QueryStringCollection.Get(i) == value)
				{
					QueryStringCollection.Remove(QueryStringCollection.Keys[i]);
					removed++;
				}
			}

			return removed;
		}

		/// <summary>
		/// Clears this querystring
		/// </summary>
		public void Clear()
		{
			QueryStringCollection.Clear();
		}

		/// <summary>
		/// Returns a count of the keys in the querystring
		/// </summary>
		/// <returns>A positive integer or 0 if no values</returns>
		public int Count()
		{
			return QueryStringCollection.Count;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current querystring
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return GetQueryString();
		}

		/// <summary>
		/// Gets the querystring as a string of name and value pairs delimited with an ampersand
		/// </summary>
		/// <returns>The querystring as a string</returns>
		public string GetQueryString()
		{
			return GetQueryString(false);
		}

		/// <summary>
		/// Gets the querystring as a string of name and value pairs delimited with an ampersand
		/// </summary>
		/// <param name="removeEmpty">If set to <c>true</c> keys with no value are removed</param>
		/// <returns>The querystring as a string with empty keys removed</returns>
		public string GetQueryString(bool removeEmpty)
		{
			StringBuilder qs = new StringBuilder();

			int len = QueryStringCollection.Keys.Count;

			for (int i = 0; i < len; i++)
			{
				string val = QueryStringCollection.Get(i);
				if (String.IsNullOrEmpty(val) && removeEmpty)
					continue;

				qs.AppendFormat("{0}={1}", HttpUtility.UrlEncode(QueryStringCollection.GetKey(i)), HttpUtility.UrlEncode(QueryStringCollection.Get(i)));
				if (i < len - 1)
					qs.Append("&");
			}

			return qs.ToString();
		}

		/// <summary>
		/// Converts the querystring NameValueCollection to a generic Dictionary that can be easily interated over
		/// </summary>
		/// <returns>A Dictionary of string name and value pairs</returns>
		public Dictionary<string, string> ToDictionary()
		{
			return this.QueryStringCollection.Cast<string>().Select(s => new { Key = s, Value = this.QueryStringCollection[s] }).ToDictionary(p => p.Key, p => p.Value);
		}
	}
}
