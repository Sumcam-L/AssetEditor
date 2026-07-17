# IDA MCP Auto-Broker Launcher Design

## Goal

Start the QiuChenly enhanced IDA MCP Broker automatically when OpenCode starts its local MCP process, share one Broker across multiple OpenCode windows, and stop that Broker after the last launcher exits.

## Architecture

Create `C:\Users\Sumcam\.config\opencode\scripts\ida_mcp_launcher.py` as the local MCP command. The launcher owns lifecycle coordination only; the installed `ida_pro_mcp.server` package remains unchanged.

The launcher uses a lock file and JSON state file under `C:\Users\Sumcam\.config\opencode\state`. State records the Broker PID, whether the Broker was started by the launcher, and active launcher PIDs. Under the lock, each launcher removes dead PIDs, starts the Broker when `/status` is unavailable, and registers itself. It then runs the stdio MCP as a foreground child with inherited stdin/stdout.

Broker output is redirected to `C:\Users\Sumcam\.config\opencode\logs\ida-mcp-broker.log`; no launcher or Broker diagnostics may be written to stdout because stdout is reserved for MCP JSON-RPC.

When a launcher exits, it unregisters itself under the lock. The last launcher terminates the Broker only if the state proves that the launcher group started that exact PID. A manually started Broker is reused and never terminated.

## Failure Handling

- If state is stale, dead launcher and Broker PIDs are removed before decisions are made.
- If port 13337 responds to `/status`, it is treated as an existing Broker.
- If port 13337 is occupied by a non-Broker service, startup fails with a diagnostic on stderr and a non-zero exit code.
- Broker startup waits for `/status` with a bounded timeout and terminates the spawned process on timeout.
- Signals and normal process exit both run launcher cleanup.

## OpenCode Integration

Replace the current disabled `ida-pro-mcp` command in `C:\Users\Sumcam\.config\opencode\opencode.json` with the launcher path and set `enabled` to `true`. OpenCode must be fully restarted because MCP configuration is loaded only at startup.

## Verification

Automated tests use a temporary state/log directory and a fake HTTP Broker/stdio child to cover initial startup, Broker reuse, multi-client reference tracking, last-client shutdown, stale PID cleanup, and manual Broker preservation. A live smoke test checks that port 13337 starts through the launcher and that the IDA plugin can register.
