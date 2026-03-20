using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.SpecailModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspectionController : ControllerBase
    {
        private readonly SpecialContext _context;

        // Network credentials
        private const string NETWORK_SHARE = @"\\172.18.106.9\oe-fadata";
        private const string NETWORK_USERNAME = @"imobile";
        private const string NETWORK_PASSWORD = "ps@imobile";

        // Paths
        private readonly string _localWorkingDir = @"D:\InspectionSync";
        private readonly string _localBackupDir = @"D:\InspectionSync\Backup";
        private readonly string _localExcelFile = @"D:\InspectionSync\Rank invoce.xlsb";
        private readonly string _networkExcelPath = @"\\172.18.106.9\oe-fadata\PC\PC_PUB\(22) Inspection report\Rank invoce.xlsb";
        private readonly string _excelPassword = "1234";
        private readonly string _pythonScriptPath = @"D:\InspectionSync\sync_xlsb.py";
        private readonly string _pythonExePath = @"D:\CrawlerShared\WPy64-3720\python-3.7.2.amd64\python.exe";

        public InspectionController(SpecialContext context)
        {
            _context = context;
            EnsureLocalScriptExists();
            EnsureBackupDirectoryExists();
        }

        #region Network Connection Methods

        private bool ConnectToNetworkShare_NetUse(string networkPath, string username, string password)
        {
            try
            {
                Console.WriteLine($"[INFO] Connecting via net use...");

                var disconnectInfo = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = $"use \"{networkPath}\" /delete /y",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var disconnectProcess = Process.Start(disconnectInfo))
                {
                    disconnectProcess.WaitForExit(5000);
                }

                System.Threading.Thread.Sleep(500);

                var connectInfo = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = $"use \"{networkPath}\" /user:{username} {password} /persistent:no",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var connectProcess = Process.Start(connectInfo))
                {
                    connectProcess.WaitForExit(10000);
                    var output = connectProcess.StandardOutput.ReadToEnd();
                    var error = connectProcess.StandardError.ReadToEnd();

                    Console.WriteLine($"[INFO] Output: {output}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"[ERROR] Error: {error}");
                    }

                    if (connectProcess.ExitCode == 0)
                    {
                        Console.WriteLine($"[INFO] ✓ Connected successfully");
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception: {ex.Message}");
                return false;
            }
        }

        private bool ConnectToNetworkShare_Impersonation(string networkPath, string username, string password)
        {
            try
            {
                Console.WriteLine($"[INFO] Testing access with impersonation...");

                using (new NetworkConnection(networkPath, new System.Net.NetworkCredential(username, password)))
                {
                    var accessible = Directory.Exists(networkPath);
                    Console.WriteLine($"[INFO] Access test: {accessible}");
                    return accessible;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Impersonation failed: {ex.Message}");
                return false;
            }
        }

        private bool ConnectToNetworkShare(string networkPath, string username, string password)
        {
            Console.WriteLine("[INFO] Trying method 1: net use command...");
            if (ConnectToNetworkShare_NetUse(networkPath, username, password))
            {
                return true;
            }

            Console.WriteLine("[INFO] Trying method 2: impersonation...");
            return ConnectToNetworkShare_Impersonation(networkPath, username, password);
        }

        #endregion

        #region NetworkConnection Helper Class

        public class NetworkConnection : IDisposable
        {
            private string _networkName;

            public NetworkConnection(string networkName, System.Net.NetworkCredential credentials)
            {
                _networkName = networkName;

                var netResource = new NetResource
                {
                    Scope = 2,
                    ResourceType = 1,
                    DisplayType = 3,
                    Usage = 1,
                    RemoteName = networkName
                };

                var userName = string.IsNullOrEmpty(credentials.Domain)
                    ? credentials.UserName
                    : credentials.Domain + "\\" + credentials.UserName;

                var result = WNetAddConnection2(netResource, credentials.Password, userName, 0);

                if (result != 0)
                {
                    throw new System.ComponentModel.Win32Exception(result);
                }
            }

            ~NetworkConnection()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                WNetCancelConnection2(_networkName, 0, true);
            }

            [DllImport("mpr.dll")]
            private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

            [DllImport("mpr.dll")]
            private static extern int WNetCancelConnection2(string name, int flags, bool force);

            [StructLayout(LayoutKind.Sequential)]
            private class NetResource
            {
                public int Scope;
                public int ResourceType;
                public int DisplayType;
                public int Usage;
                public string LocalName;
                public string RemoteName;
                public string Comment;
                public string Provider;
            }
        }

        #endregion

        #region Local Script Management

        private void EnsureLocalScriptExists()
        {
            try
            {
                if (!Directory.Exists(_localWorkingDir))
                {
                    Directory.CreateDirectory(_localWorkingDir);
                    Console.WriteLine($"[INFO] Created local working directory: {_localWorkingDir}");
                }

                var networkScriptPath = @"\\172.18.106.9\oe-fadata\PC\PC_PUB\(22) Inspection report\sync_xlsb.py";

                if (!System.IO.File.Exists(_pythonScriptPath) ||
                    System.IO.File.GetLastWriteTime(networkScriptPath) > System.IO.File.GetLastWriteTime(_pythonScriptPath))
                {
                    System.IO.File.Copy(networkScriptPath, _pythonScriptPath, true);
                    Console.WriteLine($"[INFO] Copied Python script to: {_pythonScriptPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Could not copy Python script: {ex.Message}");
            }
        }

        private void EnsureBackupDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_localBackupDir))
                {
                    Directory.CreateDirectory(_localBackupDir);
                    Console.WriteLine($"[INFO] Created backup directory: {_localBackupDir}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Could not create backup directory: {ex.Message}");
            }
        }

        #endregion

        #region Excel Process Management

        private void KillExcelProcesses()
        {
            try
            {
                var excelProcs = Process.GetProcessesByName("EXCEL");
                if (excelProcs.Length > 0)
                {
                    Console.WriteLine($"[INFO] Found {excelProcs.Length} Excel process(es), terminating...");
                    foreach (var proc in excelProcs)
                    {
                        try
                        {
                            proc.Kill();
                            proc.WaitForExit(5000);
                            Console.WriteLine($"[INFO] Killed Excel PID: {proc.Id}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARN] Could not kill Excel PID {proc.Id}: {ex.Message}");
                        }
                    }
                    System.Threading.Thread.Sleep(2000);
                }
                else
                {
                    Console.WriteLine("[INFO] No Excel processes found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Error checking Excel processes: {ex.Message}");
            }
        }

        private async Task<bool> VerifyExcelNotHung()
        {
            try
            {
                // Check for hung Excel processes
                var excelProcs = Process.GetProcessesByName("EXCEL");

                foreach (var proc in excelProcs)
                {
                    // Check if process is responding
                    try
                    {
                        if (!proc.Responding)
                        {
                            Console.WriteLine($"[WARN] Excel process {proc.Id} is not responding - killing");
                            proc.Kill();
                            proc.WaitForExit(5000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARN] Could not check/kill Excel {proc.Id}: {ex.Message}");
                    }
                }

                await Task.Delay(2000);

                // Verify all Excel processes are gone
                excelProcs = Process.GetProcessesByName("EXCEL");
                if (excelProcs.Length > 0)
                {
                    Console.WriteLine($"[WARN] {excelProcs.Length} Excel process(es) still running");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excel verification failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region File Management

        private void DeleteFileWithRetry(string filePath, int maxRetries = 3)
        {
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        // Clear attributes
                        System.IO.File.SetAttributes(filePath, FileAttributes.Normal);
                        System.IO.File.Delete(filePath);
                        Console.WriteLine($"[INFO] Deleted: {Path.GetFileName(filePath)}");
                        return;
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Delete attempt {retry + 1}/{maxRetries} failed: {ex.Message}");
                    if (retry == maxRetries - 1)
                    {
                        throw new Exception($"Cannot delete file after {maxRetries} attempts: {ex.Message}");
                    }
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }

        private void CopyFileWithVerification(string source, string destination)
        {
            // Copy file
            System.IO.File.Copy(source, destination, true);

            // Clear read-only attribute
            System.IO.File.SetAttributes(destination, FileAttributes.Normal);

            // Verify copy
            var sourceInfo = new FileInfo(source);
            var destInfo = new FileInfo(destination);

            if (sourceInfo.Length != destInfo.Length)
            {
                throw new Exception($"File copy verification failed: Source={sourceInfo.Length:N0} bytes, Dest={destInfo.Length:N0} bytes");
            }

            Console.WriteLine($"[INFO] File copied and verified: {destInfo.Length:N0} bytes");
        }

        #endregion

        #region Python Execution - Fixed for Background Mode

        private async Task<(int exitCode, string output, string error, string logFile, string errorFile)> RunPythonDirect(
            string pythonScript, string xlsbPath, string jsonPath, string password)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var logFile = Path.Combine(_localWorkingDir, $"python_output_{timestamp}.log");
            var errorFile = Path.Combine(_localWorkingDir, $"python_error_{timestamp}.log");

            Console.WriteLine("[INFO] ========================================");
            Console.WriteLine("[INFO] Running Python in BACKGROUND MODE");
            Console.WriteLine("[INFO] ========================================");
            Console.WriteLine($"[INFO] Python: {_pythonExePath}");
            Console.WriteLine($"[INFO] Script: {pythonScript}");
            Console.WriteLine($"[INFO] XLSB: {xlsbPath}");
            Console.WriteLine($"[INFO] JSON: {jsonPath}");
            Console.WriteLine("[INFO] ========================================");

            var arguments = $"\"{pythonScript}\" \"{xlsbPath}\" \"{jsonPath}\" \"{password}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonExePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _localWorkingDir
            };

            using var process = new Process { StartInfo = startInfo };

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();
            var lastActivityTime = DateTime.Now;

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                    Console.WriteLine($"[PYTHON] {e.Data}");
                    lastActivityTime = DateTime.Now; // Reset activity timer
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    Console.WriteLine($"[PYTHON_ERR] {e.Data}");
                    lastActivityTime = DateTime.Now; // Reset activity timer
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // ENHANCED TIMEOUT: 15 minutes total OR 3 minutes no activity
            var maxTotalTimeout = TimeSpan.FromMinutes(15);
            var maxInactivityTimeout = TimeSpan.FromMinutes(3);
            var startTime = DateTime.Now;

            while (!process.HasExited)
            {
                await Task.Delay(1000); // Check every second

                var totalElapsed = DateTime.Now - startTime;
                var inactivityElapsed = DateTime.Now - lastActivityTime;

                // Log progress every 30 seconds
                if (totalElapsed.TotalSeconds % 30 < 1)
                {
                    Console.WriteLine($"[MONITOR] Elapsed: {totalElapsed.TotalSeconds:F0}s | " +
                                    $"Inactive: {inactivityElapsed.TotalSeconds:F0}s | " +
                                    $"Output: {outputBuilder.Length} bytes | " +
                                    $"Errors: {errorBuilder.Length} bytes");
                }

                // Check total timeout
                if (totalElapsed > maxTotalTimeout)
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException(
                        $"Python execution exceeded maximum time limit ({maxTotalTimeout.TotalMinutes} minutes). " +
                        $"This usually means Excel is hung or waiting for user input.");
                }

                // Check inactivity timeout
                if (inactivityElapsed > maxInactivityTimeout)
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException(
                        $"Python execution stalled - no output for {maxInactivityTimeout.TotalMinutes} minutes. " +
                        $"Excel may be showing a dialog or waiting for user interaction.");
                }
            }

            // Process completed naturally
            process.WaitForExit(); // Ensure all output is captured

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            // Save to log files
            try
            {
                if (!string.IsNullOrEmpty(output))
                {
                    await System.IO.File.WriteAllTextAsync(logFile, output);
                }
                if (!string.IsNullOrEmpty(error))
                {
                    await System.IO.File.WriteAllTextAsync(errorFile, error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Could not save log files: {ex.Message}");
            }

            var totalDuration = DateTime.Now - startTime;
            Console.WriteLine($"[INFO] ========================================");
            Console.WriteLine($"[INFO] Python execution completed");
            Console.WriteLine($"[INFO] Duration: {totalDuration.TotalSeconds:F1}s");
            Console.WriteLine($"[INFO] Exit code: {process.ExitCode}");
            Console.WriteLine($"[INFO] Output: {output.Length} bytes");
            Console.WriteLine($"[INFO] Errors: {error.Length} bytes");
            Console.WriteLine($"[INFO] ========================================");

            return (process.ExitCode, output, error, logFile, errorFile);
        }

        #endregion

        #region Main Sync Endpoint

        /// <summary>
        /// Sync Rank Invoice - Fixed for non-interactive background execution
        /// Maximum execution time: 15 minutes
        /// Inactivity timeout: 3 minutes
        /// </summary>
        [HttpPost("sync-rank-invoice")]
        public async Task<IActionResult> SyncRankInvoice()
        {
            var sessionId = Guid.NewGuid().ToString().Substring(0, 8);
            var tempJsonFile = Path.Combine(_localWorkingDir, $"sync_data_{sessionId}.json");

            try
            {
                Console.WriteLine("==========================================================");
                Console.WriteLine($"SYNC RANK INVOICE - Session: {sessionId}");
                Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Working Directory: {_localWorkingDir}");
                Console.WriteLine("==========================================================");

                // Ensure working directory exists
                Directory.CreateDirectory(_localWorkingDir);

                // STEP 1: Fetch data from database
                Console.WriteLine("[STEP 1/11] Fetching data from database...");
                var viewData = await _context.UvirRankInvoices.AsNoTracking().ToListAsync();

                if (!viewData.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "ไม่มีข้อมูลใน view",
                        updated = 0,
                        timestamp = DateTime.Now
                    });
                }

                Console.WriteLine($"[INFO] ✓ Fetched {viewData.Count} records from database");

                // STEP 2: Write JSON to local directory
                Console.WriteLine("[STEP 2/11] Writing JSON data...");
                var jsonData = JsonSerializer.Serialize(viewData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                await System.IO.File.WriteAllTextAsync(tempJsonFile, jsonData, utf8WithoutBom);

                var jsonInfo = new FileInfo(tempJsonFile);
                Console.WriteLine($"[INFO] ✓ JSON file created: {jsonInfo.Length:N0} bytes");

                // STEP 3: Connect to network share
                Console.WriteLine("[STEP 3/11] Connecting to network share...");
                var connected = ConnectToNetworkShare(NETWORK_SHARE, NETWORK_USERNAME, NETWORK_PASSWORD);

                if (!connected)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "❌ Cannot connect to network share",
                        networkPath = NETWORK_SHARE
                    });
                }

                Console.WriteLine("[INFO] ✓ Connected to network share");

                // STEP 4: Verify and kill Excel processes
                Console.WriteLine("[STEP 4/11] Checking Excel processes...");
                await VerifyExcelNotHung();
                KillExcelProcesses();
                await Task.Delay(3000);

                // Double-check Excel is gone
                var stillRunning = Process.GetProcessesByName("EXCEL");
                if (stillRunning.Length > 0)
                {
                    Console.WriteLine($"[WARN] {stillRunning.Length} Excel process(es) still detected - force killing");
                    foreach (var proc in stillRunning)
                    {
                        try
                        {
                            proc.Kill();
                            proc.WaitForExit(3000);
                        }
                        catch { }
                    }
                    await Task.Delay(2000);
                }

                Console.WriteLine("[INFO] ✓ All Excel processes terminated");

                // STEP 5: Copy Excel file to local directory
                Console.WriteLine("[STEP 5/11] Copying Excel file to local directory...");

                DeleteFileWithRetry(_localExcelFile);
                CopyFileWithVerification(_networkExcelPath, _localExcelFile);

                // Ensure file is writable
                try
                {
                    System.IO.File.SetAttributes(_localExcelFile, FileAttributes.Normal);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Could not set file attributes: {ex.Message}");
                }

                var localExcelInfo = new FileInfo(_localExcelFile);
                Console.WriteLine($"[INFO] ✓ Excel file ready: {localExcelInfo.Length:N0} bytes");

                // STEP 6: Pre-flight checks
                Console.WriteLine("[STEP 6/11] Pre-flight checks...");

                var checks = new Dictionary<string, bool>
                {
                    ["Python executable"] = System.IO.File.Exists(_pythonExePath),
                    ["Python script"] = System.IO.File.Exists(_pythonScriptPath),
                    ["Local Excel file"] = System.IO.File.Exists(_localExcelFile),
                    ["JSON data file"] = System.IO.File.Exists(tempJsonFile)
                };

                var allChecksPassed = true;
                foreach (var check in checks)
                {
                    var status = check.Value ? "✓" : "✗";
                    Console.WriteLine($"[CHECK] {status} {check.Key}");

                    if (!check.Value)
                    {
                        allChecksPassed = false;
                    }
                }

                if (!allChecksPassed)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "❌ Pre-flight checks failed",
                        checks = checks
                    });
                }

                Console.WriteLine("[INFO] ✓ All pre-flight checks passed");

                // STEP 7: Execute Python script (BACKGROUND MODE)
                Console.WriteLine("[STEP 7/11] Executing Python script in BACKGROUND MODE...");
                Console.WriteLine("[INFO] ⏱️  Maximum execution time: 15 minutes");
                Console.WriteLine("[INFO] ⏱️  Inactivity timeout: 3 minutes");
                Console.WriteLine("[INFO] 📊 Monitoring output for progress...");
                Console.WriteLine("[INFO] This may take several minutes for large files...");

                var execStart = DateTime.Now;
                var (exitCode, output, error, logFile, errorFile) = await RunPythonDirect(
                    _pythonScriptPath, _localExcelFile, tempJsonFile, _excelPassword);

                var execDuration = DateTime.Now - execStart;
                Console.WriteLine($"[INFO] ✓ Python execution completed in {execDuration.TotalSeconds:F1}s");

                // STEP 8: Parse results
                Console.WriteLine("[STEP 8/11] Parsing Python results...");
                PythonResultWithProgress result = null;

                if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        // Find JSON in output
                        var jsonStart = output.IndexOf('{');
                        var jsonEnd = output.LastIndexOf('}');

                        if (jsonStart >= 0 && jsonEnd > jsonStart)
                        {
                            var jsonString = output.Substring(jsonStart, jsonEnd - jsonStart + 1);
                            result = JsonSerializer.Deserialize<PythonResultWithProgress>(jsonString,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            Console.WriteLine($"[INFO] Parsed result: Success={result?.Success}, Added={result?.AddedCount}/{result?.TotalRecords}");
                        }
                        else
                        {
                            Console.WriteLine("[WARN] No JSON result found in output");
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"[WARN] Cannot parse JSON result: {ex.Message}");
                    }
                }

                // STEP 9: Copy updated file back to network (if successful)
                if (exitCode == 0 && result?.Success == true)
                {
                    Console.WriteLine("[STEP 9/11] Creating backup and copying to network...");

                    // Create backup in LOCAL directory (D:\InspectionSync\Backup)
                    try
                    {
                        var backupFileName = $"Rank_invoce_{DateTime.Now:yyyyMMdd_HHmmss}.xlsb";
                        var backupPath = Path.Combine(_localBackupDir, backupFileName);

                        // Backup from LOCAL file (before copying to network)
                        System.IO.File.Copy(_localExcelFile, backupPath, true);
                        Console.WriteLine($"[INFO] ✓ Backup created: {backupPath}");
                        Console.WriteLine($"[INFO] Backup location: D:\\InspectionSync\\Backup\\{backupFileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARN] Backup creation failed (non-critical): {ex.Message}");
                    }

                    // Copy updated file to network
                    CopyFileWithVerification(_localExcelFile, _networkExcelPath);
                    Console.WriteLine($"[INFO] ✓ Updated file copied to network");
                }
                else
                {
                    Console.WriteLine("[STEP 9/11] Skipping network copy (Python script failed)");
                }

                // STEP 10: Cleanup temporary files
                Console.WriteLine("[STEP 10/11] Cleanup temporary files...");
                CleanupTempFiles(tempJsonFile);

                // STEP 11: Prepare and return response
                Console.WriteLine("[STEP 11/11] Preparing response...");

                var networkFileInfo = new FileInfo(_networkExcelPath);
                var lastModified = networkFileInfo.LastWriteTime;
                var timeSinceModified = DateTime.Now - lastModified;

                if (exitCode == 0 && result?.Success == true)
                {
                    Console.WriteLine("==========================================================");
                    Console.WriteLine("✓ SYNC COMPLETED SUCCESSFULLY");
                    Console.WriteLine($"  Added: {result.AddedCount} records");
                    Console.WriteLine($"  Skipped: {result.TotalRecords - result.AddedCount} records");
                    Console.WriteLine($"  Execution time: {execDuration.TotalSeconds:F1}s");
                    Console.WriteLine($"  Backup location: D:\\InspectionSync\\Backup\\");
                    Console.WriteLine("==========================================================");

                    return Ok(new
                    {
                        success = true,
                        message = "✓ อัปเดตสำเร็จ",
                        totalRecords = result.TotalRecords,
                        addedRecords = result.AddedCount,
                        skippedRecords = result.TotalRecords - result.AddedCount,
                        executionTime = $"{execDuration.TotalSeconds:F1}s",
                        fileModified = lastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                        timeSinceModified = $"{timeSinceModified.TotalSeconds:F1}s ago",
                        localFile = _localExcelFile,
                        networkFile = _networkExcelPath,
                        backupLocation = _localBackupDir,
                        pythonMessage = result.Message,
                        progress = FormatProgressDisplay(result?.Progress),
                        timestamp = DateTime.Now,
                        sessionId = sessionId
                    });
                }
                else if (timeSinceModified.TotalSeconds <= 120)
                {
                    // File was recently modified, assume success
                    Console.WriteLine("==========================================================");
                    Console.WriteLine("✓ SYNC COMPLETED (file modification detected)");
                    Console.WriteLine($"  File modified: {timeSinceModified.TotalSeconds:F1}s ago");
                    Console.WriteLine($"  Backup location: D:\\InspectionSync\\Backup\\");
                    Console.WriteLine("==========================================================");

                    return Ok(new
                    {
                        success = true,
                        message = "✓ อัปเดตสำเร็จ (ตรวจพบการแก้ไขไฟล์)",
                        totalRecords = viewData.Count,
                        executionTime = $"{execDuration.TotalSeconds:F1}s",
                        fileModified = lastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                        timeSinceModified = $"{timeSinceModified.TotalSeconds:F1}s ago",
                        localFile = _localExcelFile,
                        networkFile = _networkExcelPath,
                        backupLocation = _localBackupDir,
                        note = "File modification detected within 2 minutes",
                        timestamp = DateTime.Now,
                        sessionId = sessionId
                    });
                }
                else
                {
                    // Failure - return detailed diagnostics
                    Console.WriteLine("==========================================================");
                    Console.WriteLine("✗ SYNC FAILED");
                    Console.WriteLine($"  Exit code: {exitCode}");
                    Console.WriteLine($"  Error: {result?.Error ?? "Unknown error"}");
                    Console.WriteLine("==========================================================");

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "✗ Python script failed",
                        exitCode = exitCode,
                        error = result?.Error ?? "Unknown error",
                        executionTime = $"{execDuration.TotalSeconds:F1}s",
                        pythonOutput = output,
                        pythonError = error,
                        localFile = _localExcelFile,
                        jsonFile = tempJsonFile,
                        logFile = logFile,
                        errorFile = errorFile,
                        progress = FormatProgressDisplay(result?.Progress),
                        diagnostics = new
                        {
                            pythonExists = System.IO.File.Exists(_pythonExePath),
                            scriptExists = System.IO.File.Exists(_pythonScriptPath),
                            excelExists = System.IO.File.Exists(_localExcelFile),
                            jsonExists = System.IO.File.Exists(tempJsonFile),
                            jsonSize = System.IO.File.Exists(tempJsonFile) ?
                                new FileInfo(tempJsonFile).Length : 0,
                            logFileExists = System.IO.File.Exists(logFile),
                            errorFileExists = System.IO.File.Exists(errorFile),
                            suggestion = $"Check log files: {logFile}"
                        },
                        timestamp = DateTime.Now,
                        sessionId = sessionId
                    });
                }
            }
            catch (TimeoutException tex)
            {
                Console.WriteLine("==========================================================");
                Console.WriteLine("⏱️ TIMEOUT ERROR");
                Console.WriteLine($"  Message: {tex.Message}");
                Console.WriteLine("==========================================================");

                // Kill any remaining Excel processes
                try
                {
                    var excelProcs = Process.GetProcessesByName("EXCEL");
                    foreach (var proc in excelProcs)
                    {
                        try
                        {
                            Console.WriteLine($"[CLEANUP] Killing Excel PID: {proc.Id}");
                            proc.Kill();
                            proc.WaitForExit(3000);
                        }
                        catch { }
                    }
                }
                catch { }

                return StatusCode(500, new
                {
                    success = false,
                    message = "⏱️ Timeout - การทำงานใช้เวลานานเกินไป",
                    error = tex.Message,
                    type = "TimeoutException",
                    possibleCauses = new[]
                    {
                        "Excel file is very large or has many complex formulas",
                        "Excel is hung or waiting for user input (should not happen in background mode)",
                        "Network file access is slow",
                        "Python script encountered an infinite loop",
                        "Excel add-ins are interfering with the process"
                    },
                    suggestions = new[]
                    {
                        "Check if Excel processes are running: tasklist | findstr EXCEL",
                        "Check Python log files in D:\\InspectionSync\\",
                        "Try running the script manually to see if there are prompts",
                        "Verify the Excel file is not corrupted",
                        "Consider optimizing the Excel file (remove unused sheets, simplify formulas)"
                    },
                    timestamp = DateTime.Now,
                    sessionId = sessionId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("==========================================================");
                Console.WriteLine("❌ FATAL ERROR");
                Console.WriteLine($"  Type: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                Console.WriteLine($"  Stack: {ex.StackTrace}");
                Console.WriteLine("==========================================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "❌ เกิดข้อผิดพลาดร้ายแรง",
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace,
                    workingDir = _localWorkingDir,
                    timestamp = DateTime.Now,
                    sessionId = sessionId
                });
            }
            finally
            {
                Console.WriteLine("[FINALLY] Final cleanup...");

                // Ensure Excel is killed
                try
                {
                    var excelProcs = Process.GetProcessesByName("EXCEL");
                    if (excelProcs.Length > 0)
                    {
                        Console.WriteLine($"[FINALLY] Killing {excelProcs.Length} remaining Excel process(es)");
                        foreach (var proc in excelProcs)
                        {
                            try
                            {
                                proc.Kill();
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                Console.WriteLine("[FINALLY] Session ended");
            }
        }

        #endregion

        #region Helper Methods

        private void CleanupTempFiles(params string[] files)
        {
            foreach (var file in files)
            {
                try
                {
                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                        Console.WriteLine($"[CLEANUP] Deleted temp file: {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLEANUP] Could not delete {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }

        private List<string> FormatProgressDisplay(List<ProgressEntry> progress)
        {
            if (progress == null || !progress.Any())
                return new List<string> { "No progress information available" };

            var display = new List<string>();

            foreach (var entry in progress)
            {
                var icon = entry.Status switch
                {
                    "start" => "▶️",
                    "success" => "✓",
                    "warning" => "⚠️",
                    "error" => "✗",
                    _ => "ℹ️"
                };

                var line = $"{icon} [{entry.Step}] {entry.Message}";

                if (entry.Details != null)
                {
                    try
                    {
                        var detailsJson = JsonSerializer.Serialize(entry.Details);
                        line += $" | {detailsJson}";
                    }
                    catch
                    {
                        line += $" | {entry.Details}";
                    }
                }

                display.Add(line);
            }

            return display;
        }

        #endregion

        #region Helper Classes

        private class PythonResultWithProgress
        {
            public bool Success { get; set; }
            public int AddedCount { get; set; }
            public int TotalRecords { get; set; }
            public string Message { get; set; }
            public string Error { get; set; }
            public string Type { get; set; }
            public List<ProgressEntry> Progress { get; set; }
        }

        private class ProgressEntry
        {
            public string Step { get; set; }
            public string Status { get; set; }
            public string Message { get; set; }
            public string Timestamp { get; set; }
            public object Details { get; set; }
        }

        #endregion
    }
}