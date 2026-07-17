# IDA MCP Auto-Broker Launcher Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Automatically manage the enhanced IDA MCP Broker for one or more OpenCode windows without corrupting stdio MCP traffic.

**Architecture:** A standalone Python launcher coordinates Broker ownership and active clients through a cross-process lock and JSON state file. It starts the Broker only when needed, runs the installed stdio MCP in the foreground, and stops only a Broker it owns after the last launcher exits.

**Tech Stack:** Python 3.11 standard library, Windows `msvcrt` locking, OpenCode JSON configuration, `unittest`.

## Global Constraints

- Use `D:\Program Files\Python\Python311-32\python.exe` for Broker and stdio MCP processes.
- Reserve launcher stdout exclusively for MCP JSON-RPC.
- Reuse a healthy manually started Broker and never terminate it.
- Support multiple OpenCode windows; only the last launcher may stop an owned Broker.
- Store runtime state outside the AssetEditor repository.

---

### Task 1: Launcher Lifecycle and Tests

**Files:**
- Create: `C:\Users\Sumcam\.config\opencode\scripts\ida_mcp_launcher.py`
- Create: `C:\Users\Sumcam\.config\opencode\scripts\test_ida_mcp_launcher.py`

**Interfaces:**
- Produces: `main() -> int`, the executable OpenCode MCP entry point.
- Produces: `LauncherConfig`, configurable paths/commands used by tests and production.
- Consumes: enhanced package entry point `python -m ida_pro_mcp.server`.

- [ ] **Step 1: Write failing lifecycle tests**

Cover healthy Broker detection, state cleanup, ownership preservation, and last-client shutdown using temporary directories and injected process/HTTP functions.

- [ ] **Step 2: Run tests and verify RED**

Run:

```powershell
rtk "D:\Program Files\Python\Python311-32\python.exe" -m unittest "C:\Users\Sumcam\.config\opencode\scripts\test_ida_mcp_launcher.py" -v
```

Expected: import or missing-symbol failures because the launcher does not exist.

- [ ] **Step 3: Implement the launcher**

Implement:

- exclusive state locking with `msvcrt.locking`;
- atomic JSON state replacement;
- PID liveness checks;
- `/status` Broker health checks;
- hidden detached Broker launch with output redirected to the Broker log;
- foreground stdio MCP launch with inherited standard streams;
- signal/finally cleanup and owned-Broker termination after the last client;
- stderr-only diagnostics.

- [ ] **Step 4: Run tests and verify GREEN**

Run the same `unittest` command. Expected: all tests pass.

### Task 2: OpenCode Configuration and Live Verification

**Files:**
- Modify: `C:\Users\Sumcam\.config\opencode\opencode.json:292`

**Interfaces:**
- Consumes: launcher from Task 1.
- Produces: enabled local `ida-pro-mcp` MCP configuration.

- [ ] **Step 1: Update the MCP command**

Set the command to the Python interpreter plus `ida_mcp_launcher.py`, and set `enabled` to `true` without modifying unrelated configuration.

- [ ] **Step 2: Validate JSON**

Run:

```powershell
rtk "D:\Program Files\Python\Python311-32\python.exe" -m json.tool "C:\Users\Sumcam\.config\opencode\opencode.json"
```

Expected: exit code 0.

- [ ] **Step 3: Run a launcher smoke test**

Start a temporary launcher client, verify `/status` on port 13337, start a second client, stop the first and verify Broker remains, then stop the second and verify the owned Broker exits.

- [ ] **Step 4: Verify repository state**

Run:

```powershell
rtk git diff --check
rtk git status --short
```

Expected: no whitespace errors; only intended repository documentation and any pre-existing changes are listed.
