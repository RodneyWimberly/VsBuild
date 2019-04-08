using Microsoft.Build.Framework;
using System.IO;
using System.Xml.Serialization;

namespace VsBuild.MSBuildLogger
{
    public static class BuildEventArgsExtensions
    {
        public static string Serialize(this BuildEventArgs toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T Deserialize<T>(this string toDeserialize) where T : BuildEventArgs
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader textReader = new StringReader(toDeserialize))
            {
                return xmlSerializer.Deserialize(textReader) as T;
            }
        }
    }
}
