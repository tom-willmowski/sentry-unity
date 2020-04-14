using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sentry
{
    public static class SentryUtils
    {
        public static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
        }

        public static IEnumerable<StackTraceSpec> GetStackTraces(string stackTrace)
        {
            var stackList = stackTrace.Split('\n');
            // the format is as follows:
            // Module.Class.Method[.Invoke] (arguments) (at filename:lineno)
            // where :lineno is optional, will be omitted in builds
            for (var i = stackList.Length - 1; i >= 0; i--)
            {
                string functionName;
                string filename;
                int lineNo;

                var item = stackList[i];
                if (item == string.Empty)
                {
                    continue;
                }
                var closingParen = item.IndexOf(')');

                if (closingParen == -1)
                {
                    continue;
                }
                try
                {
                    functionName = item.Substring(0, closingParen + 1);
                    if (item.Length < closingParen + 6)
                    {
                        // No location and no params provided. Use it as-is
                        filename = string.Empty;
                        lineNo = -1;
                    }
                    else if (item.Substring(closingParen + 1, 5) != " (at ")
                    {
                        // we did something wrong, failed the check
                        Debug.Log("failed parsing " + item);
                        functionName = item;
                        lineNo = -1;
                        filename = string.Empty;
                    }
                    else
                    {
                        var colon = item.LastIndexOf(':', item.Length - 1, item.Length - closingParen);
                        if (closingParen == item.Length - 1)
                        {
                            filename = string.Empty;
                            lineNo = -1;
                        }
                        else if (colon == -1)
                        {
                            filename = item.Substring(closingParen + 6, item.Length - closingParen - 7);
                            lineNo = -1;
                        }
                        else
                        {
                            filename = item.Substring(closingParen + 6, colon - closingParen - 6);
                            lineNo = Convert.ToInt32(item.Substring(colon + 1, item.Length - 2 - colon));
                        }
                    }
                }
                catch
                {
                    continue;
                }

                bool inApp;

                if (filename == string.Empty
                    // i.e: <d315a7230dee4fa58154dc9e8884174d>
                    || (filename[0] == '<' && filename[filename.Length - 1] == '>'))
                {
                    // Addresses will mess with grouping. Unless possible to symbolicate, better not to report it.
                    filename = string.Empty;
                    inApp = true; // defaults to true

                    if (functionName.Contains("UnityEngine."))
                    {
                        inApp = false;
                    }
                }
                else
                {
                    inApp = filename.Contains("Assets/");
                }

                yield return new StackTraceSpec(filename, functionName, lineNo, inApp);
            }
        }
    }
    public static class QueueExtensions
    {
        public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize) 
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                yield return queue.Dequeue();
            }
        }
    }
}