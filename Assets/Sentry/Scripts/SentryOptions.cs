using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sentry {
    public class SentryOptions
    {
        public Dsn Dsn { get; }
        public int MaxBreadcrumbs { get; }
        public bool Debug { get; }
        public string Release { get; }
        public bool SendDefaultPii { get; }
        public List<string> Excludes { get; }
        public Func<SentryEvent, SentryEvent> Processor { get; }

        public class Builder 
        {
            private string dsn;
            private int maxBreadcrumbs;
            private string release;
            private bool sendDefaultPii;
            private List<string> excludes;
            private bool debug;
            private Func<SentryEvent, SentryEvent> processor;

            public Builder(string dsn)
            { 
                this.dsn = dsn;
                excludes = new List<string>();
                debug = false;
                sendDefaultPii = true;
                release = Application.version;
                maxBreadcrumbs = 100;
                processor = default;
            }

            public Builder SetMaxBreadcrumbs(int maxBreadcrumbs)
            {
                this.maxBreadcrumbs = maxBreadcrumbs;
                return this;
            }
            
            public Builder SetRelease(string release)
            {
                this.release = release;
                return this;
            }

            public Builder SetSendDefaultPii(bool sendDefaultPii)
            {
                this.sendDefaultPii = sendDefaultPii;
                return this;
            }

            public Builder AddExclude(string exclude)
            {
                this.excludes.Add(exclude);
                return this;
            }

            public Builder SetDebug(bool debug)
            {
                this.debug = debug;
                return this;
            }

            public Builder SetEventProcessor(Func<SentryEvent, SentryEvent> processor)
            {
                this.processor = processor;
                return this;
            }

            public SentryOptions Build()
            {
                return new SentryOptions(dsn, maxBreadcrumbs, release, sendDefaultPii, excludes, debug, processor);
            }
        }

        private SentryOptions(string dsn, int maxBreadcrumbs, string release, bool sendDefaultPii, List<string> excludes, bool debug, Func<SentryEvent, SentryEvent> processor)
        {
            this.Dsn = new Dsn(dsn);
            this.MaxBreadcrumbs = maxBreadcrumbs;
            this.Debug = debug;
            this.Release = release;
            this.SendDefaultPii = sendDefaultPii;
            this.Excludes = excludes;
            this.Processor = processor;
        }
    }
}