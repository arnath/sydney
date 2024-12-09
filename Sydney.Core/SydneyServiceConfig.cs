namespace Sydney.Core;

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class SydneyServiceConfig
{
    /// <summary>
    /// Creates a new instance of SydneyServiceConfig.
    /// </summary>
    /// <param name="useHttps">Indicates whether to use HTTPs.</param>
    /// <param name="port">Port for the server.</param>
    /// <param name="httpsServerCertificate">HTTPs server certificate. Required if useHttps is true.</param>
    /// <param name="returnExceptionMessagesInResponse">Indicates whether to return exception messages in error responses.</param>
    /// <param name="middlewares">Optional list of middlewares that can implement pre and post handler hooks.</param>
    public SydneyServiceConfig(
        bool useHttps,
        ushort port,
        X509Certificate2? httpsServerCertificate = null,
        bool returnExceptionMessagesInResponse = false,
        params SydneyMiddleware[] middlewares)
    {
        this.UseHttps = useHttps;
        this.HttpsServerCertificate = httpsServerCertificate;
        this.Port = port;
        this.ReturnExceptionMessagesInResponse = returnExceptionMessagesInResponse;
        this.Middlewares = new List<SydneyMiddleware>(middlewares);
    }

    /// <summary>
    /// Creates a SydneyServiceConfig for an HTTPs service. Defaults to port 443.
    /// </summary>
    public static SydneyServiceConfig CreateHttps(
        X509Certificate2 httpsServerCertificate,
        ushort port = 443,
        bool returnExceptionMessagesInResponse = false,
        params SydneyMiddleware[] middlewares)
    {
        return new SydneyServiceConfig(true, port, httpsServerCertificate, returnExceptionMessagesInResponse, middlewares);
    }

    /// <summary>
    /// Creates a SydneyServiceConfig for an HTTP service. Defaults to port 80.
    /// </summary>
    public static SydneyServiceConfig CreateHttp(
        ushort port = 80,
        bool returnExceptionMessagesInResponse = false,
        params SydneyMiddleware[] middlewares)
    {
        return new SydneyServiceConfig(false, port, null, returnExceptionMessagesInResponse, middlewares);
    }

    /// <summary>
    /// Indicates whether the service should use HTTPs.
    /// </summary>
    public bool UseHttps { get; set; }

    /// <summary>
    /// HTTPs server certificate. Required if UseHttps is true.
    /// </summary>
    public X509Certificate2? HttpsServerCertificate { get; set; }

    /// <summary>
    /// Port for the server.
    /// </summary>
    public ushort Port { get; set; }

    /// <summary>
    /// Indicates whether to return exception messages in error responses.
    /// </summary>
    public bool ReturnExceptionMessagesInResponse { get; set; }

    /// <summary>
    /// Optional list of middlewares that can implement pre and post handler hooks.
    /// </summary>
    public IList<SydneyMiddleware> Middlewares { get; }

    /// <summary>
    /// Performs some internal validation on service config.
    /// </summary>
    internal virtual void Validate()
    {
        if (this.Port == 0)
        {
            throw new ArgumentException("SydneyServiceConfig.Port must be a valid port value between 1 and 65535.");
        }

        if (this.UseHttps && this.HttpsServerCertificate == null)
        {
            throw new ArgumentException("SydneyServiceConfig.HttpsServerCertificate must be specified when UseHttps is true.");
        }

        if (this.Port == 443 && !this.UseHttps)
        {
            throw new ArgumentException("Cannot use port 443 while SydneyServiceConfig.UseHttps is false.");
        }

        if (this.Port == 80 && this.UseHttps)
        {
            throw new ArgumentException("Cannot use port 80 while SydneyServiceConfig.UseHttps is true.");
        }
    }
}
