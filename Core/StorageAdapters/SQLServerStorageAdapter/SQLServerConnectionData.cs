using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.StorageAdapters.SQLServerStorageAdapter;
public record SQLServerConnectionData
(
    string DataSource = "127.0.0.1",
    string UserID = "sa",
    string Password = "MasadNetunim12!@",
    string InitialCatalog = "master"
);

