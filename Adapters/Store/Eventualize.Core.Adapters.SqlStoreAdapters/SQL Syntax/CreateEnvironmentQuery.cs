using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;

// TODO: [bnaya 2023-12-10] migration should move to a different project 
// TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
public static class CreateEnvironmentQuery
{
    public static string GetSqlString(string contextIdPrefix)
    {
        // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
        return $@"
            -- Create the event table
CREATE TABLE {contextIdPrefix}event (
    domain NVARCHAR(40) NOT NULL,
    aggregate_type NVARCHAR(40) NOT NULL,
    aggregate_id NVARCHAR(40) NOT NULL,
    sequence_id BIGINT NOT NULL,
    captured_at DATETIME NOT NULL,
    event_type NVARCHAR(40) NOT NULL,
    captured_by NVARCHAR(40) NOT NULL,
    json_data NVARCHAR(MAX) NOT NULL,
    stored_at DATETIME DEFAULT GETDATE() NOT NULL,
    
    CONSTRAINT PK_{contextIdPrefix}event PRIMARY KEY (domain, aggregate_type, aggregate_id, sequence_id),
    CONSTRAINT CK_{contextIdPrefix}event_domain_not_empty CHECK (LEN(domain) > 0),
    CONSTRAINT CK_{contextIdPrefix}event_aggregate_type_not_empty CHECK (LEN(aggregate_type) > 0),
    CONSTRAINT CK_{contextIdPrefix}event_aggregate_id_not_empty CHECK (LEN(aggregate_id) > 0),
    CONSTRAINT CK_{contextIdPrefix}event_event_type_not_empty CHECK (LEN(event_type) > 0),
    CONSTRAINT CK_{contextIdPrefix}event_captured_by_not_empty CHECK (LEN(captured_by) > 0),
    CONSTRAINT CK_{contextIdPrefix}event_json_data_not_empty CHECK (LEN(json_data) > 0)
);

-- Index for getting distinct values for each column domain
CREATE INDEX IX_event_domain
ON {contextIdPrefix}event (domain);

-- Index for getting distinct values for columns domain and aggregate_type together
CREATE INDEX IX_event_domain_aggregate_type
ON {contextIdPrefix}event (domain, aggregate_type);

-- Index for getting distinct values for columns domain, aggregate_type, and aggregate_id together
CREATE INDEX IX_event_domain_aggregate_type_aggregate_id
ON {contextIdPrefix}event (domain, aggregate_type, aggregate_id);

-- Index for getting records with a specific value in column event_type and a value of captured_at within a given time range, sorted by captured_at
CREATE INDEX IX_event_event_type_captured_at
ON {contextIdPrefix}event (event_type, captured_at);



-- Create the snapshot table
CREATE TABLE {contextIdPrefix}snapshot (
    domain NVARCHAR(40) NOT NULL,
    aggregate_type NVARCHAR(40) NOT NULL,
    aggregate_id NVARCHAR(40) NOT NULL,
    sequence_id BIGINT NOT NULL,
    json_data NVARCHAR(MAX) NOT NULL,
    stored_at DATETIME DEFAULT GETDATE() NOT NULL,
    
    CONSTRAINT PK_{contextIdPrefix}snapshot PRIMARY KEY (domain, aggregate_type, aggregate_id, sequence_id),
    CONSTRAINT CK_{contextIdPrefix}snapshot_domain_not_empty CHECK (LEN(domain) > 0),
    CONSTRAINT CK_{contextIdPrefix}snapshot_aggregate_type_not_empty CHECK (LEN(aggregate_type) > 0),
    CONSTRAINT CK_{contextIdPrefix}snapshot_aggregate_id_not_empty CHECK (LEN(aggregate_id) > 0),
    CONSTRAINT CK_{contextIdPrefix}snapshot_json_data_not_empty CHECK (LEN(json_data) > 0)
);

-- Index for finding records with an earlier point in time value in column stored_at than some given value, and that other records in the group exist
CREATE INDEX IX_snapshot_earlier_stored_at
ON {contextIdPrefix}snapshot (domain, aggregate_type, aggregate_id, stored_at);
            ";
    }

}