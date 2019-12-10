namespace Sydney.Core
{
    using System;

    public class SydneyServiceConfig
    {
        public SydneyServiceConfig()
        {
        }

        public SydneyServiceConfig(string scheme, string host, ushort port, bool returnExceptionMessagesInResponse = false)
        {
            this.Scheme = scheme;
            this.Host = host;
            this.Port = port;
            this.ReturnExceptionMessagesInResponse = returnExceptionMessagesInResponse;
        }

        public string Scheme { get; set; }

        public string Host { get; set; }

        public ushort Port { get; set; }

        public bool ReturnExceptionMessagesInResponse { get; set; }

        internal void Validate()
        {
            if (this.Scheme == null ||
                (!this.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                    !this.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"SydneyServiceConfig.Scheme must be one of \"{Uri.UriSchemeHttp}\" or \"{Uri.UriSchemeHttps}\".");
            }

            if (string.IsNullOrEmpty(this.Host))
            {
                throw new ArgumentException("SydneyServiceConfig.Host must be a valid non-empty string. Use \"*\" or \"+\" to match all hosts.");
            }

            if (this.Port == 0)
            {
                throw new ArgumentException("SydneyServiceConfig.Port must be a valid port value between 1 and 65535.");
            }
        }
    }
}
