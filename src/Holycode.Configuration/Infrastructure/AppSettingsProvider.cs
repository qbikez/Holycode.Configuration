using Microsoft.Extensions.Configuration.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.Configuration.Xml
{
    /// <summary>
    /// based on https://github.com/aspnet/Configuration/blob/1.0.0/src/Microsoft.Extensions.Configuration.Xml/XmlConfigurationProvider.cs
    /// </summary>
    public class AppSettingsProvider : XmlConfigurationProvider
    {
        private const string NameAttributeKey = "key";
        private const string ValueAttributeKey = "value";
        private const string expectedRoot = "appSettings";
        internal XmlDocumentDecryptor Decryptor { get; set; } = XmlDocumentDecryptor.Instance;
        public AppSettingsProvider(AppSettingsConfigurationSource source) : base(source)
        {

        }

        public override void Load()
        {
            base.Load();
        }

        public override void Load(Stream stream)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var readerSettings = new XmlReaderSettings()
            {
                CloseInput = false, // caller will close the stream
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (var reader = Decryptor.CreateDecryptingXmlReader(stream, readerSettings))
            {
                var prefixStack = new Stack<string>();

                SkipUntilRootElement(reader);

                if (reader.Name != expectedRoot) throw new Exception($"expected root '{expectedRoot}', got '{reader.Name}' {GetLineInfo(reader)}");
                // We process the root element individually since it doesn't contribute to prefix 
                //ProcessAttributes(reader, prefixStack, data, null);
                ProcessAttributes(reader, prefixStack, data, null);

                var preNodeType = reader.NodeType;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            //prefixStack.Push(reader.LocalName);
                            //ProcessAttributes(reader, prefixStack, data, AddNamePrefix);
                            ProcessAttributes(reader, prefixStack, data, (r, s, d, w) =>
                            {
                                AddNamePrefix(r, s, d, w);
                                AddValue(r, s, d, w);
                            });

                            // If current element is self-closing
                            //if (reader.IsEmptyElement)
                            //{
                            //    prefixStack.Pop();
                            //}
                            break;

                        case XmlNodeType.EndElement:
                            if (prefixStack.Any())
                            {
                                // If this EndElement node comes right after an Element node,
                                // it means there is no text/CDATA node in current element
                                if (preNodeType == XmlNodeType.Element)
                                {
                                    var key = ConfigurationPath.Combine(prefixStack.Reverse());
                                    data[key] = string.Empty;
                                }

                                prefixStack.Pop();
                            }
                            break;

                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            {
                                var key = ConfigurationPath.Combine(prefixStack.Reverse());
                                if (data.ContainsKey(key))
                                {
                                    throw new FormatException($"KeyIsDuplicated: {key} {GetLineInfo(reader)}");
                                }

                                data[key] = reader.Value;
                                break;
                            }
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Comment:
                        case XmlNodeType.Whitespace:
                            // Ignore certain types of nodes
                            break;

                        default:
                            throw new FormatException($"UnsupportedNodeType: {reader.NodeType} {GetLineInfo(reader)}");
                    }
                    preNodeType = reader.NodeType;
                    // If this element is a self-closing element,
                    // we pretend that we just processed an EndElement node
                    // because a self-closing element contains an end within itself
                    if (preNodeType == XmlNodeType.Element &&
                        reader.IsEmptyElement)
                    {
                        preNodeType = XmlNodeType.EndElement;
                    }
                }
            }

            Data = data;
        }

        private void SkipUntilRootElement(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.XmlDeclaration &&
                    reader.NodeType != XmlNodeType.ProcessingInstruction)
                {
                    break;
                }
            }
        }

        private void ProcessAttributes(XmlReader reader, Stack<string> prefixStack, IDictionary<string, string> data,
          Action<XmlReader, Stack<string>, IDictionary<string, string>, XmlWriter> act, XmlWriter writer = null)
        {
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                // If there is a namespace attached to current attribute
                if (!string.IsNullOrEmpty(reader.NamespaceURI))
                {
                    throw new FormatException($"NamespaceIsNotSupported: {GetLineInfo(reader)}");
                }

                act(reader, prefixStack, data, writer);
            }

            // Go back to the element containing the attributes we just processed
            reader.MoveToElement();
        }


        private string GetLineInfo(XmlReader reader)
        {
            var lineInfo = reader as IXmlLineInfo;
            return lineInfo == null ? string.Empty :
                $"in {Source.Path} at line:{lineInfo.LineNumber}, col:{lineInfo.LinePosition}";
        }

        // The special attribute "Name" only contributes to prefix
        // This method adds a prefix if current node in reader represents a "Name" attribute
        private static void AddNamePrefix(XmlReader reader, Stack<string> prefixStack,
            IDictionary<string, string> data, XmlWriter writer)
        {
            if (!string.Equals(reader.LocalName, NameAttributeKey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var val = reader.Value.Replace('.',':');
            // If current element is not root element
            if (prefixStack.Any())
            {
                var lastPrefix = prefixStack.Pop();
                prefixStack.Push(ConfigurationPath.Combine(lastPrefix, val));
            }
            else
            {
                prefixStack.Push(val);
            }
        }

        // Common attributes contribute to key-value pairs
        // This method adds a key-value pair if current node in reader represents a common attribute
        private  void AddValue(XmlReader reader, Stack<string> prefixStack,
            IDictionary<string, string> data, XmlWriter writer)
        {
            if (!string.Equals(reader.LocalName, ValueAttributeKey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var key = ConfigurationPath.Combine(prefixStack.Reverse());
            if (data.ContainsKey(key))
            {
                throw new FormatException($"KeyIsDuplicated: {key} {GetLineInfo(reader)}");
            }

            data[key] = reader.Value;
            prefixStack.Pop();
        }
    }

    /// <summary>
    /// An XML file based <see cref="FileConfigurationSource"/>.
    /// </summary>
    public class AppSettingsConfigurationSource : XmlConfigurationSource
    {
        /// <summary>
        /// Builds the <see cref="XmlConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="XmlConfigurationProvider"/></returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new AppSettingsProvider(this);
        }
    }


}

namespace Microsoft.Extensions.Configuration
{
    public static class AppSettingsConfigurationExtensions
    {
        /// <summary>
        /// Adds the app settings XML configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddXmlAppSettings(this IConfigurationBuilder builder, string path, bool optional = true, bool reloadOnChange = false)
        {
            return AddXmlAppSettings(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        /// <summary>
        /// Adds app settings XML configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddXmlAppSettings(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"InvalidFilePath {nameof(path)}");
            }

            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new AppSettingsConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            return builder;
        }
    }
}
