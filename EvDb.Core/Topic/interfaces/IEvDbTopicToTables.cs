using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Core.Internals;

/// <summary>
/// Map topic to tables.
/// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
/// </summary>
public interface IEvDbTopicToTables
{
    /// <summary>
    /// Map topic to tables.
    /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
    /// </summary>
    /// <param name="topic">The topic.</param>
    /// <returns></returns>
    string[] TopicToTables(string topic);
}
