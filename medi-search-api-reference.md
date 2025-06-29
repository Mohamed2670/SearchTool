
# 📘 Medi-Search Tool – API Reference

All routes are prefixed by their respective controllers. Authorization requirements and endpoint behaviors are noted per section.

---

## 🔄 `DBSyncController` (Route: `/DBSync`)

🔐 **Authorization**: `SuperAdmin` policy (some anonymous allowed)

| Method | Endpoint                              | Description                                  |
|--------|----------------------------------------|----------------------------------------------|
| POST   | `/DBSync/SyncUsers`                   | Sync users from main source to database      |
| POST   | `/DBSync/SyncUserDataCsv`             | Import users from default `Users.csv`        |
| POST   | `/DBSync/SyncLogs`                    | Sync logs from main source                   |
| POST   | `/DBSync/SyncLogsCsv`                 | Import logs from CSV (`logs (5).csv`)        |
| GET    | `/DBSync/SyncLogsByExcel`             | Export logs to CSV                           |
| GET    | `/DBSync/SyncUserByExcel`             | Export users to CSV                          |
| GET    | `/DBSync/ImportLogsFromCsvWithoutId`  | Import logs from CSV (ignores IDs)           |

---

## 💊 `DrugController` (Route: `/drug`)

🔐 **Authorization**: `Pharmacist` (some anonymous)

| Method | Endpoint                              | Description                                  |
|--------|----------------------------------------|----------------------------------------------|
| GET    | `/drug`                               | Trigger drug processing                      |
| GET    | `/temp2`                              | Trigger secondary processing routine         |
| GET    | `/GetAllDrugs`                        | Returns all drugs                            |
| GET    | `/SearchByNdc`                        | Search drug by NDC                           |
| GET    | `/searchByName`                       | Search drugs by name (paginated)             |
| GET    | `/GetClassesByName`                   | Get drug classes by name (paginated)         |
| GET    | `/SearchByIdNdc`                      | Search drug by ID + NDC                      |
| GET    | `/getDrugNDCs`                        | Get NDCs by drug name                        |
| GET    | `/GetByslections`                     | Get drug by name, NDC, insurance             |
| GET    | `/GetAltrantives`                     | Get alternatives by class + insurance        |
| GET    | `/GetDetails`                         | Get drug details by NDC + insurance ID       |
| GET    | `/GetClassById`                       | Get drug class by ID                         |
| GET    | `/GetDrugsByClass`                    | Get drugs by class ID                        |
| GET    | `/GetDrugsByClassBranch`              | Get drugs by class and branch ID             |
| GET    | `/GetAllLatest`                       | Get latest drugs                             |
| GET    | `/GetAllLatestScripts`                | Get latest scripts                           |
| GET    | `/GetAllDrugs`                        | Get all drugs by class                       |
| GET    | `/GetAllDrugsV2,V3,V4,V2Insu`         | Get all drugs by class, versioned logic      |
| GET    | `/GetDrugById`                        | Get drug by ID                               |
| GET    | `/GetInsuranceByNdc`                  | Get insurances for NDC                       |
| GET    | `/GetScriptByScriptCode`              | Get script by code (admin only)              |
| GET    | `/ImportInsurancesFromCsvAsync`       | Import insurances from CSV                   |
| GET    | `/GetAlternativesByClassIdBranchId`   | Get alternatives by class + branch           |
| GET    | `/GetDrugsByInsurance`                | Get drugs by insurance ID + name             |
| GET    | `/GetDrugsByInsuranceName`            | Get drugs by insurance name                  |
| GET    | `/GetDrugsByInsuranceNamePagintated`  | Get drugs by insurance name (paginated)      |
| GET    | `/GetDrugsByPCNPagintated`            | Get drugs by PCN (paginated)                 |
| GET    | `/GetDrugsByBINPagintated`            | Get drugs by BIN (paginated)                 |
| GET    | `/GetDrugsByInsuranceNameDrugName`    | Get drugs by insurance + name (paginated)    |
| GET    | `/GetDrugsByPCN`                      | Get drugs by PCN                             |
| GET    | `/GetDrugsByBIN`                      | Get drugs by BIN                             |
| GET    | `/GetInsurances`                      | Get insurances by name                       |
| GET    | `/GetInsurancesBinsByName`            | Get BINs by insurance name                   |
| GET    | `/GetInsurancesPcnByBinId`            | Get PCNs by BIN ID                           |
| GET    | `/GetInsurancesRxByPcnId`             | Get RX groups by PCN ID                      |
| GET    | `/GetAllLatestScriptsPaginated`       | Get paginated latest scripts (admin)         |
| GET    | `/GetAllLatestScriptsPaginatedv2`     | Get paginated latest scripts (v2)            |
| GET    | `/GetLatestScriptsByMonthYear`        | Get scripts by month/year (admin only)       |
| POST   | `/AddScritps`                         | Add scripts (admin only)                     |
| GET    | `/GetBestAlternativeByNDCRxGroupId`   | Get best alternative by class + RxGroup ID   |
| GET    | `/AddMediCare`                        | Add Medicare drugs                           |
| GET    | `/GetAllMediDrugs`                    | Get Medicare drugs by class ID               |
| GET    | `/GetDrugClassesByInsuranceNamePagintated` | Drug classes by insurance name       |
| GET    | `/GetDrugClassesByPCNPagintated`      | Drug classes by PCN                          |
| GET    | `/GetDrugClassesByBINPagintated`      | Drug classes by BIN                          |
| GET    | `/GetAllDrugsWithClassNames`          | Export drugs with class names to CSV         |

