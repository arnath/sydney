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
        /// <param name="port">Port for the server.</param>
        /// <param name="returnExceptionMessagesInResponse">Indicates whether to return exception messages in error responses.</param>
        public SydneyServiceConfig(ushort port, bool returnExceptionMessagesInResponse = false)
        {
            this.Port = port;
            this.ReturnExceptionMessagesInResponse = returnExceptionMessagesInResponse;
        }

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
            if (this.Port == 0)
            {
                throw new ArgumentException("SydneyServiceConfig.Port must be a valid port value between 1 and 65535.");
            }
        }
    }
}
