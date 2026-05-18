$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5116/api"

$randomSuffix = Get-Random -Maximum 100000

$adminEmail = "admin$randomSuffix@test.com"
$docEmail = "doc$randomSuffix@test.com"
$patEmail = "pat$randomSuffix@test.com"

Write-Host "1. Registering Admin ($adminEmail)..."
$admin = Invoke-RestMethod -Uri "$baseUrl/Auth/register" -Method Post -Body @{ Email=$adminEmail; Password="Password123!"; ConfirmPassword="Password123!"; FullName="Admin User"; Role="Admin" }
$adminToken = $admin.token

Write-Host "2. Registering Doctor ($docEmail)..."
$doc = Invoke-RestMethod -Uri "$baseUrl/Auth/register" -Method Post -Body @{ Email=$docEmail; Password="Password123!"; ConfirmPassword="Password123!"; FullName="Doctor User"; Role="Doctor" }
$docToken = $doc.token

Write-Host "3. Registering Patient ($patEmail)..."
$pat = Invoke-RestMethod -Uri "$baseUrl/Auth/register" -Method Post -Body @{ Email=$patEmail; Password="Password123!"; ConfirmPassword="Password123!"; FullName="Patient User"; Role="Patient" }
$patToken = $pat.token

Write-Host "Fetching Doctor ID..."
$docs = Invoke-RestMethod -Uri "$baseUrl/Doctors" -Method Get -Headers @{ "Authorization" = "Bearer $patToken" }
$docId = $docs[-1].id # Getting the last created doctor
Write-Host "Doctor ID: $docId"

Write-Host "4. Patient books session..."
$bookBody = @{
    doctorId = $docId
    sessionDate = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")
    sessionType = "chat"
} | ConvertTo-Json
$bookRes = Invoke-RestMethod -Uri "$baseUrl/BookSession" -Method Post -Headers @{ "Authorization" = "Bearer $patToken"; "Content-Type" = "application/json" } -Body $bookBody
Write-Host ($bookRes | ConvertTo-Json)

Write-Host "5. Admin checks pending sessions..."
$pendingRes = Invoke-RestMethod -Uri "$baseUrl/AdminDashboard/pending-sessions" -Method Get -Headers @{ "Authorization" = "Bearer $adminToken" }
Write-Host ($pendingRes | ConvertTo-Json)

if ($pendingRes.Count -gt 0) {
    $sessionId = $pendingRes[-1].id
    Write-Host "6. Admin accepts session $sessionId..."
    $acceptRes = Invoke-RestMethod -Uri "$baseUrl/AdminDashboard/accept-session/$sessionId" -Method Post -Headers @{ "Authorization" = "Bearer $adminToken" }
    Write-Host ($acceptRes | ConvertTo-Json)

    $pendingResAfter = Invoke-RestMethod -Uri "$baseUrl/AdminDashboard/pending-sessions" -Method Get -Headers @{ "Authorization" = "Bearer $adminToken" }
    Write-Host "Pending sessions count after accept: $($pendingResAfter.Count)"
}
