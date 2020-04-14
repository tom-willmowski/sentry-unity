using System;
using UnityEngine;

namespace Sentry
{
    public class UnityEventProcessor : ISentryEventProcessor
    {
        public SentryEvent Process(SentryEvent sentryEvent)
        {
            sentryEvent.release = Application.version;
            sentryEvent.contexts.app.app_identifier = Application.identifier;
            sentryEvent.contexts.os.name = SystemInfo.operatingSystem;

            sentryEvent.contexts.device.name = SystemInfo.deviceName;
#pragma warning disable RECS0018 // Value is exact when expressing no battery level
            if (SystemInfo.batteryLevel != -1.0)
#pragma warning restore RECS0018
            {
                sentryEvent.contexts.device.battery_level = (float)(SystemInfo.batteryLevel * 100);
            }
            sentryEvent.contexts.device.memory_size = SystemInfo.systemMemorySize;
            sentryEvent.contexts.device.timezone = TimeZoneInfo.Local.ToString();
            sentryEvent.contexts.app.app_start_time = DateTimeOffset.UtcNow.AddSeconds(-Time.realtimeSinceStartup).ToString();

            var model = SystemInfo.deviceModel;
            if (model != SystemInfo.unsupportedIdentifier
                // Returned by the editor
                && model != "System Product Name (System manufacturer)")
            {
                sentryEvent.contexts.device.model = model;
            }

            if (SystemInfo.systemMemorySize != 0)
            {
                sentryEvent.contexts.device.memory_size = SystemInfo.systemMemorySize * 1048576L; // Sentry device mem is in Bytes
            }

            var gpu = new Gpu
            {
                id = SystemInfo.graphicsDeviceID,
                name = SystemInfo.graphicsDeviceName,
                vendor_id = SystemInfo.graphicsDeviceVendorID,
                vendor_name = SystemInfo.graphicsDeviceVendor,
                memory_size = SystemInfo.graphicsMemorySize,
                multi_threaded_rendering = SystemInfo.graphicsMultiThreaded,
                npot_support = SystemInfo.npotSupport.ToString(),
                version = SystemInfo.graphicsDeviceVersion,
                api_type = SystemInfo.graphicsDeviceType.ToString()
            };

            sentryEvent.contexts.gpu = gpu;
            sentryEvent.contexts.app.app_start_time = DateTimeOffset.UtcNow
                .AddSeconds(-Time.realtimeSinceStartup).ToString();
            sentryEvent.contexts.app.build_type = Debug.isDebugBuild ? "debug" : "production";
            sentryEvent.contexts.device.simulator = Application.isEditor;
            return sentryEvent;
        }
    }
}
