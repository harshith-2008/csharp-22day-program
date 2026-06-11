--Q1. Window Functions
SELECT
    p.FullName,
	d.Name,
    COUNT(e.EncounterId) AS EncounterCount,
    RANK()
    OVER (
        ORDER BY COUNT(e.EncounterId) DESC
    ) AS VolumeRank
FROM Provider p
LEFT JOIN Encounter e
    ON e.ProviderId = p.ProviderId
JOIN Department d ON d.DepartmentId = p.DepartmentId
GROUP BY
    p.ProviderId,
    p.FullName,
	d.Name;


--Q2. temporal table
-- step-1 - would be enabling temporal
ALTER TABLE [dbo].[Insurance]
ADD
    ValidFrom DATETIME2
        GENERATED ALWAYS AS ROW START HIDDEN
        CONSTRAINT DF_Insurance_From
        DEFAULT SYSUTCDATETIME(),

    ValidTo DATETIME2
        GENERATED ALWAYS AS ROW END HIDDEN
        CONSTRAINT DF_Insurance_To
        DEFAULT '9999-12-31 23:59:59.9999999',

    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo);

-- step-2 - wuld be enabling versioning
ALTER TABLE [dbo].[Insurance]
SET (
    SYSTEM_VERSIONING = ON
    (
        HISTORY_TABLE = dbo.Insurance_History
    )
);

-- FInal step - displaying report
SELECT
    InsuranceId,
    Payer,
	PolicyNumber,
    ValidFrom,
    ValidTo
FROM Insurance
FOR SYSTEM_TIME ALL
WHERE InsuranceId = 10
ORDER BY ValidFrom;

--Q3. Stored procedures, Views
WITH OrderedEncounters AS (
    SELECT
        PatientId,
        EncounterId,
        AdmitDate,
        DischargeDate,
        LAG(DischargeDate)
          OVER (
           PARTITION BY PatientId
           ORDER BY AdmitDate
         ) AS PreviousDischarge
    FROM Encounter
    WHERE EncounterType = 'Inpatient'
)
SELECT
    PatientId,
    EncounterId,
    AdmitDate,
    PreviousDischarge,
    DATEDIFF(
        DAY,
        PreviousDischarge,
        AdmitDate
    ) AS DaysBetweenVisits
FROM OrderedEncounters
WHERE PreviousDischarge IS NOT NULL
AND DATEDIFF(
        DAY,
        PreviousDischarge,
        AdmitDate
)<=30;

--Q4. Complex Query & sp
CREATE OR ALTER PROCEDURE usp_CeoReport
AS
BEGIN
    SET NOCOUNT ON;

    -- 1- Total Active Patients
    SELECT
        COUNT(*) AS TotalActivePatients
    FROM Patient
    WHERE IsActive = 1;

    -- 2 - Top 5 Departments by Encounters
    SELECT TOP 5
        d.Name AS Department,
        COUNT(e.EncounterId) AS TotalEncounters
    FROM Department d
    JOIN Encounter e
        ON d.DepartmentId = e.DepartmentId
    GROUP BY d.Name
    ORDER BY TotalEncounters DESC;

    -- 3 -  Denied Claims
    SELECT
        COUNT(*) AS DeniedClaims
    FROM Claim
    WHERE Status = 'Denied';
END;

exec usp_CeoReport
