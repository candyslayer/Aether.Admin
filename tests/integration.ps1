param([string]$BaseUrl = 'http://localhost:5080', [string]$TenantCode = 'default', [string]$OtherTenantCode = '')
$ErrorActionPreference = 'Stop'
$password = $env:AETHER_ADMIN_PASSWORD
if (-not $password) { throw 'Set AETHER_ADMIN_PASSWORD.' }
$script:checks = 0
function Call([string]$Method, [string]$Path, $Body = $null, [string]$Token = '', [int]$Expected = 200) {
    $headers = @{}
    if ($Token) { $headers.Authorization = "Bearer $Token" }
    $arguments = @{ Uri = "$BaseUrl$Path"; Method = $Method; Headers = $headers; SkipHttpErrorCheck = $true; NoProxy = $true }
    if ($null -ne $Body) { $arguments.Body = ConvertTo-Json $Body -Depth 20 -Compress; $arguments.ContentType = 'application/json' }
    $response = Invoke-WebRequest @arguments
    if ([int]$response.StatusCode -ne $Expected) { throw "$Method $Path expected $Expected, got $($response.StatusCode): $($response.Content)" }
    $script:checks++
    if ($response.Content) { return ($response.Content | ConvertFrom-Json).data }
}
function Assert($Condition, [string]$Message) { if (-not $Condition) { throw $Message }; $script:checks++ }
$admin = (Call POST /api/auth/login @{tenantCode=$TenantCode;username='admin';password=$password}).accessToken
Call GET /api/users $null '' 401 | Out-Null
$org = (Call GET /api/organizations $null $admin)[0]
if ($OtherTenantCode) {
    $other = Call POST /api/auth/login @{tenantCode=$OtherTenantCode;username='admin';password=$password}
    $otherToken = $other.accessToken
    $otherRoot = (Call GET /api/depts/tree $null $otherToken)[0]
    Call POST /api/depts @{name='Cross tenant';organizationId=$org.id;parentId=$otherRoot.id} $admin 400 | Out-Null
    $otherUsers = Call GET /api/users $null $otherToken
    Assert ($otherUsers.total -eq 1 -and $otherUsers.items[0].id -eq $other.userId) 'Tenant isolation failed.'
}
$root = (Call GET /api/depts/tree $null $admin)[0]
$suffix = [guid]::NewGuid().ToString('N').Substring(0,8)
$dept = Call POST /api/depts @{name="Engineering-$suffix";organizationId=$org.id;parentId=$root.id} $admin
$child = Call POST /api/depts @{name="Backend-$suffix";organizationId=$org.id;parentId=$dept.id} $admin
$outside = Call POST /api/depts @{name="Sales-$suffix";organizationId=$org.id;parentId=$root.id} $admin
Call DELETE "/api/depts/$($dept.id)" $null $admin 409 | Out-Null
Call PUT "/api/depts/$($dept.id)" @{name=$dept.name;organizationId=$org.id;parentId=$child.id;version=1} $admin 409 | Out-Null
$post = Call POST /api/posts @{name="Engineer-$suffix";code="ENGINEER_$suffix"} $admin
$role = Call POST /api/roles @{name="Employee-$suffix";code="EMPLOYEE_$suffix"} $admin
$menuTree = Call GET /api/menus $null $admin
function Flatten($nodes) { foreach ($n in $nodes) { $n; Flatten $n.children } }
$menus = @(Flatten $menuTree)
$listMenu = ($menus | Where-Object permissionCode -eq 'system:user:list')[0]
$updateMenu = ($menus | Where-Object permissionCode -eq 'system:user:update')[0]
$grants = @{menuIds=@($listMenu.id,$updateMenu.id);scopeType=5;departmentIds=@()}
Call PUT "/api/roles/$($role.id)/permissions" $grants $admin | Out-Null
$userPassword = 'IntegrationUser2026!'
$employee = Call POST /api/users @{username="employee_$suffix";password=$userPassword;nickname='Employee';deptId=$dept.id;roleIds=@($role.id);postIds=@($post.id)} $admin 201
$childUser = Call POST /api/users @{username="child_$suffix";password=$userPassword;nickname='Child';deptId=$child.id;roleIds=@();postIds=@()} $admin 201
$outsideUser = Call POST /api/users @{username="outside_$suffix";password=$userPassword;nickname='Outside';deptId=$outside.id;roleIds=@();postIds=@()} $admin 201
$token = (Call POST /api/auth/login @{tenantCode=$TenantCode;username=$employee.username;password=$userPassword}).accessToken
$self = Call GET /api/users $null $token
Assert ($self.total -eq 1 -and $self.items[0].id -eq $employee.id) 'SELF leaked data.'
Call POST /api/users @{username='forbidden';password=$userPassword;nickname='Forbidden';deptId=$dept.id;roleIds=@();postIds=@()} $token 403 | Out-Null
Call PUT "/api/users/$($outsideUser.id)" @{username=$outsideUser.username;nickname='Attack';deptId=$outside.id;roleIds=@();postIds=@();version=1} $token 403 | Out-Null
$grants.scopeType=3
Call PUT "/api/roles/$($role.id)/permissions" $grants $admin | Out-Null
$departmentData = Call GET /api/users $null $token
Assert ($departmentData.total -eq 1) 'DEPT leaked child or outside data.'
$grants.scopeType=4
Call PUT "/api/roles/$($role.id)/permissions" $grants $admin | Out-Null
$subtree = Call GET /api/users $null $token
Assert ($subtree.total -eq 2) 'DEPT_CHILD did not include descendants.'
$grants.scopeType=2; $grants.departmentIds=@($outside.id)
Call PUT "/api/roles/$($role.id)/permissions" $grants $admin | Out-Null
$custom = Call GET /api/users $null $token
Assert ($custom.total -eq 1 -and $custom.items[0].id -eq $outsideUser.id) 'CUSTOM scope failed.'
$grants.scopeType=1; $grants.departmentIds=@()
Call PUT "/api/roles/$($role.id)/permissions" $grants $admin | Out-Null
$all = Call GET /api/users $null $token
Assert ($all.total -ge 4) 'ALL scope failed.'
$adminUser = ($all.items | Where-Object username -eq 'admin')[0]
Call PUT "/api/users/$($adminUser.id)" @{username='admin';password='TakeoverPassword2026!';nickname='Administrator';deptId=$adminUser.deptId;roleIds=@($adminUser.roleIds);postIds=@($adminUser.postIds);version=$adminUser.version} $token 403 | Out-Null
$grants.menuIds=@()
Call PUT "/api/roles/$($role.id)/permissions" $grants $admin | Out-Null
Call GET /api/users $null $token 403 | Out-Null
$employeeUpdate = @{username=$employee.username;password='ChangedPassword2026!';nickname=$employee.nickname;deptId=$dept.id;roleIds=@($role.id);postIds=@($post.id);version=1}
Call PUT "/api/users/$($employee.id)" $employeeUpdate $admin | Out-Null
Call GET /api/auth/permissions $null $token 401 | Out-Null
Call POST /api/users @{username="invalid_$suffix";password=$userPassword;nickname='Invalid';deptId=[guid]::NewGuid().ToString();roleIds=@();postIds=@()} $admin 400 | Out-Null
Call PUT "/api/depts/$($outside.id)" @{name='Stale';organizationId=$org.id;parentId=$root.id;version=99} $admin 409 | Out-Null
$logs = Call GET /api/audit-logs $null $admin
Assert ($logs.total -gt 0) 'Audit did not persist.'
foreach ($u in @($employee,$childUser,$outsideUser)) { Call DELETE "/api/users/$($u.id)" $null $admin | Out-Null }
Call DELETE "/api/roles/$($role.id)" $null $admin | Out-Null
Call DELETE "/api/posts/$($post.id)" $null $admin | Out-Null
foreach ($d in @($child,$dept,$outside)) { Call DELETE "/api/depts/$($d.id)" $null $admin | Out-Null }
Write-Output "PASS: $script:checks HTTP and security assertions."
