<#
.SYNOPSIS
    Tự động gọi API cào dữ liệu qua AI Service cho ngày trước ngày hiện tại 1 ngày (T - 1).

.DESCRIPTION
    Script thực hiện tính toán ngày hôm qua (ngày hiện tại - 1 ngày) theo định dạng yyyy-MM-dd,
    sau đó gửi HTTP POST request tới AI Service endpoint /ingestion/vbpl/crawl để khởi chạy job cào dữ liệu.
    Có thể cấu hình chạy tự động định kỳ thông qua Windows Task Scheduler hoặc cron job.

.PARAMETER BaseUrl
    Địa chỉ base URL của AI Service (mặc định: http://127.0.0.1:8000).

.PARAMETER CrawlDate
    Ngày cần cào (định dạng yyyy-MM-dd). Nếu để trống, script sẽ tự động lấy ngày hôm qua.

.EXAMPLE
    # Tự động cào ngày hôm qua qua AI Service:
    .\scripts\crawl-daily.ps1

.EXAMPLE
    # Chỉ định ngày và endpoint tùy chỉnh:
    .\scripts\crawl-daily.ps1 -BaseUrl "http://localhost:8000" -CrawlDate "2026-11-16"

.NOTES
    Hướng dẫn tạo Windows Task Scheduler (chạy hàng ngày lúc 00:05 AM):
    Register-ScheduledTask -TaskName "Tillegal-AI-DailyCrawl" `
        -Action (New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$PWD\scripts\crawl-daily.ps1`"") `
        -Trigger (New-ScheduledTaskTrigger -Daily -At "00:05") `
        -Description "Tự động kích hoạt AI Service crawl dữ liệu cho ngày hôm qua"
#>

[CmdletBinding()]
param(
    [string]$BaseUrl = "http://127.0.0.1:8000",
    [string]$CrawlDate
)

# 1. Tính toán ngày cào dữ liệu (mặc định: ngày hiện tại - 1 ngày)
if ([string]::IsNullOrWhiteSpace($CrawlDate)) {
    $targetDate = (Get-Date).AddDays(-1).ToString("yyyy-MM-dd")
} else {
    $targetDate = $CrawlDate
}

$cleanBaseUrl = $BaseUrl.TrimEnd('/')
$endpointUri = "$cleanBaseUrl/ingestion/vbpl/crawl"
$payload = @{
    crawl_date = $targetDate
} | ConvertTo-Json -Compress

Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Bắt đầu kích hoạt crawl dữ liệu qua AI Service..." -ForegroundColor Cyan
Write-Host "  - Target Date : $targetDate (ngày trước hiện tại 1 ngày)"
Write-Host "  - Endpoint    : $endpointUri"
Write-Host "  - Payload     : $payload"

try {
    # 2. Gửi request POST kích hoạt job cào dữ liệu tới AI Service
    $job = Invoke-RestMethod -Method Post `
        -Uri $endpointUri `
        -ContentType "application/json" `
        -Body $payload

    # 3. Trả về kết quả và Job ID
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Kích hoạt job thành công!" -ForegroundColor Green
    if ($job.id) {
        Write-Host "  - Job ID: $($job.id)" -ForegroundColor Yellow
    }
    
    return $job
}
catch {
    Write-Error "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Lỗi khi gọi API cào dữ liệu từ AI Service: $_"
    exit 1
}
