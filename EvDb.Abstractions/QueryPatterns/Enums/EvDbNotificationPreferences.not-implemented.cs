// TODO: [bnaya: 2025-05-28] Consider Polling vs. Notification (slow updates favor for Notification, heavy favor Polling, filtering favor Polling)
/*
 * https://claude.ai/share/6b5fd52a-02b7-483e-b967-967bf02ecd08
 * 
 * pg_notify is more efficient when:
 * 
 *   - Low to medium event frequency (< 100 events/second)
 *   - Low latency requirements (< 10ms)
 *   - Single PostgreSQL database
 *   - Simple event filtering
 * 
 * Polling is more efficient when:
 * 
 *   - High event frequency (> 1000 events/second)
 *   - Complex filtering at database level
 *   - Multiple data sources
 *   - Batch processing preferred over individual events
 */

//namespace EvDb.Core;

///// <summary>
/////  <![CDATA[Preferences for notification vs. batch polling in continuous fetch operations.
///// 
///// Notification is more efficient when:
///// 
/////   - Low to medium event frequency (< 100 events/second)
/////   - Low latency requirements (< 10ms)
/////   - Single PostgreSQL database
/////   - Simple event filtering
///// 
///// Polling is more efficient when:
///// 
/////   - High event frequency (> 1000 events/second)
/////   - Complex filtering at database level
/////   - Multiple data sources
/////   - Batch processing preferred over individual events]]>
///// </summary>
//public enum EvDbNotificationPreferences
//{
//    /// <summary>   
//    /// Prefer batching over notification.
//    /// </summary>
//    PreferBatchingPolling = 1,
//    /// <summary>
//    /// Prefer notification over batching.
//    /// </summary>
//    PreferNotification = 2,
//    ///// <summary>
//    ///// Adaptive strategy that chooses between batching and notification based on load and performance heuristics.
//    ///// </summary>
//    //Adaptive,
//    Default = PreferNotification, //  Adaptive,
//}
