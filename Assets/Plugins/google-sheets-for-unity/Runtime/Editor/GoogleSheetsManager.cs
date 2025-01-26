using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using UnityEditor;
using UnityEngine;

namespace catnexu.googlesheetsforunity.Editor
{
    public static class GoogleSheetsManager
    {
        private static readonly string[] s_scopes = new[] { SheetsService.Scope.Spreadsheets };
        
        private static string s_tokenPath = string.Empty;
        private static SheetsService s_service;

        public static SheetsService SheetsService => s_service;

        [MenuItem("Tools/Utils/Google Sheets/Auth")]
        public static void AuthGoogle()
        {
            s_tokenPath = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), "googlesheets_token");
            if (!TryAuthorization(out UserCredential credential))
            {
                return;
            }

            s_service = new SheetsService(new BaseClientService.Initializer
                    {
                            HttpClientInitializer = credential,
                            ApplicationName = GoogleSheetSettings.instance.ApplicationName,
                    });
        }

        private static bool TryAuthorization(out UserCredential credential)
        {
            credential = default;
            GoogleSheetSettings sheetSettings = GoogleSheetSettings.instance;
            string credentialsData = sheetSettings.Credentials;
            if (string.IsNullOrEmpty(credentialsData))
            {
                Debug.LogError("Google Sheet Credentials is null or empty!");
                return false;
            }
            ServicePointManager.ServerCertificateValidationCallback += CertificateValidationCallback;
            using (var stream = GetStream(credentialsData))
            {
                try
                {
                    ClientSecrets secrets = GoogleClientSecrets.FromStream(stream).Secrets;
                    FileDataStore dataStore = new FileDataStore(s_tokenPath, true);
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets,
                                                                             s_scopes,
                                                                             sheetSettings.User,
                                                                             CancellationToken.None,
                                                                             dataStore).Result;
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception.ToString());
                    return false;
                }
                finally
                {
                    Debug.Log($"Credential file saved to: {s_tokenPath}");   
                }
            }

            return true;
        }

        private static Stream GetStream(string credentials)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(credentials);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            ServicePointManager.ServerCertificateValidationCallback -= CertificateValidationCallback;
            bool isOk = true;
            if (sslpolicyerrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            
            return isOk;
        }
    }
}