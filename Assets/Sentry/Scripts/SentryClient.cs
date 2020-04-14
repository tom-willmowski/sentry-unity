using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Sentry 
{
    public class SentryClient
    {
        private SentryOptions options;
        private Queue<Breadcrumb> breadcrumbs;

        public SentryClient(SentryOptions options)
        {
            this.options = options;
            this.breadcrumbs = new Queue<Breadcrumb>();
        }

        public void CaptureMessage(string message)
        {
            SentryEvent sentryEvent = new SentryEvent(message, GetBreadcrumbs());
            sentryEvent.level = "info";
            Send(sentryEvent);
        }

        public void CaptureException(string condition, string stackTrace, LogType type)
        {
            Debug.Log($"condition: {condition} stack: {stackTrace}");
            var exc = condition.Split(new char[] {':'}, 2);
            var excType = exc[0];
            var excValue = exc[1].Substring(1);
            
            List<StackTraceSpec> stackTraces = SentryUtils.GetStackTraces(stackTrace).ToList();
            SentryExceptionEvent exception = new SentryExceptionEvent(excType, excValue, GetBreadcrumbs(), stackTraces);
            Send(exception);
        }

        public void AddBreadcrumb(string message)
        {
            breadcrumbs.Enqueue(new Breadcrumb(SentryUtils.GetTimestamp(), message));
        }

        public List<Breadcrumb> GetBreadcrumbs()
        {
            return breadcrumbs.DequeueChunk(options.MaxBreadcrumbs).ToList();
        }

        private void Send(SentryEvent sentryEvent)
        {
            sentryEvent = options.Processor.Invoke(sentryEvent);
            string json = JsonUtility.ToJson(sentryEvent);
            UnityWebRequest request = new UnityWebRequest(options.Dsn.callUri);
            request.method = "POST";
            request.SetRequestHeader("X-Sentry-Auth", GetAuth());
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation handler = request.SendWebRequest();
            handler.completed += OnRequestSent;
        }

        private void OnRequestSent(AsyncOperation operation)
        {
            UnityWebRequest request = ((UnityWebRequestAsyncOperation)operation).webRequest;
            if(request.isNetworkError || request.isHttpError || request.responseCode != 200)
            {
                Debug.LogWarning($"Sentry send event error: {request.error}");
            } else if(options.Debug)
            {
                Debug.Log($"Sentry event sent: {request.downloadHandler.text}");
            }
            request.Dispose();
        }

        private string GetAuth()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
            return string.Format("Sentry sentry_version=5,sentry_client=Unity0.1," +
                 "sentry_timestamp={0}," +
                 "sentry_key={1}," +
                 "sentry_secret={2}",
                 timestamp,
                 options.Dsn.publicKey,
                 options.Dsn.secretKey);
        }
    }
}