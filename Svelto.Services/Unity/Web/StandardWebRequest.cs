using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using UnityEngine.Networking;

namespace Svelto.Services
{
    public enum Method
    {
        GET,
        POST
    }

    public enum Result
    {
        Success,
        ServerHandledError,
        ServerException,
        ClientFailure,
    }

    public sealed class StandardWebRequest
    {
        static StandardWebRequest()
        {
            ServicePointManager.ServerCertificateValidationCallback = CannotVerifyMessageCertificate;

            ServicePointManager.DefaultConnectionLimit = 64;
        }

        public Result result { get; private set; }

        public int maxAttempts        = 3;
        public int waitOnRetrySeconds = 1;
        public int timeoutSeconds     = 10;

        public Method                     method             = Method.POST;
        public Func<HttpStatusCode, bool> processStatusCodes = code => false;
        public IResponseHandler           responseHandler;
        public string                     URL;

        public IEnumerator<TaskContract> Execute(byte[] bodyRaw)
        {
            int attemptNumber = 0;

            do
            {
                using (UnityWebRequest request = new UnityWebRequest())
                {
                    switch (method)
                    {
                        case Method.GET:
                            request.method = UnityWebRequest.kHttpVerbGET;
                            break;
                        case Method.POST:
                            request.method = UnityWebRequest.kHttpVerbPOST;
                            break;
                        default:
                            request.method = UnityWebRequest.kHttpVerbPOST;
                            break;
                    }

                    request.url = URL;
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new UnityDownloadHandler(responseHandler);
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = timeoutSeconds;

                    AsyncOperation op = request.SendWebRequest();

                    while (op.isDone == false)
                    {
                        yield return Yield.It;
                    }

                    if (request.isNetworkError == false)
                    {
                        if (ProcessResponse(request) == true)
                        {
                            result = Result.Success;
                            
                            Svelto.Console.LogDebug("web request completed");

                            yield break;
                        }
                        else
                        {
                            Svelto.Console.LogDebug("web request completed with failure ", URL);

                            try
                            {
                                responseHandler?.CompleteContent();

                                result = Result.ServerHandledError;
                            }
                            catch
                            {
                                result = Result.ServerException;
                            }

                            yield break; //no retry on server error!
                        }
                    }
                    else
                    if (++attemptNumber < maxAttempts)
                    {
                        var wait = new ReusableWaitForSecondsEnumerator(waitOnRetrySeconds);

                        while (wait.MoveNext() == true) yield return Yield.It;

                        Svelto.Console.LogDebug("web request retry");

                        continue; //retry on client error
                    }
                    else
                    {
                        result = Result.ClientFailure;

                        yield break;
                    }
                }
            }
            while (true);
        }

        bool ProcessResponse(UnityWebRequest request)
        {
            HttpStatusCode statusCode = (HttpStatusCode) request.responseCode;

            if (statusCode != HttpStatusCode.OK)
                if (processStatusCodes(statusCode) == false)
                    return false;

            return true;
        }

        // BOC:: After turning on SSL on the gameserver we started getting certificate problems.
        // Found this solution on Stack Overflow with Mike R
        // (https://stackoverflow.com/questions/4926676/mono-https-webrequest-fails-with-the-authentication-or-decryption-has-failed)
        static bool CannotVerifyMessageCertificate(System.Object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;

            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                        continue;

                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

                    bool chainIsValid = chain.Build((X509Certificate2) certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }

            return isOk;
        }
    }
}