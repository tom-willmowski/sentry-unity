namespace Sentry
{
    public interface ISentryEventProcessor
    {
        SentryEvent Process(SentryEvent sentryEvent);
    }
}