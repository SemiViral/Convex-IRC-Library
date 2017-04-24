#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

#endregion

namespace Convex {
    public static class Extensions {
        /// <summary>
        ///     Obtain HTTP response from a GET request
        /// </summary>
        /// <returns>GET response</returns>
        public static string HttpGet(this string instance) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(instance);
            request.Method = "GET";

            using (HttpWebResponse httpr = (HttpWebResponse)request.GetResponse()) {
                return new StreamReader(httpr.GetResponseStream()).ReadToEnd();
            }
        }

        /// <summary>
        ///     Splits a string into seperate parts
        /// </summary>
        /// <param name="maxLength">max length of individual strings to split</param>
        public static IEnumerable<string> Split(this string instance, int maxLength) {
            for (int i = 0; i < instance.Length; i += maxLength)
                yield return instance.Substring(i, Math.Min(maxLength, instance.Length - i));
        }

        public static string DeliminateSpaces(this string str) {
            StringBuilder deliminatedSpaces = new StringBuilder();
            bool isSpace = false;

            // using for loop to increase speed
            for (int i = 0; i < str.Length; i++) {
                if (str[i].Equals(' ')) {
                    if (isSpace)
                        continue;

                    isSpace = true;
                } else {
                    isSpace = false;
                }

                deliminatedSpaces.Append(str[i]);
            }

            return deliminatedSpaces.ToString();
        }
    }
}