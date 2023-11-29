-- Create the event table
CREATE TABLE {0}_event (
    domain NVARCHAR(40) NOT NULL,
    aggregate_type NVARCHAR(40) NOT NULL,
    aggregate_id NVARCHAR(40) NOT NULL,
    sequence_id BIGINT NOT NULL,
    captured_at DATETIME NOT NULL,
    event_type NVARCHAR(40) NOT NULL,
    captured_by NVARCHAR(40) NOT NULL,
    json_data NVARCHAR(MAX) NOT NULL,
    stored_at DATETIME DEFAULT GETDATE() NOT NULL,
    
    CONSTRAINT PK_event PRIMARY KEY (domain, aggregate_type, aggregate_id, sequence_id),
    CONSTRAINT CK_event_domain_not_empty CHECK (LEN(domain) > 0),
    CONSTRAINT CK_event_aggregate_type_not_empty CHECK (LEN(aggregate_type) > 0),
    CONSTRAINT CK_event_aggregate_id_not_empty CHECK (LEN(aggregate_id) > 0),
    CONSTRAINT CK_event_event_type_not_empty CHECK (LEN(event_type) > 0),
    CONSTRAINT CK_event_captured_by_not_empty CHECK (LEN(captured_by) > 0),
    CONSTRAINT CK_event_json_data_not_empty CHECK (LEN(json_data) > 0)
);

-- Index for getting distinct values for each column domain
CREATE INDEX IX_event_domain
ON event (domain);

-- Index for getting distinct values for columns domain and aggregate_type together
CREATE INDEX IX_event_domain_aggregate_type
ON event (domain, aggregate_type);

-- Index for getting distinct values for columns domain, aggregate_type, and aggregate_id together
CREATE INDEX IX_event_domain_aggregate_type_aggregate_id
ON event (domain, aggregate_type, aggregate_id);

-- Index for getting records with a specific value in column event_type and a value of captured_at within a given time range, sorted by captured_at
CREATE INDEX IX_event_event_type_captured_at
ON event (event_type, captured_at);



-- Create the snapshot table
CREATE TABLE {0}_snapshot (
    domain NVARCHAR(40) NOT NULL,
    aggregate_type NVARCHAR(40) NOT NULL,
    aggregate_id NVARCHAR(40) NOT NULL,
    sequence_id BIGINT NOT NULL,
    json_data NVARCHAR(MAX) NOT NULL,
    stored_at DATETIME DEFAULT GETDATE() NOT NULL,
    
    CONSTRAINT PK_snapshot PRIMARY KEY (domain, aggregate_type, aggregate_id, sequence_id),
    CONSTRAINT CK_domain_not_empty CHECK (LEN(domain) > 0),
    CONSTRAINT CK_aggregate_type_not_empty CHECK (LEN(aggregate_type) > 0),
    CONSTRAINT CK_aggregate_id_not_empty CHECK (LEN(aggregate_id) > 0),
    CONSTRAINT CK_json_data_not_empty CHECK (LEN(json_data) > 0)
);

-- Index for finding records with an earlier point in time value in column stored_at than some given value, and that other records in the group exist
CREATE INDEX IX_snapshot_earlier_stored_at
ON snapshot (domain, aggregate_type, aggregate_id, stored_at);