---

## 🛡️ `InsuranceController` (Route: `/Insurance`)

🔐 **Authorization**: Some endpoints require `Admin`

| Method | Endpoint                                | Description                                |
|--------|------------------------------------------|--------------------------------------------|
| GET    | `/GetInsuranceDetails`                  | Get insurance details by ID                |
| GET    | `/GetAllRxGroups`                       | Get all Rx groups                          |
| GET    | `/GetAllRxGroupsByPcnId`                | Rx groups by PCN ID                        |
| GET    | `/GetAllPCNByBinId`                     | Get PCNs by BIN ID                         |
| GET    | `/GetAllBIN`                            | Get all BINs                               |
| GET    | `/GetInsurancePCNDetails`               | Get insurance PCN details by ID            |
| GET    | `/GetInsuranceBINDetails`               | Get insurance BIN details by ID            |
| GET    | `/GetAllRxGroupsByBINId`                | Rx groups by BIN ID                        |
| GET    | `/GetAllPCNsByBINId`                    | PCNs by BIN ID                             |

---

## 🧾 `LogsController` (Route: `/Logs`)

🔐 **Authorization**: Requires auth (some allow anonymous)

| Method | Endpoint                        | Description                                 |
|--------|----------------------------------|---------------------------------------------|
| GET    | `/GetLogs`                      | Get logs (admin only)                       |
| GET    | `/GetAllLogsToSharePoint`       | Get logs for SharePoint (anonymous allowed) |
| POST   | `/InsertAllLogsToDB`            | Insert logs to DB (anonymous allowed)       |

---

## 💲 `NadacController` (Route: `/nadac`)

| Method | Endpoint                  | Description                        |
|--------|----------------------------|------------------------------------|
| GET    | `/UpateNadacPrices`       | Update NADAC prices from CSV URL   |

---

## 📦 `OrderController` (Route: `/order`)

| Method | Endpoint                       | Description                                 |
|--------|----------------------------------|---------------------------------------------|
| GET    | `/GetAllOrders`                | Get all orders                              |
| POST   | `/CreateOrder`                 | Create a new order                          |
| GET    | `/GetAllOrdersByUserId`        | Get orders by user ID (anonymous allowed)   |

---

## 🧩 `SharePointController` (Route: `/sharePoint`)

🔐 **Authorization**: Requires `Admin`

| Method | Endpoint             | Description                  |
|--------|-----------------------|------------------------------|
| GET    | `/token-test`        | Test token validity          |

---

## 👤 `UserController` (Route: `/user`)

| Method | Endpoint                   | Description                              |
|--------|-----------------------------|------------------------------------------|
| POST   | `/`                        | Register a new user                      |
| POST   | `/login`                   | Login and issue tokens                   |
| GET    | `/token-test`              | Test authorization                       |
| POST   | `/access-token`            | Generate new token via refresh token     |
| GET    | `/UserById`                | Get current user details (Pharmacist)    |
| PUT    | `/UpdateUser`              | Update current user                      |
| DELETE | `/id`                      | Delete user by ID                        |
| GET    | `/`                        | Get all users                            |
| GET    | `/allCrid`                 | Get user credentials (anonymous allowed) |
| POST   | `/InsertUserData`          | Bulk insert users (anonymous allowed)    |
| GET    | `/Logout`                  | Logout                                   |
