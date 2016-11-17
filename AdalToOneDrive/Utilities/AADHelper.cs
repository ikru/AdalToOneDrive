
namespace AdalToOneDrive
{
    using System;
    using System.Configuration;
    using System.Text;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Security.Cryptography.X509Certificates;
    using System.Reflection;
    using System.Security;

    class AADHelper
    {
        private static string AuthorityUrl = ConfigurationManager.AppSettings["AAD:AuthorityUrl"];
        private static string ClientId = ConfigurationManager.AppSettings["AAD:ClientId"];
        private static string ClientKey = ConfigurationManager.AppSettings["AAD:ClientKey"];
        private static Uri ClientRedirectUri = new Uri(ConfigurationManager.AppSettings["AAD:ClientRedirectUri"]);
        private static string ResourceID = ConfigurationManager.AppSettings["AAD:ResourceID"];

        public static string CurrentAccessToken = "";     
        public static DateTimeOffset ExpiresOn;

        private static X509Certificate2 Getx509Certificate()
        {
            X509Certificate2 cert = null;

            try
            {                                
                SecureString pwd = DataEncryption.GetSecuredPasswordFromEncryptedFile(ConfigurationManager.AppSettings["EncryptedPasswordFilePathandName"], Int32.Parse(ConfigurationManager.AppSettings["EncryptionByteLength"]), ConfigurationManager.AppSettings["EncryptionEntropy"]);

                cert = new X509Certificate2(ConfigurationManager.AppSettings["CertificateFilePath"], pwd, X509KeyStorageFlags.MachineKeySet |
                                                                                                                X509KeyStorageFlags.PersistKeySet |
                                                                                                                X509KeyStorageFlags.Exportable);             
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Inner Exception : " + ex.InnerException.Message;
                }

                throw new LoggerException("AcquireToken exception: " + message, ex);
            }

            return cert;
        }

        /// <summary>
        /// Retrieves an access token from AAD using the client app credentials.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public static string AuthenticateAndGetToken(string tenantId, string authenticationMethod)
        {
            AuthenticationResult result = null;
           // string accessToken;

            try
            {                               
                AuthenticationContext authContext = new AuthenticationContext(String.Format(AuthorityUrl, tenantId));                                   
              
                try
                {
                    if (authenticationMethod == "Certificate")
                    {
                        if ((ConfigurationManager.AppSettings["EncryptionEntropy"] == "") || (ConfigurationManager.AppSettings["EncryptionByteLength"] == ""))
                        {
                            Console.WriteLine("Missing Certificate password information. Please enter your certificate password: ");
                            string certPwd = DataEncryption.GetMaskedTextFromConsole();
                            DataEncryption.SetupPasswordEncryption(certPwd);
                        }

                        ClientAssertionCertificate secureClientCredentials = new ClientAssertionCertificate(AADHelper.ClientId, Getx509Certificate());
                        result = authContext.AcquireToken(AADHelper.ResourceID, secureClientCredentials);
                    }
                    else if (authenticationMethod == "ClientKey")
                    {
                        ClientCredential clientCredentials = new ClientCredential(AADHelper.ClientId, AADHelper.ClientKey);
                        result = authContext.AcquireToken(AADHelper.ResourceID, clientCredentials);
                    }
                    else
                    {
                        throw new LoggerException(String.Format("Invalid authentication method: {0}", authenticationMethod));
                    }

                    if (result != null)
                    {
                        //accessToken = result.AccessToken;

                        CurrentAccessToken = result.AccessToken;
                        ExpiresOn = result.ExpiresOn;                        

                        if (String.IsNullOrEmpty(CurrentAccessToken))
                        {
                            LoggerException logEx = new LoggerException(String.Format("A token was not received from Azure Active Directory with Tenant ID {0}.", tenantId), tenantId);
                            throw logEx;
                        }
                    }                    
                }
                catch (AdalException ex)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }

                    throw new LoggerException("AcquireToken exception: " + message, ex);
                }
            }
            catch (LoggerException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new LoggerException("AuthenticateAndGetToken exception: " + ex.Message, ex);
            }
           
            return result.AccessToken;
        }

        /// <summary>
        /// Checks if AccessToken has less than 2 minutes until expiration time.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="AccessToken">The access token.</param>
        /// <returns></returns>
        public static bool AccessTokenAboutToExpire()
        {
            bool result = false;

            TimeSpan critical = new TimeSpan(0, 2, 0); //2 minutes
            
            if ((ExpiresOn - DateTime.Now) < critical)
            {
                result = true;
            }
            
            return result;
        }
    }
}
