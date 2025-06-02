### Creating User:

'''
$body = @{
username = "alice"
email = "alice@example.com"
password = "Secret123!"
}

Invoke-RestMethod -Uri http://localhost:5041/api/auth/signup -Method POST -Body ($body | ConvertTo-Json) -ContentType "application/json" -Verbose
'''
