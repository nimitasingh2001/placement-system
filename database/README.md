# Database scripts

SQL Server scripts that build the `PlacementDb` database from scratch.
Written and tested on SQL Server Express with SSMS (needs SQL Server 2016 SP1 or later).

## Run order

| # | File | What it does |
|---|---|---|
| 00 | `00_create_database.sql` | Creates the empty `PlacementDb` database |
| 01 | `01_schema.sql` | 9 tables with all constraints (**drops existing tables: data is lost**) |
| 02 | `02_indexes.sql` | 5 nonclustered indexes |
| 03 | `03_views.sql` | 5 views |
| 04 | `04_functions.sql` | `fn_StudentJobMatch`, `fn_MissingSkills` |
| 05 | `05_procedures.sql` | `usp_ApplyForJob`, `usp_UpdateApplicationStatus`, `usp_SelectApplication` |
| 06 | `06_seed_part1.sql` | Dev data: 15 skills, 6 companies, 30 students (**deletes existing rows**) |
| 07 | `07_seed_part2.sql` | Dev data: 12 jobs, applications, placements, skills |

Other files:

- `tests.sql`: every test block used while building (run one section at a time).
- `optional/queries.sql`: practice queries (JOINs, GROUP BY, skill match).
- `optional/bulk_data.sql`: 100,000-row load for index measurements.
- `optional/bulk_data_cleanup.sql`: removes the bulk rows again (indexes stay).
- `optional/index_experiments.sql`: logical-reads comparison, scan vs index.

## Option 1: SSMS

Open each file in order, make sure the right database is selected (files 01 to 07 start with `USE PlacementDb;`), and press F5. File 00 can run while connected to `master`.

## Option 2: command line (Command Prompt)

Replace the server name with yours (the "Server name" box in SSMS).

```
sqlcmd -S LAPTOP-CPG6JP9U\SQLEXPRESS -E -b -i 00_create_database.sql
for %f in (01_*.sql 02_*.sql 03_*.sql 04_*.sql 05_*.sql 06_*.sql 07_*.sql) do sqlcmd -S LAPTOP-CPG6JP9U\SQLEXPRESS -E -I -b -i "%f"
```

`-E` = Windows authentication, `-I` = quoted identifiers on (needed for the filtered index), `-b` = stop with an error code if a script fails. In a `.bat` file write `%%f` instead of `%f`.

## Notes

- Seed scripts are a **development reset**. Never run them on data you want to keep.
- `AppUser` is empty after seeding: the backend creates users (it hashes the passwords).
- Business rules live in the procedures; error numbers are listed at the top of `05_procedures.sql`.
- Do not commit `.bak` backup files (add `*.bak` to `.gitignore`).
