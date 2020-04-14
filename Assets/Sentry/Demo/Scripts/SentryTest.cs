using UnityEngine.Assertions;
using UnityEngine;
using System;
using Sentry;

public class SentryTest : MonoBehaviour
{
    private SentryClient client;
    [SerializeField]
    private string dsn;

    private void Awake()
    {
        SentryOptions.Builder builder = new SentryOptions.Builder(dsn);
        UnityEventProcessor defaultProcessor = new UnityEventProcessor();
        SentryOptions options = builder
            .SetDebug(true)
            .AddExclude("UnityEngine")
            .SetMaxBreadcrumbs(100)
            .SetSendDefaultPii(true)
            .SetEventProcessor(defaultProcessor.Process)
            .Build();
        client = new SentryClient(options);
    }

    private void Start()
    {
        Application.logMessageReceived += OnLog;
    }

    private void OnLog(string condition, string stackTrace, LogType type)
    {
        client.AddBreadcrumb(condition);
        if(type == LogType.Exception)
        {
            client.CaptureException(condition, stackTrace, type);
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
    }

    #region
    GameObject go = null; 
    public void Crash()
    {
        go.SetActive(true);
    }

    public void Message()
    {
        Debug.Log("Simple message");
    }
    #endregion
}