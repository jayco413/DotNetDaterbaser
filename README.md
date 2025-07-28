# DotNetDaterbaser  
*A zero-friction SQL-script runner for CI / local builds*

![.NET Core](https://img.shields.io/badge/.NET-9.0-blue.svg)
![License](https://img.shields.io/github/license/jayco413/DotNetDaterbaser)

DotNetDaterbaser is a **tiny command-line utility that automatically applies SQL Server scripts every time your solution builds**, eliminating the “works on my box” drift between source control and real databases.  
It was originally built to let ChatGPT/Codex generate application code while this tool quietly handled the database side, but it’s equally handy for any Dev/CI workflow.

---

## ✨ Key Features
| Feature | What it does |
|---------|--------------|
| 🔑 **Connection-agnostic** | Pass one _or many_ SQL Server connection strings; each database is processed independently. |
| 🗂 **Two-tier script model** | • **Full build script** (schema + seed) — runs once per DB.<br>• **Incremental scripts** — run exactly once in file-name order. |
| 📝 **State tracking** | `tracking.json` remembers which scripts have run per-server/per-database so re-runs are idempotent. |
| 🗁 **Fail-safe “GO” splitter** | Multi-batch scripts are split on `GO` statements and executed safely inside a single connection. |
| 📜 **Auto-generated helpers** | If missing, it creates `.gitignore` (adds `tracking.json`) and a starter `AGENTS.md` file for documentation. |
| 📋 **Simple log** | A per-DB log file (`<server>_<db>.log`) is appended after every successful run. |

---

## 🚀 Quick Start

1. **Build** the solution or grab the binary from your CI output.  
2. **Arrange your script folder** (e.g. `DatabaseScripts/`):

```
MyServer_MyDb_full_database_script.sql          <- full build (runs once)
MyServer_MyDb_20250728_001_script.sql           <- incremental patch 1
MyServer_MyDb_20250729_hotfix_script.sql        <- incremental patch 2
tracking.json                                   <- auto-generated
.gitignore                                      <- auto-generated (adds tracking.json)
AGENTS.md                                       <- optional docs

```

3. **Run**:

```bash
DotNetDaterbaser "<ConnStr1>" "<ConnStr2>" ./Logs ./DatabaseScripts
```

*Arguments*
1 … N  – SQL Server connection strings
N+1    – output directory for logs
N+2    – scripts directory (containing the files above)

4. Re-run the same command any time – already-executed scripts are skipped automatically.

---

## 🗂 Naming Conventions

| Script type     | Required pattern                                                          | Runs when…                           |
| --------------- | ------------------------------------------------------------------------- | ------------------------------------ |
| **Full build**  | `<server>_<db>_full_database_script.sql`                                  | `FullRun == false` for that database |
| **Incremental** | `<server>_<db>_*_script.sql` *(any other name ending with `_script.sql`)* | Not yet listed in `tracking.json`    |

Order for incremental scripts is the file-system sort order (keep dates or incremental numbers in the name). ([GitHub][1])

---

## 🔄 Typical CI Integration

Add a **pre-build step** to your pipeline:

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run database migrations'
  inputs:
    command: 'custom'
    custom: 'DotNetDaterbaser'
    arguments: '"$(DB_CONN_DEV)" "$(Build.ArtifactStagingDirectory)/logs" "$(Build.SourcesDirectory)/DatabaseScripts"'
```

---

## 🛠 Extending

* **Different RDBMS?** Swap `Microsoft.Data.SqlClient` for another provider and adjust the `GO` splitter if required.
* **Existing migration history?** Pre-populate `tracking.json` with `"FullRun": true` and a list of historical script names.
* **Custom logging** – substitute the simple `File.AppendAllTextAsync` call with your own structured logger.

---

## 🤝 Contributing

Pull requests and issues are welcome!
For substantial changes, please open an issue first to discuss what you’d like to add or change.

---

## 📄 License

[MIT](LICENSE.txt)

---

> *Built with ❤️ and a desire to never manually “CTRL-SHIFT-E” a SQL script again.*

[1]: https://github.com/jayco413/DotNetDaterbaser/raw/master/DotNetDaterbaser/Program.cs "raw.githubusercontent.com"
