namespace Sydney.Core
{
    using System;

    /// <summary>
    /// Configuration for the Sydney service.
    /// </summary>
    public class SydneyServiceConfig
    {
        /// <summary>
        /// Creates a new instance of SydneyServiceConfig.
        /// </summary>
        public SydneyServiceConfig()
        {
        }

        /// <summary>
        /// Creates a new instance of SydneyServiceConfig.
        /// </summary>
        /// <param name="scheme">Scheme for the server, must be "http" or "https".</param>
        /// <param name="host">Host address like "www.example.com" or "*" or "+" to match all addresess.</param>
        /// <param name="port">Port for the server.</param>
        /// <param name="returnExceptionMessagesInResponse">Indicates whether to return exception messages in error responses.</param>
        public SydneyServiceConfig(string scheme, string host, ushort port, bool returnExceptionMessagesInResponse = false)
        {
            this.Scheme = scheme;
            this.Host = host;
            this.Port = port;
            this.ReturnExceptionMessagesInResponse = returnExceptionMessagesInResponse;
        }

        /// <summary>
        /// Gets or sets the scheme for the server, must be "http" or "https".
        /// It is recommended to use Uri.UriSchemeHttp or Uri.UriSchemeHttps.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the host address for the server like "www.example.com".
        /// Can also use the value "*" or "+" to match all addresess.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port for the server.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return exception messages
        /// in error responses.
        /// </summary>
        public bool ReturnExceptionMessagesInResponse { get; set; }

        internal virtual void Validate()
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
