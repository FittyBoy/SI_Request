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
        private readonly string _localExcelFile = @"D:\InspectionSync\Rank invoceForTest.xlsb";
        private readonly string _networkExcelPath = @"\\172.18.106.9\oe-fadata\PC\PC_PUB\(22) Inspection report\Rank invoceForTest.xlsb";
        private readonly string _excelPassword = "1234";
        private readonly string _pythonScriptPath = @"D:\InspectionSync\sync_xlsb.py";
        private readonly string _pythonExePath = @"D:\CrawlerShared\WPy64-3720\python-3.7.2.amd64\python.exe";
        private readonly string _psexecPath = @"D:\InspectionSync\PsExec64.exe";

        public InspectionController(SpecialContext context)
        {
            _context = context;
            EnsureLocalScriptExists();
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

        #endregion

        #region PsExec Helper

        private async Task<(int exitCode, string output, string error, string logFile, string errorFile)> RunPythonWithPsExec(
            string pythonScript, string xlsbPath, string jsonPath, string password)
        {
            if (!System.IO.File.Exists(_psexecPath))
            {
                throw new FileNotFoundException(
                    "PsExec64.exe not found. Download from: https://download.sysinternals.com/files/PSTools.zip",
                    _psexecPath);
            }

            // Verify all files exist before running
            if (!System.IO.File.Exists(_pythonExePath))
            {
                throw new FileNotFoundException($"Python not found: {_pythonExePath}");
            }
            if (!System.IO.File.Exists(pythonScript))
            {
                throw new FileNotFoundException($"Python script not found: {pythonScript}");
            }
            if (!System.IO.File.Exists(xlsbPath))
            {
                throw new FileNotFoundException($"Excel file not found: {xlsbPath}");
            }
            if (!System.IO.File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"JSON file not found: {jsonPath}");
            }

            // Create log files for Python output
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var logFile = Path.Combine(_localWorkingDir, $"python_output_{timestamp}.log");
            var errorFile = Path.Combine(_localWorkingDir, $"python_error_{timestamp}.log");

            // Use cmd /c to redirect Python output to files
            var pythonCommand = $@"""{_pythonExePath}"" ""{pythonScript}"" ""{xlsbPath}"" ""{jsonPath}"" ""{password}""";
            var arguments = $@"-accepteula -i -u imobile -p ""ps@imobile"" -w ""D:\InspectionSync"" " +
                           $@"cmd /c ""{pythonCommand} > \""{logFile}\"" 2> \""{errorFile}\""""";

            Console.WriteLine($"[INFO] Running PsExec with output redirection");
            Console.WriteLine($"[INFO] Python: {_pythonExePath}");
            Console.WriteLine($"[INFO] Script: {pythonScript}");
            Console.WriteLine($"[INFO] Excel: {xlsbPath}");
            Console.WriteLine($"[INFO] JSON: {jsonPath}");
            Console.WriteLine($"[INFO] Output log: {logFile}");
            Console.WriteLine($"[INFO] Error log: {errorFile}");

            var startInfo = new ProcessStartInfo
            {
                FileName = _psexecPath,
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

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                    Console.WriteLine($"[PSEXEC_OUT] {e.Data}");
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    Console.WriteLine($"[PSEXEC_ERR] {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var completed = process.WaitForExit(300000); // 5 minutes

            if (!completed)
            {
                try { process.Kill(); } catch { }
                throw new TimeoutException("PsExec/Python execution timeout (5 minutes)");
            }

            Console.WriteLine($"[INFO] Process exit code: {process.ExitCode}");

            // Wait a bit for files to be written
            await Task.Delay(2000);

            // Read log files
            string logContent = "";
            string errorContent = "";

            if (System.IO.File.Exists(logFile))
            {
                logContent = await System.IO.File.ReadAllTextAsync(logFile);
                Console.WriteLine($"[INFO] Log file size: {logContent.Length} bytes");
            }
            else
            {
                Console.WriteLine($"[WARN] Log file not created: {logFile}");
            }

            if (System.IO.File.Exists(errorFile))
            {
                errorContent = await System.IO.File.ReadAllTextAsync(errorFile);
                Console.WriteLine($"[INFO] Error file size: {errorContent.Length} bytes");
            }
            else
            {
                Console.WriteLine($"[WARN] Error file not created: {errorFile}");
            }

            return (process.ExitCode, logContent, errorContent, logFile, errorFile);
        }

        #endregion

        #region Main Sync Endpoint

        /// <summary>
        /// Sync Rank Invoice - ทำงานทั้งหมดใน D:\InspectionSync แล้วค่อย copy ไปเครือข่าย
        /// </summary>
        [HttpPost("sync-rank-invoice")]
        public async Task<IActionResult> SyncRankInvoice()
        {
            var tempId = Guid.NewGuid().ToString();
            var tempJsonFile = Path.Combine(_localWorkingDir, $"sync_data_{tempId}.json");

            try
            {
                Console.WriteLine("==========================================================");
                Console.WriteLine($"SYNC RANK INVOICE - ID: {tempId}");
                Console.WriteLine($"Working in: {_localWorkingDir}");
                Console.WriteLine("==========================================================");

                Directory.CreateDirectory(_localWorkingDir);

                // STEP 1: Fetch data
                Console.WriteLine("[STEP 1] Fetching data from database...");
                var viewData = await _context.UvirRankInvoices.AsNoTracking().ToListAsync();
                if (!viewData.Any())
                {
                    return Ok(new { message = "ไม่มีข้อมูลใน view", updated = 0 });
                }
                Console.WriteLine($"[INFO] Total records: {viewData.Count}");

                // STEP 2: Write JSON to D:\InspectionSync
                Console.WriteLine("[STEP 2] Writing JSON data...");
                var jsonData = JsonSerializer.Serialize(viewData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                await System.IO.File.WriteAllTextAsync(tempJsonFile, jsonData, utf8WithoutBom);
                Console.WriteLine($"[INFO] JSON saved: {tempJsonFile}");
                Console.WriteLine($"[INFO] JSON size: {new FileInfo(tempJsonFile).Length:N0} bytes");

                // STEP 3: Connect to network share
                Console.WriteLine("[STEP 3] Connecting to network share...");
                var connected = ConnectToNetworkShare(NETWORK_SHARE, NETWORK_USERNAME, NETWORK_PASSWORD);
                if (!connected)
                {
                    return StatusCode(500, new { message = "Cannot connect to network share" });
                }

                // STEP 4: Copy Excel to D:\InspectionSync\Rank invoceForTest.xlsb
                Console.WriteLine("[STEP 4] Copying Excel to local working directory...");
                if (System.IO.File.Exists(_localExcelFile))
                {
                    System.IO.File.Delete(_localExcelFile);
                    await Task.Delay(500);
                }
                System.IO.File.Copy(_networkExcelPath, _localExcelFile, true);
                Console.WriteLine($"[INFO] Excel copied to: {_localExcelFile}");
                Console.WriteLine($"[INFO] File size: {new FileInfo(_localExcelFile).Length:N0} bytes");

                // STEP 5: Run Python via PsExec
                Console.WriteLine("[STEP 5] Running Python via PsExec on local file...");
                var (exitCode, output, error, logFile, errorFile) = await RunPythonWithPsExec(
                    _pythonScriptPath, _localExcelFile, tempJsonFile, _excelPassword);
                Console.WriteLine($"[INFO] PsExec exit code: {exitCode}");

                // STEP 6: Copy updated file back to network
                Console.WriteLine("[STEP 6] Copying updated file back to network...");
                if (System.IO.File.Exists(_localExcelFile))
                {
                    // Create backup on network
                    var backupPath = Path.Combine(
                        Path.GetDirectoryName(_networkExcelPath),
                        $"backup_{DateTime.Now:yyyyMMdd_HHmmss}_Rank invoceForTest.xlsb");

                    try
                    {
                        System.IO.File.Copy(_networkExcelPath, backupPath, true);
                        Console.WriteLine($"[INFO] Backup created: {Path.GetFileName(backupPath)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARN] Backup failed: {ex.Message}");
                    }

                    // Copy updated file to network
                    System.IO.File.Copy(_localExcelFile, _networkExcelPath, true);
                    Console.WriteLine($"[INFO] ✓ Updated file copied to: {_networkExcelPath}");
                }

                // STEP 7: Parse results
                Console.WriteLine("[STEP 7] Parsing results...");
                PythonResultWithProgress result = null;
                if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        var jsonStart = output.IndexOf('{');
                        var jsonEnd = output.LastIndexOf('}');
                        if (jsonStart >= 0 && jsonEnd > jsonStart)
                        {
                            var jsonString = output.Substring(jsonStart, jsonEnd - jsonStart + 1);
                            result = JsonSerializer.Deserialize<PythonResultWithProgress>(jsonString,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"[WARN] Cannot parse JSON: {ex.Message}");
                    }
                }

                var networkFileInfo = new FileInfo(_networkExcelPath);
                var lastModified = networkFileInfo.LastWriteTime;
                var timeSinceModified = DateTime.Now - lastModified;

                // Parse result ก่อน cleanup
                if (exitCode == 0 && result?.Success == true)
                {
                    CleanupTempFiles(tempJsonFile);

                    return Ok(new
                    {
                        message = "✓ อัปเดตสำเร็จ",
                        totalRecords = result.TotalRecords,
                        addedRecords = result.AddedCount,
                        skippedRecords = result.TotalRecords - result.AddedCount,
                        fileModified = lastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                        localFile = _localExcelFile,
                        networkFile = _networkExcelPath,
                        pythonMessage = result.Message,
                        progress = FormatProgressDisplay(result?.Progress)
                    });
                }
                else if (timeSinceModified.TotalSeconds <= 120)
                {
                    CleanupTempFiles(tempJsonFile);

                    return Ok(new
                    {
                        message = "✓ อัปเดตสำเร็จ (ตรวจพบการแก้ไขไฟล์)",
                        totalRecords = viewData.Count,
                        fileModified = lastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                        timeSinceModified = $"{timeSinceModified.TotalSeconds:F1}s",
                        localFile = _localExcelFile,
                        networkFile = _networkExcelPath
                    });
                }
                else
                {
                    // เก็บไฟล์ไว้ debug ในกรณี error
                    return StatusCode(500, new
                    {
                        message = "✗ Python script failed",
                        exitCode = exitCode,
                        error = result?.Error ?? "Unknown error",
                        pythonOutput = output,
                        pythonError = error,
                        localFile = _localExcelFile,
                        jsonFile = tempJsonFile,
                        logFile = logFile,
                        errorFile = errorFile,
                        diagnostics = new
                        {
                            pythonExists = System.IO.File.Exists(_pythonExePath),
                            scriptExists = System.IO.File.Exists(_pythonScriptPath),
                            excelExists = System.IO.File.Exists(_localExcelFile),
                            jsonExists = System.IO.File.Exists(tempJsonFile),
                            psexecExists = System.IO.File.Exists(_psexecPath),
                            jsonSize = System.IO.File.Exists(tempJsonFile) ?
                                new FileInfo(tempJsonFile).Length : 0,
                            logFileExists = System.IO.File.Exists(logFile),
                            errorFileExists = System.IO.File.Exists(errorFile),
                            logFileSize = System.IO.File.Exists(logFile) ?
                                new FileInfo(logFile).Length : 0,
                            errorFileSize = System.IO.File.Exists(errorFile) ?
                                new FileInfo(errorFile).Length : 0,
                            suggestion = "Check log files in D:\\InspectionSync\\python_output_*.log and python_error_*.log"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return StatusCode(500, new
                {
                    message = "เกิดข้อผิดพลาด",
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace,
                    workingDir = _localWorkingDir
                });
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
                    }
                }
                catch { }
            }
        }

        private List<string> FormatProgressDisplay(List<ProgressEntry> progress)
        {
            if (progress == null || !progress.Any())
                return new List<string> { "No progress information" };

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
                display.Add($"{icon} [{entry.Step}] {entry.Message}");
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