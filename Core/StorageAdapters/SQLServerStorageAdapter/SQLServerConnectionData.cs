using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.StorageAdapters.SQLServerStorageAdapter;

// TODO: [bnaya 2023-12-07] get it from environment variable
public record SQLServerConnectionData
(
    string DataSource = "localhost",
    string UserID = "sa",
    string Password = "MasadNetunim12!@",
    string InitialCatalog = "master",
    bool TrustServerCertificate = true,
    bool MultipleActiveResultSets = true
);

