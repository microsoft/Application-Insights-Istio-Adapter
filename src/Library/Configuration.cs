namespace Microsoft.IstioMixerPlugin.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    internal class Configuration
    {
        private readonly XElement configuration;

        public string Host {
            get
            {
                try
                {
                    return this.configuration.Element("Host").Value;
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public int? Port
        {
            get
            {
                try
                {
                    string value = this.configuration.Element("Port").Value;

                    if (int.TryParse(value, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public string HttpListenerPrefix
        {
            get
            {
                try
                {
                    return this.configuration.Element("WebServer")?.Element("HttpListenerPrefix")?.Value;
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public string InstrumentationKey
        {
            get
            {
                try
                {
                    return this.configuration.Element("InstrumentationKey").Value;
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public string LiveMetricsStreamAuthenticationApiKey
        {
            get
            {
                try
                {
                    return this.configuration.Element("LiveMetricsStreamAuthenticationApiKey").Value;
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public string[] Watchlist_Namespaces
        {
            get
            {
                try
                {
                    return this.configuration.Element("Watchlist")?.Element("Namespaces")?.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(ns => ns.Trim()).ToArray() ?? new string[0];
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public string[] Watchlist_IgnoredNamespaces
        {
            get
            {
                try
                {
                    return this.configuration.Element("Watchlist")?.Element("IgnoredNamespaces")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ns => ns.Trim()).ToArray() ?? new string[0];
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public bool? AdaptiveSampling_Enabled
        {
            get
            {
                try
                {
                    string value = this.configuration.Element("AdaptiveSampling").Attribute("Enabled").Value;

                    if (bool.TryParse(value, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public int? AdaptiveSampling_MaxEventsPerSecond
        {
            get
            {
                try
                {
                    string value = this.configuration.Element("AdaptiveSampling").Element("MaxEventsPerSecond").Value;

                    if (int.TryParse(value, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public int? AdaptiveSampling_MaxOtherItemsPerSecond
        {
            get
            {
                try
                {
                    string value = this.configuration.Element("AdaptiveSampling").Element("MaxOtherItemsPerSecond").Value;

                    if (int.TryParse(value, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        FormattableString.Invariant($"Could not find or convert the data field {MethodBase.GetCurrentMethod().Name} in configuration. {this.configuration.Value}"), e);
                }
            }
        }

        public Configuration(string configuration)
        {
            try
            {
                configuration = Environment.ExpandEnvironmentVariables(configuration);
            }
            catch(Exception e)
            {
                throw new ArgumentException(FormattableString.Invariant($"Error expanding environment variables contained within the configuration"), e);
            }

            try
            {
                this.configuration = XElement.Parse(configuration);
            }
            catch (Exception e)
            {
                throw new ArgumentException(FormattableString.Invariant($"Error parsing configuration"), e);
            }
        }
    }
}
